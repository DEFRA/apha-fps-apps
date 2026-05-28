CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees (
    pactcode character varying(50) NOT NULL,
    owning_vic character varying(50) NOT NULL,
    received timestamp without time zone,
    contract character varying(20) NOT NULL,
    record_id character varying(20) NOT NULL,
    volume double precision,
    totalfee money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblsurvff_fees PRIMARY KEY (owning_vic, contract, record_id, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_default PARTITION OF fps.tblsurvff_fees
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2016 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2017 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2018 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2019 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2020 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2021 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2022 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2023 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2024 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2025 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_fees_y2026 PARTITION OF fps.tblsurvff_fees
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblsurvff_fees
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblsurvff_fees_fpsyear'
          AND conrelid = 'fps.tblsurvff_fees'::regclass
    ) THEN
        ALTER TABLE fps.tblsurvff_fees
            ADD CONSTRAINT fk_tblsurvff_fees_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
