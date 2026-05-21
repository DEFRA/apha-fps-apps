CREATE TABLE IF NOT EXISTS fps.milestone (
    project character varying(20) NOT NULL,
    milestoneref character varying(4) NOT NULL,
    objectiveref character varying(50) NOT NULL,
    milsetonetitle character varying(120),
    plandate timestamp without time zone,
    actualdate timestamp without time zone,
    comment text,
    monthnofin double precision,
    year character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_milestone PRIMARY KEY (project, milestoneref, objectiveref, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.milestone_default PARTITION OF fps.milestone
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.milestone_y2016 PARTITION OF fps.milestone
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.milestone_y2017 PARTITION OF fps.milestone
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.milestone_y2018 PARTITION OF fps.milestone
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.milestone_y2019 PARTITION OF fps.milestone
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.milestone_y2020 PARTITION OF fps.milestone
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.milestone_y2021 PARTITION OF fps.milestone
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.milestone_y2022 PARTITION OF fps.milestone
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.milestone_y2023 PARTITION OF fps.milestone
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.milestone_y2024 PARTITION OF fps.milestone
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.milestone_y2025 PARTITION OF fps.milestone
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.milestone_y2026 PARTITION OF fps.milestone
    FOR VALUES IN (2026);

COMMENT ON TABLE fps.milestone IS 'Milestone information';
COMMENT ON COLUMN fps.milestone.project IS 'Project identifier';
COMMENT ON COLUMN fps.milestone.milestoneref IS 'Milestone reference';
COMMENT ON COLUMN fps.milestone.objectiveref IS 'Objective reference';
COMMENT ON COLUMN fps.milestone.milsetonetitle IS 'Milestone title';
COMMENT ON COLUMN fps.milestone.plandate IS 'Planned date';
COMMENT ON COLUMN fps.milestone.actualdate IS 'Actual date';
COMMENT ON COLUMN fps.milestone.comment IS 'Additional comments';
COMMENT ON COLUMN fps.milestone.monthnofin IS 'Month number (financial)';
COMMENT ON COLUMN fps.milestone.year IS 'Year';

-- Foreign keys for fps.milestone
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_fpsyear'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_1'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_1 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_10'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_10 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_11'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_11 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_12'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_12 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_2'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_2 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_3'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_3 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_4'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_4 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_5'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_5 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_6'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_6 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_7'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_7 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_8'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_8 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_milestone_project_9'
          AND conrelid = 'fps.milestone'::regclass
    ) THEN
        ALTER TABLE fps.milestone
            ADD CONSTRAINT fk_milestone_project_9 FOREIGN KEY (project, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
