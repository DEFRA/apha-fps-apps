CREATE TABLE IF NOT EXISTS fps.period_proj_subcontract (
    period smallint NOT NULL,
    subcontcounter integer NOT NULL,
    project character varying(20),
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    isdefraproject character varying(3) NOT NULL,
    opc character varying(50),
    occ double precision,
    month double precision,
    amount money,
    acctcode character varying(30),
    CONSTRAINT pk_period_proj_subcontract PRIMARY KEY (period, subcontcounter)
);
