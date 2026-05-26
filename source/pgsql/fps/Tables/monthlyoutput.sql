-- Table: fps.monthlyoutput
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: monthlyoutput; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.monthlyoutput (
    testcode public.citext NOT NULL,
    buyer public.citext NOT NULL,
    month double precision NOT NULL,
    workgroup public.citext NOT NULL,
    volume double precision,
    wgbuyer character varying(50),
    fpsyear integer NOT NULL
);
-- Name: monthlyoutput pk_monthlyoutput; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlyoutput
    ADD CONSTRAINT pk_monthlyoutput PRIMARY KEY (testcode, buyer, month, workgroup, fpsyear);
-- Name: month; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX month ON fps.monthlyoutput USING btree (month);
-- Name: monthlyoutput_workgroup; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX monthlyoutput_workgroup ON fps.monthlyoutput USING btree (workgroup);
-- Name: reference14; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference14 ON fps.monthlyoutput USING btree (testcode, buyer);
-- Name: reference25; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference25 ON fps.monthlyoutput USING btree (workgroup, testcode);
-- Name: testcode; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX testcode ON fps.monthlyoutput USING btree (testcode);
-- Name: monthlyoutput fk_monthlyoutput_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlyoutput
    ADD CONSTRAINT fk_monthlyoutput_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: monthlyoutput fk_monthlyoutput_testcode_buyer; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlyoutput
    ADD CONSTRAINT fk_monthlyoutput_testcode_buyer FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt(testcode, buyer, fpsyear);
-- Name: monthlyoutput fk_monthlyoutput_testcode_workgroup; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.monthlyoutput
    ADD CONSTRAINT fk_monthlyoutput_testcode_workgroup FOREIGN KEY (testcode, workgroup, fpsyear) REFERENCES fps.tlkptestcapability(testcode, workgroup, fpsyear);
