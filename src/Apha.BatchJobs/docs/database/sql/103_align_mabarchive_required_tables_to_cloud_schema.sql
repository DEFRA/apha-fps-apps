-- Align required FPS/MABArchive tables to cloud schema reference
-- Source reference: database/sink/schema-reference/latest-cloud-schema-columns.csv
-- Scope: only required MABArchive process tables

BEGIN;

-- 1) Recreate fps.tblyearmaster to match cloud order/type/nullability
-- Cloud expectations from reference:
--   fpsyear (int, not null)
--   fpsyearcode (varchar, not null)
--   yearstatus (varchar, not null)
--   remarks (text, nullable)
--   active (boolean, not null)
--   createdon (timestamptz, not null)
--   createdby (varchar, nullable)

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'fps' AND table_name = 'tblyearmaster'
    ) THEN
        ALTER TABLE fps.tblyearmaster RENAME TO tblyearmaster_pre_cloud_align;

        -- Rename old constraints/index-backed names to avoid collisions on recreate.
        IF EXISTS (
            SELECT 1
            FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'fps'
              AND t.relname = 'tblyearmaster_pre_cloud_align'
              AND c.conname = 'pk_tblyearmaster'
        ) THEN
            ALTER TABLE fps.tblyearmaster_pre_cloud_align
                RENAME CONSTRAINT pk_tblyearmaster TO pk_tblyearmaster_pre_cloud_align;
        END IF;

        IF EXISTS (
            SELECT 1
            FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'fps'
              AND t.relname = 'tblyearmaster_pre_cloud_align'
              AND c.conname = 'uq_tblyearmaster_fpsyearcode'
        ) THEN
            ALTER TABLE fps.tblyearmaster_pre_cloud_align
                RENAME CONSTRAINT uq_tblyearmaster_fpsyearcode TO uq_tblyearmaster_fpsyearcode_pre_cloud_align;
        END IF;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS fps.tblyearmaster (
    fpsyear INTEGER NOT NULL,
    fpsyearcode VARCHAR(20) NOT NULL,
    yearstatus VARCHAR(10) NOT NULL,
    remarks TEXT,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    createdon TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    createdby VARCHAR(100),
    CONSTRAINT pk_tblyearmaster PRIMARY KEY (fpsyear),
    CONSTRAINT uq_tblyearmaster_fpsyearcode UNIQUE (fpsyearcode)
);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'fps' AND table_name = 'tblyearmaster_pre_cloud_align'
    ) THEN
        INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdon, createdby)
        SELECT
            fpsyear,
            fpsyearcode,
            COALESCE(yearstatus, 'Open') AS yearstatus,
            remarks,
            COALESCE(active, TRUE) AS active,
            COALESCE(createdon, NOW())::timestamptz AS createdon,
            createdby
        FROM fps.tblyearmaster_pre_cloud_align
        ON CONFLICT (fpsyear) DO NOTHING;

        DROP TABLE fps.tblyearmaster_pre_cloud_align;
    END IF;
END $$;

-- 2) Enforce NOT NULL on fpsyear for required source tables
ALTER TABLE fps.fpsyeartotals      ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.monthlyoutput      ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.monthlytime        ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.profitcentregrade  ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.proj_invoice       ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.proj_subcontract   ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.projectmonthfinal  ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tbladditionalcosts ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tblanimalreq       ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tblanimals         ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tblcontract        ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tblemployee        ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tblstaffjob        ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tblwgemployee      ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.testorproduct      ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.timecostcalcs      ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tlkpprogram        ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tlkpproject        ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.tlkptestreqmt      ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.workgroup          ALTER COLUMN fpsyear SET NOT NULL;
ALTER TABLE fps.workgroupgrade     ALTER COLUMN fpsyear SET NOT NULL;

-- 3) Align mabarchive.my_tblanimalreq sequence/default naming to cloud reference
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.sequences
        WHERE sequence_schema = 'mabarchive'
          AND sequence_name = 'my_tblanimalreq_AR_Counter_seq'
    ) AND NOT EXISTS (
        SELECT 1
        FROM information_schema.sequences
        WHERE sequence_schema = 'mabarchive'
          AND sequence_name = 'my_tblanimalreq_ar_counter_seq'
    ) THEN
        ALTER SEQUENCE mabarchive."my_tblanimalreq_AR_Counter_seq"
        RENAME TO my_tblanimalreq_ar_counter_seq;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.sequences
        WHERE sequence_schema = 'mabarchive'
          AND sequence_name = 'my_tblanimalreq_ar_counter_seq'
    ) THEN
        CREATE SEQUENCE mabarchive.my_tblanimalreq_ar_counter_seq;
    END IF;
END $$;

ALTER TABLE mabarchive.my_tblanimalreq
    ALTER COLUMN ar_counter SET DEFAULT nextval('mabarchive.my_tblanimalreq_ar_counter_seq'::regclass);

COMMIT;
