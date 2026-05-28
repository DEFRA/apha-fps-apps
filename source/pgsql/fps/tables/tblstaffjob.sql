CREATE TABLE IF NOT EXISTS fps.tblstaffjob (
    staffid character varying(50) NOT NULL,
    jobcode character varying(20) NOT NULL,
    plannedhours double precision DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblstaffjob PRIMARY KEY (staffid, jobcode, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_default PARTITION OF fps.tblstaffjob
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2016 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2017 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2018 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2019 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2020 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2021 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2022 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2023 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2024 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2025 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblstaffjob_y2026 PARTITION OF fps.tblstaffjob
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblstaffjob
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_fpsyear'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_1'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_1 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_10'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_10 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_11'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_11 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_12'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_12 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_2'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_2 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_3'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_3 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_4'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_4 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_5'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_5 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_6'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_6 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_7'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_7 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_8'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_8 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_jobcode_9'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_jobcode_9 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_1'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_1 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2016(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_10'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_10 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2025(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_11'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_11 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2026(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_12'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_12 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_default(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_2'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_2 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2017(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_3'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_3 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2018(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_4'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_4 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2019(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_5'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_5 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2020(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_6'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_6 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2021(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_7'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_7 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2022(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_8'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_8 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2023(pactid, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblstaffjob_staffid_9'
          AND conrelid = 'fps.tblstaffjob'::regclass
    ) THEN
        ALTER TABLE fps.tblstaffjob
            ADD CONSTRAINT fk_tblstaffjob_staffid_9 FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee_y2024(pactid, fpsyear);
    END IF;
END $$;
