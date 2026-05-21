CREATE TABLE IF NOT EXISTS fps.timecostcalcs (
    workgroup character varying(50) NOT NULL,
    jobcode character varying(50) NOT NULL,
    project character varying(20) NOT NULL,
    month double precision NOT NULL,
    staffid character varying(50) NOT NULL,
    gradecode character varying(10),
    name character varying(50),
    chargerate money,
    class character varying(255),
    time double precision,
    cost double precision,
    division character varying(10),
    jobcodeold character varying(14),
    pay money,
    nonpay money,
    overhead money,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_timecostcalcs PRIMARY KEY (workgroup, jobcode, project, month, staffid, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_default PARTITION OF fps.timecostcalcs
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2016 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2017 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2018 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2019 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2020 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2021 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2022 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2023 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2024 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2025 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.timecostcalcs_y2026 PARTITION OF fps.timecostcalcs
    FOR VALUES IN (2026);

-- Foreign keys for fps.timecostcalcs
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_timecostcalcs_fpsyear'
          AND conrelid = 'fps.timecostcalcs'::regclass
    ) THEN
        ALTER TABLE fps.timecostcalcs
            ADD CONSTRAINT fk_timecostcalcs_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
