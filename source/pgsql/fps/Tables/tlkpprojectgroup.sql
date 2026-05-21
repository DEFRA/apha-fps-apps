-- Table: fps.tlkpprojectgroup
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpprojectgroup; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpprojectgroup (
    projectgroup public.citext NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tlkpprojectgroup pk_tlkpprojectgroup; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpprojectgroup
    ADD CONSTRAINT pk_tlkpprojectgroup PRIMARY KEY (projectgroup, fpsyear);
-- Name: tlkpprojectgroup fk_tlkpprojectgroup_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpprojectgroup
    ADD CONSTRAINT fk_tlkpprojectgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
