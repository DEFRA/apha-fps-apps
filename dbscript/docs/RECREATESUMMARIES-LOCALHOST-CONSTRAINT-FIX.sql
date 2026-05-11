-- RecreateSummaries local constraint fix for missing table constraints
-- Target DB: batch_jobs_foundation_db
-- Source of truth: dbscript/schemas/01fps/01tables/{milestone,timecodevalid,tlkptestcapability}.sql

BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'milestone'
          AND co.conname = 'pk_milestone_1__12'
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT pk_milestone_1__12 PRIMARY KEY (project, milestoneref, objectiveref);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'milestone'
          AND co.conname = 'fk_milestone_project'
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project FOREIGN KEY (fpsyear, project)
            REFERENCES fps.tlkpproject(fpsyear, parentproject);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'timecodevalid'
          AND co.conname = 'aaaaatimecodevalid_pk'
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT aaaaatimecodevalid_pk PRIMARY KEY (workgroup, timecode, parentproject);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'timecodevalid'
          AND co.conname = 'fk_timecodevalid_parentproject'
    ) THEN
        ALTER TABLE fps.timecodevalid
            ADD CONSTRAINT fk_timecodevalid_parentproject FOREIGN KEY (fpsyear, parentproject)
            REFERENCES fps.tlkpproject(fpsyear, parentproject);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'tlkptestcapability'
          AND co.conname = 'pk__tlkptestcapabili__4e53a1aa'
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT pk__tlkptestcapabili__4e53a1aa PRIMARY KEY (testcode, workgroup);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'tlkptestcapability'
          AND co.conname = 'fk_tlkptestcapability_1__15'
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_1__15 FOREIGN KEY (fpsyear, workgroup)
            REFERENCES fps.workgroup(fpsyear, workgroup);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'tlkptestcapability'
          AND co.conname = 'fk_tlkptestcapability_1__18'
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_1__18 FOREIGN KEY (fpsyear, planportfolio)
            REFERENCES fps.tlkpproject(fpsyear, parentproject);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint co
        JOIN pg_class c ON c.oid = co.conrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        WHERE n.nspname = 'fps'
          AND c.relname = 'tlkptestcapability'
          AND co.conname = 'fk_tlkptestcapability_2__18'
    ) THEN
        ALTER TABLE fps.tlkptestcapability
            ADD CONSTRAINT fk_tlkptestcapability_2__18 FOREIGN KEY (fpsyear, testcode)
            REFERENCES fps.testorproduct(fpsyear, itemcode);
    END IF;
END $$;

COMMIT;
