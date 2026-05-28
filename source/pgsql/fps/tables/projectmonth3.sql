CREATE TABLE IF NOT EXISTS fps.projectmonth3 (
    endperiod double precision NOT NULL,
    periodname character varying(50),
    project character varying(20) NOT NULL,
    cumcost money,
    cuminvoices money,
    cumcoiw money,
    cumportsales double precision,
    cumprofile money,
    sumofcostprofile money,
    sumofmstonedue double precision,
    sumofdue__done double precision,
    sumofontime double precision,
    cumcwdebit money,
    cumcwcredit money,
    cumtotalhours double precision,
    cumsubcontracts double precision,
    cumtestcosts double precision,
    cumpaycosts double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_projectmonth3 PRIMARY KEY (endperiod, project, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_default PARTITION OF fps.projectmonth3
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2016 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2017 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2018 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2019 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2020 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2021 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2022 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2023 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2024 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2025 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.projectmonth3_y2026 PARTITION OF fps.projectmonth3
    FOR VALUES IN (2026);

-- Foreign keys for fps.projectmonth3
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_projectmonth3_fpsyear'
          AND conrelid = 'fps.projectmonth3'::regclass
    ) THEN
        ALTER TABLE fps.projectmonth3
            ADD CONSTRAINT fk_projectmonth3_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
