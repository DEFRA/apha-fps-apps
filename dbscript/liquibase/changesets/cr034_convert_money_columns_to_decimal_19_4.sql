--liquibase formatted sql

--changeset repo-admin:CR034 labels:ddl context:all splitStatements:false runOnChange:true
--comment Create a reusable helper that converts one batch of money tables and rebuilds only views that depend on that batch.
CREATE OR REPLACE PROCEDURE public._m2d_convert_money_batch(
    p_schema text,
    p_roots text[]
)
LANGUAGE plpgsql
AS $procedure$
DECLARE
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

    INSERT INTO m2d_target_tables (table_oid, schemaname, tablename)
    WITH roots AS (
        SELECT unnest(p_roots) AS root_name
    )
    SELECT DISTINCT
        c.oid,
        n.nspname,
        c.relname
    FROM roots r
    INNER JOIN pg_catalog.pg_class c
        ON c.relkind IN ('r', 'p')
    INNER JOIN pg_catalog.pg_namespace n
        ON n.oid = c.relnamespace
    WHERE n.nspname = p_schema
            AND NOT c.relispartition
      AND (
          c.relname = r.root_name
          OR c.relname = r.root_name || '_default'
          OR c.relname ~ ('^' || r.root_name || '_y[0-9]{4}$')
      )
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
        RAISE NOTICE 'No money columns found for schema % and roots %', p_schema, p_roots;
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

    -- Alter column types
    FOR rec IN
        SELECT
            schemaname,
            tablename,
            string_agg(
                format(
                    'ALTER COLUMN %I TYPE decimal(19,4) USING %I::decimal(19,4)',
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
    FOR view_rec IN
        SELECT schemaname, viewname, view_definition
        FROM m2d_dependent_views
        ORDER BY depth ASC, view_oid ASC
    LOOP
        recreate_sql := format('CREATE VIEW %I.%I AS %s', view_rec.schemaname, view_rec.viewname, view_rec.view_definition);
        EXECUTE recreate_sql;
        RAISE NOTICE 'Recreated view %.%', view_rec.schemaname, view_rec.viewname;
    END LOOP;

END;
$procedure$;

--changeset repo-admin:CR034_01 labels:ddl context:all
--comment Convert operational fps tables with money columns in a smaller transactional batch.
CALL public._m2d_convert_money_batch(
    'fps',
    ARRAY[
        'additionalcosts_log',
        'tbladditionalcosts',
        'tblanimals',
        'tblbid',
        'tblpurchase',
        'tblstagingpurchaselocal',
        'tblsurvff_fees',
        'tbltestrccost',
        'tbltestreqbaseline',
        'tbltestrequirementrccost'
    ]
);

--changeset repo-admin:CR034_02 labels:ddl context:all
--comment Convert monthly and subcontract fps tables with money columns in a smaller transactional batch.
CALL public._m2d_convert_money_batch(
    'fps',
    ARRAY[
        'period_monthlyoutput',
        'period_proj_subcontract',
        'period_timecostcalcs',
        'proj_invoice',
        'proj_subcontract',
        'resourcecentremonth',
        'timecostcalcs'
    ]
);

--changeset repo-admin:CR034_03 labels:ddl context:all
--comment Convert fps reference and rate tables with money columns in a smaller transactional batch.
CALL public._m2d_convert_money_batch(
    'fps',
    ARRAY[
        'divisiongrade',
        'grade',
        'profitcentregrade',
        'profitcentregrade_nondefra',
        'testorproduct',
        'tlkpdivision',
        'tlkpprogram',
        'tlkptestcapability',
        'tblkpprofitcentre',
        'workgroup',
        'workgroupgrade'
    ]
);

--changeset repo-admin:CR034_04 labels:ddl context:all
--comment Convert fps project and totals tables with money columns in a smaller transactional batch.
CALL public._m2d_convert_money_batch(
    'fps',
    ARRAY[
        'fpsyeartotals',
        'project_log',
        'projectmonth',
        'projectmonth2',
        'projectmonth3',
        'projectmonthfinal',
        'tblcbsummary',
        'tblpostmortem1report',
        'tbltotalbusinessoverheads',
        'tlkpproject',
        'tlkptestreqmt',
        'workgroupmonth'
    ]
);

--changeset repo-admin:CR034_05 labels:ddl context:all
--comment Convert mabarchive tables with money columns in a smaller transactional batch.
CALL public._m2d_convert_money_batch(
    'mabarchive',
    ARRAY[
        'g_tlkpproject_radtrackdata',
        'my_fpsyeartotals',
        'my_profitcentregrade',
        'my_proj_invoice',
        'my_proj_subcontract',
        'my_projectmonthfinal',
        'my_tbladditionalcosts',
        'my_tblanimals',
        'my_tblprofitcentre',
        'my_testorproduct',
        'my_timecostcalcs',
        'my_tlkpprogram',
        'my_tlkpproject',
        'my_tlkpproject_all',
        'my_tlkpprojectradtrackdata',
        'my_tlkptestreqmt',
        'my_workgroup'
    ]
);

--changeset repo-admin:CR034_cleanup labels:ddl context:all
--comment Remove the temporary CR034 helper procedure once all batches are complete.
DROP PROCEDURE IF EXISTS public._m2d_convert_money_batch(text, text[]);

--rollback not required
