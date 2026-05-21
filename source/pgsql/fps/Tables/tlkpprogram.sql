-- Table: fps.tlkpprogram
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpprogram; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpprogram (
    programno public.citext NOT NULL,
    programname character varying(80),
    directorate character varying(15),
    minim character varying(7),
    sector_name character varying(50) DEFAULT 'Charge'::character varying,
    customer character varying(50),
    target money DEFAULT 0,
    manager character varying(50),
    fpsyear integer NOT NULL
);
-- Name: tlkpprogram pk_tlkpprogram; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpprogram
    ADD CONSTRAINT pk_tlkpprogram PRIMARY KEY (programno, fpsyear);
-- Name: tlkpprogram_minim; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX tlkpprogram_minim ON fps.tlkpprogram USING btree (minim);
-- Name: tlkpprogram fk_tlkpprogram_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpprogram
    ADD CONSTRAINT fk_tlkpprogram_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
