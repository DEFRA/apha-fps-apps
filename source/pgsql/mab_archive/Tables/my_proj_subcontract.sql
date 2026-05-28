CREATE TABLE IF NOT EXISTS mabarchive.my_proj_subcontract (
    year smallint NOT NULL,
    subcontcounter integer NOT NULL,
    project character varying(20),
    testjob character varying(50),
    month double precision,
    amount money,
    workgroup character varying(50),
    acctcode character varying(30),
    supplier character varying(50),
    description character varying(255),
    suppliernumber integer,
    dailyrate money,
    animaldays integer,
    CONSTRAINT pk_my_proj_subcontract PRIMARY KEY (year, subcontcounter)
);
