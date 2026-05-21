-- Table: fps.projectmonth2
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: projectmonth2; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.projectmonth2 (
    project character varying(20) NOT NULL,
    monthno double precision NOT NULL,
    costprofile money,
    subcontracts money,
    animals money,
    nonanimal money,
    timecosts double precision,
    transfercosts double precision,
    totalcost money,
    invoices money,
    coiw money,
    sumofcostprofile money,
    portsales double precision,
    mstonedue integer,
    due__done double precision,
    ontime double precision,
    totalhours double precision,
    paycosts double precision,
    fpsyear integer NOT NULL
);
-- Name: projectmonth2 pk_projectmonth2; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonth2
    ADD CONSTRAINT pk_projectmonth2 PRIMARY KEY (project, monthno, fpsyear);
-- Name: projectmonth2 fk_projectmonth2_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonth2
    ADD CONSTRAINT fk_projectmonth2_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
