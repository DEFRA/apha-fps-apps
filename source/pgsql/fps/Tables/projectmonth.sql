-- Table: fps.projectmonth
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: projectmonth; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.projectmonth (
    project character varying(20) NOT NULL,
    monthno integer NOT NULL,
    costprofile money,
    fpsyear integer NOT NULL
);
-- Name: projectmonth pk_projectmonth; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonth
    ADD CONSTRAINT pk_projectmonth PRIMARY KEY (project, monthno, fpsyear);
-- Name: projectmonth fk_projectmonth_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonth
    ADD CONSTRAINT fk_projectmonth_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
