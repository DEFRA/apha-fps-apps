-- Table: fps.timecodevalid
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: timecodevalid; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.timecodevalid (
    timecode public.citext NOT NULL,
    workgroup public.citext NOT NULL,
    parentproject public.citext NOT NULL,
    testcode character varying(50),
    jobcode character varying(50),
    portfolio character varying(20),
    active boolean NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: timecodevalid pk_timecodevalid; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.timecodevalid
    ADD CONSTRAINT pk_timecodevalid PRIMARY KEY (workgroup, timecode, parentproject, fpsyear);
-- Name: reference20; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference20 ON fps.timecodevalid USING btree (jobcode);
-- Name: reference24; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference24 ON fps.timecodevalid USING btree (testcode, portfolio);
-- Name: reference3; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference3 ON fps.timecodevalid USING btree (parentproject);
-- Name: timecodevalid fk_timecodevalid_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.timecodevalid
    ADD CONSTRAINT fk_timecodevalid_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: timecodevalid fk_timecodevalid_parentproject; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.timecodevalid
    ADD CONSTRAINT fk_timecodevalid_parentproject FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
