--liquibase formatted sql

--changeset repo-admin:CR034 labels:ddl context:all ignore:true
--comment Convert all fps and mabarchive money columns to decimal(19,4) and safely rebuild dependent views.
DO $$
DECLARE
    v record;
    recreated_count integer;
    remaining_count integer;
BEGIN
    CREATE TEMP TABLE m2d_view_defs (
        schemaname text NOT NULL,
        viewname text NOT NULL,
        definition text NOT NULL
    ) ON COMMIT DROP;

    INSERT INTO m2d_view_defs (schemaname, viewname, definition)
    SELECT
        n.nspname AS schemaname,
        c.relname AS viewname,
        replace(
            replace(pg_get_viewdef(c.oid, true), '::"money"', '::decimal(19,4)'),
            '::money',
            '::decimal(19,4)'
        ) AS definition
    FROM pg_catalog.pg_class c
    INNER JOIN pg_catalog.pg_namespace n
        ON n.oid = c.relnamespace
    WHERE n.nspname IN ('fps', 'mabarchive')
      AND c.relkind = 'v';

    FOR v IN
        SELECT schemaname, viewname
        FROM m2d_view_defs
        ORDER BY schemaname, viewname
    LOOP
        EXECUTE format('DROP VIEW IF EXISTS %I.%I CASCADE', v.schemaname, v.viewname);
    END LOOP;

    FOR v IN
        SELECT
            n.nspname AS table_schema,
            c.relname AS table_name,
            string_agg(
                format(
                    'ALTER COLUMN %I TYPE decimal(19,4) USING %I::decimal(19,4)',
                    a.attname,
                    a.attname
                ),
                ', ' ORDER BY a.attnum
            ) AS alter_clauses
        FROM pg_catalog.pg_attribute a
        INNER JOIN pg_catalog.pg_class c
            ON c.oid = a.attrelid
        INNER JOIN pg_catalog.pg_namespace n
            ON n.oid = c.relnamespace
        INNER JOIN pg_catalog.pg_type t
            ON t.oid = a.atttypid
        WHERE n.nspname IN ('fps', 'mabarchive')
          AND c.relkind IN ('r', 'p')
          AND a.attnum > 0
          AND NOT a.attisdropped
          AND a.attislocal
          AND t.typname = 'money'
        GROUP BY n.nspname, c.relname
        ORDER BY n.nspname, c.relname
    LOOP
        EXECUTE format('ALTER TABLE %I.%I %s', v.table_schema, v.table_name, v.alter_clauses);
    END LOOP;

    LOOP
        recreated_count := 0;

        FOR v IN
            SELECT schemaname, viewname, definition
            FROM m2d_view_defs
            ORDER BY schemaname, viewname
        LOOP
            BEGIN
                EXECUTE format('CREATE OR REPLACE VIEW %I.%I AS %s', v.schemaname, v.viewname, v.definition);
                DELETE FROM m2d_view_defs
                WHERE schemaname = v.schemaname
                  AND viewname = v.viewname;
                recreated_count := recreated_count + 1;
            EXCEPTION
                WHEN OTHERS THEN
                    -- View depends on another view that is not recreated yet.
                    NULL;
            END;
        END LOOP;

        SELECT count(*) INTO remaining_count FROM m2d_view_defs;
        EXIT WHEN remaining_count = 0;

        IF recreated_count = 0 THEN
            RAISE EXCEPTION 'Failed to rebuild % views after money-to-decimal conversion. First pending view: %.%',
                remaining_count,
                (SELECT schemaname FROM m2d_view_defs ORDER BY schemaname, viewname LIMIT 1),
                (SELECT viewname FROM m2d_view_defs ORDER BY schemaname, viewname LIMIT 1);
        END IF;
    END LOOP;
END $$;

--rollback not required
