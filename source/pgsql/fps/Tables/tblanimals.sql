-- Table: fps.tblanimals
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblanimals; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblanimals (
    animaltype public.citext NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean DEFAULT false NOT NULL,
    defradailyrate money,
    fpsyear integer NOT NULL
);
-- Name: tblanimals pk_tblanimals; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimals
    ADD CONSTRAINT pk_tblanimals PRIMARY KEY (animaltype, fpsyear);
-- Name: tblanimals fk_tblanimals_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblanimals
    ADD CONSTRAINT fk_tblanimals_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
