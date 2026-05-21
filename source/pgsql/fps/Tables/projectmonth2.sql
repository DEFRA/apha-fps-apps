CREATE TABLE IF NOT EXISTS fps.projectmonth2 (
    project character varying(20) NOT NULL,
    monthno double precision NOT NULL,
    costprofile money,
    subcontracts money,
    animals money,
    nonanimal money,
    timecosts double precision,
    transfercosts double precision,
    totalcost money,
    invoices money,
    coiw money,
    sumofcostprofile money,
    portsales double precision,
    mstonedue integer,
    due__done double precision,
    ontime double precision,
    totalhours double precision,
    paycosts double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_projectmonth2 PRIMARY KEY (project, monthno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_default PARTITION OF fps.projectmonth2
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2016 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2017 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2018 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2019 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2020 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2021 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2022 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2023 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2024 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2025 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.projectmonth2_y2026 PARTITION OF fps.projectmonth2
    FOR VALUES IN (2026);

-- Foreign keys for fps.projectmonth2
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_projectmonth2_fpsyear'
          AND conrelid = 'fps.projectmonth2'::regclass
    ) THEN
        ALTER TABLE fps.projectmonth2
            ADD CONSTRAINT fk_projectmonth2_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
