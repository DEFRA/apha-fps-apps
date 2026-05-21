CREATE TABLE IF NOT EXISTS fps.tblbid (
    workgroup character varying(50) NOT NULL,
    account character varying(50) NOT NULL,
    genbid money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblbid PRIMARY KEY (workgroup, account, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblbid_default PARTITION OF fps.tblbid
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblbid_y2016 PARTITION OF fps.tblbid
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2017 PARTITION OF fps.tblbid
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2018 PARTITION OF fps.tblbid
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2019 PARTITION OF fps.tblbid
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2020 PARTITION OF fps.tblbid
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2021 PARTITION OF fps.tblbid
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2022 PARTITION OF fps.tblbid
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2023 PARTITION OF fps.tblbid
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2024 PARTITION OF fps.tblbid
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2025 PARTITION OF fps.tblbid
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblbid_y2026 PARTITION OF fps.tblbid
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblbid
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_1'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_1 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2016(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_10'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_10 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2025(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_11'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_11 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2026(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_12'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_12 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_default(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_2'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_2 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2017(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_3'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_3 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2018(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_4'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_4 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2019(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_5'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_5 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2020(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_6'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_6 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2021(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_7'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_7 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2022(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_8'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_8 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2023(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_account_9'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_account_9 FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory_y2024(accshortname, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_fpsyear'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_1'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_1 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2016(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_10'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_10 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2025(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_11'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_11 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2026(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_12'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_12 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_default(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_2'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_2 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2017(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_3'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_3 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2018(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_4'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_4 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2019(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_5'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_5 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2020(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_6'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_6 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2021(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_7'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_7 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2022(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_8'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_8 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2023(workgroup, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblbid_workgroup_9'
          AND conrelid = 'fps.tblbid'::regclass
    ) THEN
        ALTER TABLE fps.tblbid
            ADD CONSTRAINT fk_tblbid_workgroup_9 FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup_y2024(workgroup, fpsyear);
    END IF;
END $$;
