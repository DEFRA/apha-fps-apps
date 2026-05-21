-- Table: fps.tlkpdivision
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpdivision; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpdivision (
    divisionid integer,
    agencyid integer NOT NULL,
    divname public.citext NOT NULL,
    centoverhead money DEFAULT 0
);
-- Name: tlkpdivision pk__tlkpdivision__10566f31; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpdivision
    ADD CONSTRAINT pk__tlkpdivision__10566f31 PRIMARY KEY (divname);
-- Name: tlkpdivision fk_tlkpdivision_agencyid; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpdivision
    ADD CONSTRAINT fk_tlkpdivision_agencyid FOREIGN KEY (agencyid) REFERENCES fps.tlkpagency(agencyid);
