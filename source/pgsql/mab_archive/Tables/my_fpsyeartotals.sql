-- Table: mabarchive.my_fpsyeartotals
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_fpsyeartotals; Type: TABLE; Schema: mabarchive; Owner: -
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
    totalpaycosts double precision
);
-- Name: my_fpsyeartotals pk_my_fpsyeartotals; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_fpsyeartotals
    ADD CONSTRAINT pk_my_fpsyeartotals PRIMARY KEY (year, parentproject);
