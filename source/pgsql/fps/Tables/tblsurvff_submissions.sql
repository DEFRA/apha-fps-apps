CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions (
    sd_pact_wg character varying(50) NOT NULL,
    contract character varying(20) NOT NULL,
    countofjobname integer,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblsurvff_submissions PRIMARY KEY (sd_pact_wg, contract, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_default PARTITION OF fps.tblsurvff_submissions
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2016 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2017 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2018 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2019 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2020 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2021 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2022 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2023 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2024 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2025 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.tblsurvff_submissions_y2026 PARTITION OF fps.tblsurvff_submissions
    FOR VALUES IN (2026);

-- Foreign keys for fps.tblsurvff_submissions
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblsurvff_submissions_fpsyear'
          AND conrelid = 'fps.tblsurvff_submissions'::regclass
    ) THEN
        ALTER TABLE fps.tblsurvff_submissions
            ADD CONSTRAINT fk_tblsurvff_submissions_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
