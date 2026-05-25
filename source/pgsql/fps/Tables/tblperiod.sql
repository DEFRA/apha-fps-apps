-- Table: fps.tblperiod
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblperiod; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblperiod (
    periodname character varying(50) NOT NULL COLLATE public.latin1_general_ci_as,
    periodtype character varying(50) COLLATE public.latin1_general_ci_as,
    startperiod double precision,
    endperiod double precision,
    finalsummariesrun smallint,
    periodlocked smallint DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tblperiod pk_tblperiod; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblperiod
    ADD CONSTRAINT pk_tblperiod PRIMARY KEY (periodname, fpsyear);
-- Name: endperiod; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX endperiod ON fps.tblperiod USING btree (endperiod);
-- Name: tblperiod fk_tblperiod_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblperiod
    ADD CONSTRAINT fk_tblperiod_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
