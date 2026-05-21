-- Table: fps.tbluser_program
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbluser_program; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbluser_program (
    user_id integer NOT NULL,
    programno character varying(10) NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbluser_program pk_tbluser_program; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_program
    ADD CONSTRAINT pk_tbluser_program PRIMARY KEY (programno, user_id, fpsyear);
-- Name: xif84tbluser_program; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX xif84tbluser_program ON fps.tbluser_program USING btree (programno);
-- Name: tbluser_program fk_tbluser_program_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_program
    ADD CONSTRAINT fk_tbluser_program_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
