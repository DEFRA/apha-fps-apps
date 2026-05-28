CREATE TABLE IF NOT EXISTS fps.project_log (
    sequenceno integer GENERATED ALWAYS AS IDENTITY,
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200) NOT NULL,
    program character varying(10) NOT NULL,
    customer character varying(50) NOT NULL,
    manager character varying(50),
    transferincome money NOT NULL,
    custincome money NOT NULL,
    wip_eoy money,
    wip_limit money,
    wip_current money,
    projectstatus character varying(50) NOT NULL,
    costbookno character varying(50),
    datecreated timestamp without time zone,
    feccost money,
    profit money,
    budget_cvl money,
    datecosted timestamp without time zone,
    disease character varying(50) NOT NULL,
    contract character varying(10) NOT NULL,
    projectparent character varying(50),
    shorttitle character varying(30),
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    finished smallint,
    owningrc character varying(50),
    comments text,
    carryover money,
    carryoverseed money,
    date_time timestamp without time zone,
    user_id character varying(255),
    insert_delete character(2),
    jobcode character varying(20) NOT NULL,
    isdefraproject smallint,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    projectgroup character varying(50),
    incomeaccountcode character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_project_log PRIMARY KEY (sequenceno, fpsyear)
)
PARTITION BY LIST (fpsyear);

CREATE TABLE IF NOT EXISTS fps.project_log_default PARTITION OF fps.project_log
    DEFAULT;

CREATE TABLE IF NOT EXISTS fps.project_log_y2016 PARTITION OF fps.project_log
    FOR VALUES IN (2016);

CREATE TABLE IF NOT EXISTS fps.project_log_y2017 PARTITION OF fps.project_log
    FOR VALUES IN (2017);

CREATE TABLE IF NOT EXISTS fps.project_log_y2018 PARTITION OF fps.project_log
    FOR VALUES IN (2018);

CREATE TABLE IF NOT EXISTS fps.project_log_y2019 PARTITION OF fps.project_log
    FOR VALUES IN (2019);

CREATE TABLE IF NOT EXISTS fps.project_log_y2020 PARTITION OF fps.project_log
    FOR VALUES IN (2020);

CREATE TABLE IF NOT EXISTS fps.project_log_y2021 PARTITION OF fps.project_log
    FOR VALUES IN (2021);

CREATE TABLE IF NOT EXISTS fps.project_log_y2022 PARTITION OF fps.project_log
    FOR VALUES IN (2022);

CREATE TABLE IF NOT EXISTS fps.project_log_y2023 PARTITION OF fps.project_log
    FOR VALUES IN (2023);

CREATE TABLE IF NOT EXISTS fps.project_log_y2024 PARTITION OF fps.project_log
    FOR VALUES IN (2024);

CREATE TABLE IF NOT EXISTS fps.project_log_y2025 PARTITION OF fps.project_log
    FOR VALUES IN (2025);

CREATE TABLE IF NOT EXISTS fps.project_log_y2026 PARTITION OF fps.project_log
    FOR VALUES IN (2026);

-- Foreign keys for fps.project_log
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_project_log_fpsyear'
          AND conrelid = 'fps.project_log'::regclass
    ) THEN
        ALTER TABLE fps.project_log
            ADD CONSTRAINT fk_project_log_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
    END IF;
END $$;
