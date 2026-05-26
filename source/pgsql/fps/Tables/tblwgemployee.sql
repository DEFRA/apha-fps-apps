-- Table: fps.tblwgemployee
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblwgemployee; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblwgemployee (
    pactid public.citext NOT NULL,
    spnumber public.citext NOT NULL,
    workgroupgrade public.citext NOT NULL,
    personstatus character varying(10) DEFAULT 'A'::character varying NOT NULL,
    personclass character varying(10),
    hrspaid double precision NOT NULL,
    leave double precision NOT NULL,
    sickspecial double precision NOT NULL,
    hrsavail double precision NOT NULL,
    makeavailable integer DEFAULT '-1'::integer NOT NULL,
    timerecorder integer DEFAULT 0 NOT NULL,
    startdate date,
    enddate date,
    hoursperweek double precision,
    fpsyear integer NOT NULL
);
-- Name: tblwgemployee pk_tblwgemployee; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblwgemployee
    ADD CONSTRAINT pk_tblwgemployee PRIMARY KEY (pactid, fpsyear);
-- Name: ix_tblwgemployee_makeavailable; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX ix_tblwgemployee_makeavailable ON fps.tblwgemployee USING btree (makeavailable) INCLUDE (pactid, spnumber, workgroupgrade);
-- Name: tblwgemployee fk_tblwgemployee_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblwgemployee
    ADD CONSTRAINT fk_tblwgemployee_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tblwgemployee fk_tblwgemployee_spnumber; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblwgemployee
    ADD CONSTRAINT fk_tblwgemployee_spnumber FOREIGN KEY (spnumber, fpsyear) REFERENCES fps.tblemployee(spnumber, fpsyear);
-- Name: tblwgemployee fk_tblwgemployee_workgroupgrade; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblwgemployee
    ADD CONSTRAINT fk_tblwgemployee_workgroupgrade FOREIGN KEY (workgroupgrade, fpsyear) REFERENCES fps.workgroupgrade(wggrade, fpsyear);
