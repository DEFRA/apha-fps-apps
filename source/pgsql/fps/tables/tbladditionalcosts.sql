CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts (
    jobcode character varying(20) NOT NULL,
    account character varying(50) NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money DEFAULT 0 NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbladditionalcosts PRIMARY KEY (jobcode, account, description, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_default PARTITION OF fps.tbladditionalcosts
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2016 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2017 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2018 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2019 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2020 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2021 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2022 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2023 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2024 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2025 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tbladditionalcosts_y2026 PARTITION OF fps.tbladditionalcosts
    FOR VALUES IN (2026);

-- Foreign keys for fps.tbladditionalcosts
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_1'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_1 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2016(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_10'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_10 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2025(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_11'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_11 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2026(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_12'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_12 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_default(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_2'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_2 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2017(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_3'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_3 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2018(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_4'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_4 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2019(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_5'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_5 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2020(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_6'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_6 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2021(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_7'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_7 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2022(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_8'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_8 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2023(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_account_9'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_account_9 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2024(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_fpsyear'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_1'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_1 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2016(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_10'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_10 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2025(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_11'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_11 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2026(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_12'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_12 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_default(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_2'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_2 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2017(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_3'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_3 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2018(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_4'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_4 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2019(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_5'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_5 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2020(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_6'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_6 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2021(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_7'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_7 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2022(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_8'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_8 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2023(parentproject, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbladditionalcosts_jobcode_9'
          AND conrelid = 'fps.tbladditionalcosts'::regclass
    ) THEN
        ALTER TABLE fps.tbladditionalcosts
            ADD CONSTRAINT fk_tbladditionalcosts_jobcode_9 FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject_y2024(parentproject, fpsyear);
    END IF;
END $$;
