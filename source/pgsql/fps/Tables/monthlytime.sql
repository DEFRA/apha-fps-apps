-- Table: fps.monthlytime
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: monthlytime; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.monthlytime (
    pactstaffid public.citext NOT NULL,
    timecode public.citext NOT NULL,
    month double precision NOT NULL,
    parentproject public.citext NOT NULL,
    workgroup public.citext,
    hours double precision,
    fpsyear integer NOT NULL
);
-- Name: monthlytime pk_monthlytime; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlytime
    ADD CONSTRAINT pk_monthlytime PRIMARY KEY (pactstaffid, timecode, month, parentproject, fpsyear);
-- Name: ijnd_staffid; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX ijnd_staffid ON fps.monthlytime USING btree (pactstaffid);
-- Name: monthlytime_workgroup; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX monthlytime_workgroup ON fps.monthlytime USING btree (workgroup);
-- Name: reference23; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference23 ON fps.monthlytime USING btree (workgroup, timecode, parentproject);
-- Name: timecode; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX timecode ON fps.monthlytime USING btree (timecode);
-- Name: monthlytime fk_monthlytime_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlytime
    ADD CONSTRAINT fk_monthlytime_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: monthlytime fk_monthlytime_pactstaffid; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlytime
    ADD CONSTRAINT fk_monthlytime_pactstaffid FOREIGN KEY (pactstaffid, fpsyear) REFERENCES fps.tblwgemployee(pactid, fpsyear);
-- Name: monthlytime fk_monthlytime_timecodevalid; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlytime
    ADD CONSTRAINT fk_monthlytime_timecodevalid FOREIGN KEY (workgroup, timecode, parentproject, fpsyear) REFERENCES fps.timecodevalid(workgroup, timecode, parentproject, fpsyear);
