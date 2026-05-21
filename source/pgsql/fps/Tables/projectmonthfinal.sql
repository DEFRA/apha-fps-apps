CREATE TABLE IF NOT EXISTS fps.projectmonthfinal (
    project character varying(20) NOT NULL,
    monthno double precision NOT NULL,
    periodname character varying(50),
    cumflag double precision,
    costprofile money,
    subcontracts money,
    animals money,
    nonanimals money,
    timecosts money,
    transfercosts money,
    totalcost money,
    invoices money,
    coiw money,
    portsales money,
    cumcost money,
    cumprofile money,
    sumofcostprofile money,
    cuminvoices money,
    cumcoiw money,
    cumportsales money,
    mstonedue integer,
    due__done double precision,
    ontime double precision,
    sumofmstonedue double precision,
    sumofdue__done double precision,
    sumofontime double precision,
    cwdebit money,
    cwcredit money,
    cumcwdebit money,
    cumcwcredit money,
    totalhours double precision,
    cumtotalhours double precision,
    cumsubcontracts double precision,
    x integer,
    cumtestcosts double precision,
    paycosts double precision,
    cumpaycosts double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_projectmonthfinal PRIMARY KEY (project, monthno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_default PARTITION OF fps.projectmonthfinal
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2016 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2017 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2018 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2019 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2020 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2021 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2022 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2023 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2024 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2025 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.projectmonthfinal_y2026 PARTITION OF fps.projectmonthfinal
    FOR VALUES IN (2026);

-- Foreign keys for fps.projectmonthfinal
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_projectmonthfinal_fpsyear'
          AND conrelid = 'fps.projectmonthfinal'::regclass
    ) THEN
        ALTER TABLE fps.projectmonthfinal
            ADD CONSTRAINT fk_projectmonthfinal_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
