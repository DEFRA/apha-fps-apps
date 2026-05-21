-- Table: fps.tblemployee
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblemployee; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblemployee (
    spnumber public.citext NOT NULL,
    firstname character varying(20),
    lastname character varying(20),
    title character varying(4),
    fpsyear integer NOT NULL
);
-- Name: tblemployee pk_tblemployee; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblemployee
    ADD CONSTRAINT pk_tblemployee PRIMARY KEY (spnumber, fpsyear);
-- Name: tblemployee fk_tblemployee_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblemployee
    ADD CONSTRAINT fk_tblemployee_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
