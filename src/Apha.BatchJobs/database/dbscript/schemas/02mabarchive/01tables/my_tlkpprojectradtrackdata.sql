-- Table: mabarchive.my_tlkpprojectradtrackdata

CREATE TABLE mabarchive.my_tlkpprojectradtrackdata (
    year smallint NOT NULL,
    project character varying(20) NOT NULL,
    bfbudget money,
    pybudget money,
    seedcorn money,
    manhours double precision,
    mandays double precision,
    manyears double precision,
    paycosts money,
    nonpayohcosts money,
    testcosts money,
    animalcosts money,
    nonanimalcosts money,
    manhourschanged smallint DEFAULT 0,
    paycostschanged smallint DEFAULT 0,
    nonpayohcostschanged smallint DEFAULT 0,
    testcostschanged smallint DEFAULT 0,
    animalcostschanged smallint DEFAULT 0,
    nonanimalcostschanged smallint DEFAULT 0,
    adjustment money,
    adjustmentcomment character varying(250),
    locked smallint DEFAULT 0,
    datecosted timestamp without time zone,
    costedby character varying(20),
    actualexpenditure money,
    actualmanyears double precision,
    vla_budget money,
    CONSTRAINT pk_my_tlkpprojectradtrackdata PRIMARY KEY (year, project)
);

