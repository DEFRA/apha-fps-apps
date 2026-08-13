--liquibase formatted sql

--changeset repo-admin:CR034 labels:ddl context:all splitStatements:false runOnChange:true
--comment Convert every money column in the fps and mabarchive schemas to numeric(19,4) and rebuild all dependent views in one self-contained, idempotent pass.
-- Remove any helper procedures left behind by earlier iterations of this changeset.
DROP PROCEDURE IF EXISTS public._m2d_convert_money_batch(text, text[]);
DROP PROCEDURE IF EXISTS public._m2d_convert_money_all(text[]);

DO $do$
DECLARE
    p_schemas text[] := ARRAY['fps', 'mabarchive'];
    rec record;
    view_rec record;
    recreate_sql text;
BEGIN
    DROP TABLE IF EXISTS m2d_view_defs;
    DROP TABLE IF EXISTS m2d_target_tables;
    DROP TABLE IF EXISTS m2d_dependent_views;

    CREATE TEMP TABLE m2d_target_tables (
        table_oid oid PRIMARY KEY,
        schemaname text NOT NULL,
        tablename text NOT NULL
    ) ON COMMIT DROP;

    CREATE TEMP TABLE m2d_dependent_views (
        view_oid oid PRIMARY KEY,
        schemaname text NOT NULL,
        viewname text NOT NULL,
        view_definition text NOT NULL,
        depth integer NOT NULL
    ) ON COMMIT DROP;

    -- Discover every base table (including partitioned parents) in the target
    -- schemas that still has at least one money column. Individual partitions
    -- inherit the parent's ALTER, so they are excluded here.
    INSERT INTO m2d_target_tables (table_oid, schemaname, tablename)
    SELECT DISTINCT
        c.oid,
        n.nspname,
        c.relname
    FROM pg_catalog.pg_class c
    INNER JOIN pg_catalog.pg_namespace n
        ON n.oid = c.relnamespace
    WHERE n.nspname = ANY(p_schemas)
      AND c.relkind IN ('r', 'p')
      AND NOT c.relispartition
      AND EXISTS (
          SELECT 1
          FROM pg_catalog.pg_attribute a
          INNER JOIN pg_catalog.pg_type t
              ON t.oid = a.atttypid
          WHERE a.attrelid = c.oid
            AND a.attnum > 0
            AND NOT a.attisdropped
            AND t.typname = 'money'
      );

    IF NOT EXISTS (SELECT 1 FROM m2d_target_tables) THEN
        RAISE NOTICE 'No money columns found in schemas %', p_schemas;
        RETURN;
    END IF;

    -- Find all views that depend on target tables (directly or transitively,
    -- including views built on top of other dependent views).
    -- View dependencies are recorded via pg_rewrite rules, so we must join
    -- pg_depend -> pg_rewrite -> pg_class (ev_class) to reach the actual view.
    INSERT INTO m2d_dependent_views (view_oid, schemaname, viewname, view_definition, depth)
    WITH RECURSIVE view_deps AS (
        -- Base: views that directly depend on a target table
        SELECT DISTINCT
            v.oid AS view_oid,
            1 AS depth
        FROM m2d_target_tables t
        INNER JOIN pg_catalog.pg_depend d
            ON d.refobjid = t.table_oid
            AND d.refclassid = 'pg_class'::regclass
            AND d.classid = 'pg_rewrite'::regclass
        INNER JOIN pg_catalog.pg_rewrite rw
            ON rw.oid = d.objid
        INNER JOIN pg_catalog.pg_class v
            ON v.oid = rw.ev_class
            AND v.relkind = 'v'
        WHERE v.oid <> t.table_oid

        UNION ALL

        -- Recursive: views that depend on already-discovered views
        SELECT DISTINCT
            v.oid AS view_oid,
            vd.depth + 1
        FROM view_deps vd
        INNER JOIN pg_catalog.pg_depend d
            ON d.refobjid = vd.view_oid
            AND d.refclassid = 'pg_class'::regclass
            AND d.classid = 'pg_rewrite'::regclass
        INNER JOIN pg_catalog.pg_rewrite rw
            ON rw.oid = d.objid
        INNER JOIN pg_catalog.pg_class v
            ON v.oid = rw.ev_class
            AND v.relkind = 'v'
        WHERE v.oid <> vd.view_oid
    )
    SELECT
        vd.view_oid,
        vn.nspname,
        v.relname,
        pg_get_viewdef(vd.view_oid),
        MAX(vd.depth) AS depth
    FROM view_deps vd
    INNER JOIN pg_catalog.pg_class v
        ON v.oid = vd.view_oid
    INNER JOIN pg_catalog.pg_namespace vn
        ON vn.oid = v.relnamespace
    GROUP BY vd.view_oid, vn.nspname, v.relname;

    -- Drop dependent views deepest-first so that views built on other views
    -- are removed before the views they reference.
    FOR view_rec IN
        SELECT schemaname, viewname
        FROM m2d_dependent_views
        ORDER BY depth DESC, view_oid DESC
    LOOP
        EXECUTE format('DROP VIEW IF EXISTS %I.%I CASCADE', view_rec.schemaname, view_rec.viewname);
        RAISE NOTICE 'Dropped view %.%', view_rec.schemaname, view_rec.viewname;
    END LOOP;

    -- Convert every money column in every target table BEFORE recreating any
    -- view, so that views spanning multiple tables never mix money and numeric.
    FOR rec IN
        SELECT
            schemaname,
            tablename,
            string_agg(
                format(
                    'ALTER COLUMN %I TYPE numeric(19,4) USING %I::numeric(19,4)',
                    a.attname,
                    a.attname
                ),
                ', ' ORDER BY a.attnum
            ) AS alter_clauses
        FROM m2d_target_tables t
        INNER JOIN pg_catalog.pg_attribute a
            ON a.attrelid = t.table_oid
        INNER JOIN pg_catalog.pg_type ty
            ON ty.oid = a.atttypid
        WHERE a.attnum > 0
          AND NOT a.attisdropped
          AND ty.typname = 'money'
        GROUP BY schemaname, tablename
        ORDER BY schemaname, tablename
    LOOP
        EXECUTE format('ALTER TABLE %I.%I %s', rec.schemaname, rec.tablename, rec.alter_clauses);
        RAISE NOTICE 'Converted money columns in %.%', rec.schemaname, rec.tablename;
    END LOOP;

    -- Recreate dependent views shallowest-first so base views exist before
    -- the views that reference them.
    -- pg_get_viewdef freezes any implicit money coercions into the definition
    -- text (e.g. a CASE/COALESCE literal becomes '(0)::money'). Since the
    -- underlying columns are now numeric(19,4), those frozen '::money' casts
    -- would fail to unify. Rewrite them to '::numeric(19,4)' so every branch
    -- matches the converted columns.
    FOR view_rec IN
        SELECT schemaname, viewname, view_definition
        FROM m2d_dependent_views
        ORDER BY depth ASC, view_oid ASC
    LOOP
        recreate_sql := format(
            'CREATE VIEW %I.%I AS %s',
            view_rec.schemaname,
            view_rec.viewname,
            replace(view_rec.view_definition, '::money', '::numeric(19,4)')
        );
        EXECUTE recreate_sql;
        RAISE NOTICE 'Recreated view %.%', view_rec.schemaname, view_rec.viewname;
    END LOOP;

END $do$;

--rollback not required
