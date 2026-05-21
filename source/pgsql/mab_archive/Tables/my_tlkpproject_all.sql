-- Table: mabarchive.my_tlkpproject_all
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tlkpproject_all; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tlkpproject_all (
    year smallint NOT NULL,
    parentproject character varying(20) NOT NULL,
    program character varying(10),
    customer character varying(50),
    manager character varying(50),
    transferincome money,
    custincome money,
    wip_eoy money,
    wip_limit money,
    wip_current money,
    projectstatus character varying(50),
    datecreated date,
    feccost money,
    profit money,
    budget_cvl money,
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    source character(5),
    disease character varying(50),
    contract character varying(10),
    finished smallint,
    comments text,
    carryover money,
    isdefraproject smallint,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    projectgroup character varying(50),
    incomeaccountcode character varying(50)
);
-- Name: my_tlkpproject_all pk_my_tlkpproject_all; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tlkpproject_all
    ADD CONSTRAINT pk_my_tlkpproject_all PRIMARY KEY (year, parentproject);
