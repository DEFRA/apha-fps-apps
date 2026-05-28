CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule (
    contract character varying(10) NOT NULL,
    duedate timestamp without time zone NOT NULL,
    paid smallint NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblpaymentschedule PRIMARY KEY (contract, duedate, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_default PARTITION OF fps.tblpaymentschedule
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2016 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2017 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2018 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2019 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2020 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2021 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2022 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2023 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2024 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2025 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblpaymentschedule_y2026 PARTITION OF fps.tblpaymentschedule
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblpaymentschedule
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_1'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_1 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2016(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_10'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_10 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2025(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_11'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_11 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2026(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_12'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_12 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_default(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_2'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_2 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2017(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_3'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_3 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2018(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_4'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_4 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2019(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_5'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_5 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2020(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_6'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_6 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2021(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_7'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_7 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2022(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_8'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_8 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2023(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_contract_9'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_contract_9 FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract_y2024(contractno, fpsyear);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpaymentschedule_fpsyear'
          AND conrelid = 'fps.tblpaymentschedule'::regclass
    ) THEN
        ALTER TABLE fps.tblpaymentschedule
            ADD CONSTRAINT fk_tblpaymentschedule_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
