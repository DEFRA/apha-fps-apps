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
BEGIN
    DROP TABLE IF EXISTS m2d_view_defs;
    DROP TABLE IF EXISTS m2d_target_tables;

    CREATE TEMP TABLE m2d_target_tables (
        table_oid oid PRIMARY KEY,
        schemaname text NOT NULL,
        tablename text NOT NULL
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
