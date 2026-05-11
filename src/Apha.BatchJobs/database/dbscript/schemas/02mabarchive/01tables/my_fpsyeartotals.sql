-- Table: mabarchive.my_fpsyeartotals

CREATE TABLE mabarchive.my_fpsyeartotals (
    year smallint NOT NULL,
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
    projectstatus character varying(50) NOT NULL,
    pvsincome money,
    plancaseworkdebit money,
    totalpaycosts double precision,
    CONSTRAINT pk_my_fpsyeartotals PRIMARY KEY (year, parentproject)
);

