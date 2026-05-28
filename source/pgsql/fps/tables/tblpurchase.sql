CREATE TABLE IF NOT EXISTS fps.tblpurchase (
    workgroup character varying(50) NOT NULL,
    account character varying(50) NOT NULL,
    itemdescription character varying(50) NOT NULL,
    amount money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblpurchase PRIMARY KEY (workgroup, account, itemdescription, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_default PARTITION OF fps.tblpurchase
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2016 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2017 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2018 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2019 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2020 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2021 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2022 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2023 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2024 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2025 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblpurchase_y2026 PARTITION OF fps.tblpurchase
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblpurchase
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_fpsyear'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_1'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_1 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2016(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_10'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_10 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2025(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_11'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_11 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2026(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_12'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_12 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_default(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_2'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_2 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2017(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_3'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_3 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2018(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_4'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_4 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2019(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_5'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_5 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2020(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_6'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_6 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2021(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_7'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_7 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2022(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_8'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_8 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2023(workgroup, account, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpurchase_workgroup_account_9'
          AND conrelid = 'fps.tblpurchase'::regclass
    ) THEN
        ALTER TABLE fps.tblpurchase
            ADD CONSTRAINT fk_tblpurchase_workgroup_account_9 FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid_y2024(workgroup, account, fpsyear);
    END IF;
END $$;
