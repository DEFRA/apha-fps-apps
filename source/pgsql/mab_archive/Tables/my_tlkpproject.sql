-- Table: mabarchive.my_tlkpproject
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tlkpproject; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tlkpproject (
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
    datecreated timestamp without time zone,
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
-- Name: my_tlkpproject pk_my_tlkpproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tlkpproject
    ADD CONSTRAINT pk_my_tlkpproject PRIMARY KEY (year, parentproject);
-- Name: my_p_year; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX my_p_year ON mabarchive.my_tlkpproject USING btree (year);
