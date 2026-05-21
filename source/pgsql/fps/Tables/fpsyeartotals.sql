CREATE TABLE IF NOT EXISTS fps.fpsyeartotals (
    parentproject character varying(20) NOT NULL,
    program character varying(10) NOT NULL,
    totaladditionalcosts money,
    totalanimalcosts double precision,
    totalstaffcosts double precision,
    totaltestcosts double precision,
    totalcosts double precision,
    custincome money NOT NULL,
    transferincome money NOT NULL,
    totalincome money NOT NULL,
    budget_cvl money,
    requiredprofit money,
    manager character varying(50),
    customer character varying(50),
    projectstatus character varying(50),
    pvsincome money,
    plancaseworkdebit money,
    totalpaycosts double precision,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_fpsyeartotals PRIMARY KEY (parentproject, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_default PARTITION OF fps.fpsyeartotals
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2016 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2017 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2018 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2019 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2020 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2021 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2022 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2023 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2024 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2025 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.fpsyeartotals_y2026 PARTITION OF fps.fpsyeartotals
    FOR VALUES IN (2026);

-- Foreign keys for fps.fpsyeartotals
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_fpsyeartotals_fpsyear'
          AND conrelid = 'fps.fpsyeartotals'::regclass
    ) THEN
        ALTER TABLE fps.fpsyeartotals
            ADD CONSTRAINT fk_fpsyeartotals_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
