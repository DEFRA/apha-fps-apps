-- Table: fps.projectmonthfinal
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: projectmonthfinal; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.projectmonthfinal (
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
    x integer,
    cumtestcosts double precision,
    paycosts double precision,
    cumpaycosts double precision,
    fpsyear integer NOT NULL
);
-- Name: projectmonthfinal pk_projectmonthfinal; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonthfinal
    ADD CONSTRAINT pk_projectmonthfinal PRIMARY KEY (project, monthno, fpsyear);
-- Name: projectmonthfinal fk_projectmonthfinal_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonthfinal
    ADD CONSTRAINT fk_projectmonthfinal_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
