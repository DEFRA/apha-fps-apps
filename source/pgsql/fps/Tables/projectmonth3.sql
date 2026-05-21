-- Table: fps.projectmonth3
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: projectmonth3; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.projectmonth3 (
    endperiod double precision NOT NULL,
    periodname character varying(50),
    project character varying(20) NOT NULL,
    cumcost money,
    cuminvoices money,
    cumcoiw money,
    cumportsales double precision,
    cumprofile money,
    sumofcostprofile money,
    sumofmstonedue double precision,
    sumofdue__done double precision,
    sumofontime double precision,
    cumcwdebit money,
    cumcwcredit money,
    cumtotalhours double precision,
    cumsubcontracts double precision,
    cumtestcosts double precision,
    cumpaycosts double precision,
    fpsyear integer NOT NULL
);
-- Name: projectmonth3 pk_projectmonth3; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonth3
    ADD CONSTRAINT pk_projectmonth3 PRIMARY KEY (endperiod, project, fpsyear);
-- Name: projectmonth3 fk_projectmonth3_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonth3
    ADD CONSTRAINT fk_projectmonth3_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
