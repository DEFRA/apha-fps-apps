-- Table: fps.tbluser_profitcentre
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbluser_profitcentre; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbluser_profitcentre (
    profitcentre character varying(50) NOT NULL,
    user_id integer NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbluser_profitcentre pk_tbluser_profitcentre; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_profitcentre
    ADD CONSTRAINT pk_tbluser_profitcentre PRIMARY KEY (profitcentre, user_id, fpsyear);
-- Name: xif89tbluser_profitcentre; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX xif89tbluser_profitcentre ON fps.tbluser_profitcentre USING btree (user_id);
-- Name: xif90tbluser_profitcentre; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX xif90tbluser_profitcentre ON fps.tbluser_profitcentre USING btree (profitcentre);
-- Name: tbluser_profitcentre fk_tbluser_profitcentre_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_profitcentre
    ADD CONSTRAINT fk_tbluser_profitcentre_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
