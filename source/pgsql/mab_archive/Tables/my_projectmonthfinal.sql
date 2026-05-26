-- Table: mabarchive.my_projectmonthfinal
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_projectmonthfinal; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_projectmonthfinal (
    year smallint NOT NULL,
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
    cumtestcosts double precision,
    paycosts double precision,
    cumpaycosts double precision
);
-- Name: my_projectmonthfinal pk_my_projectmonthfinal; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_projectmonthfinal
    ADD CONSTRAINT pk_my_projectmonthfinal PRIMARY KEY (year, project, monthno);
