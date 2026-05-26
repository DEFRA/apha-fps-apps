-- Table: fps.tbluser_projectgroup
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbluser_projectgroup; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbluser_projectgroup (
    user_id integer NOT NULL,
    projectgroup character varying(50) NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbluser_projectgroup pk_tbluser_projectgroup; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_projectgroup
    ADD CONSTRAINT pk_tbluser_projectgroup PRIMARY KEY (projectgroup, user_id, fpsyear);
-- Name: tbluser_projectgroup fk_tbluser_projectgroup_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_projectgroup
    ADD CONSTRAINT fk_tbluser_projectgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
