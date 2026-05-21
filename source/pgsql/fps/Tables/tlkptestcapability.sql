-- Table: fps.tlkptestcapability
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkptestcapability; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkptestcapability (
    testcode public.citext NOT NULL,
    workgroup public.citext NOT NULL,
    planportfolio public.citext NOT NULL,
    unitcost money DEFAULT 0,
    predoutturn double precision DEFAULT 0,
    sop character varying(50),
    smscode character varying(50),
    fpsyear integer NOT NULL
);
-- Name: tlkptestcapability pk_tlkptestcapability; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestcapability
    ADD CONSTRAINT pk_tlkptestcapability PRIMARY KEY (testcode, workgroup, fpsyear);
-- Name: tlkptestcapability_planportfol; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX tlkptestcapability_planportfol ON fps.tlkptestcapability USING btree (planportfolio);
-- Name: tlkptestcapability fk_tlkptestcapability_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestcapability
    ADD CONSTRAINT fk_tlkptestcapability_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tlkptestcapability fk_tlkptestcapability_planportfolio; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestcapability
    ADD CONSTRAINT fk_tlkptestcapability_planportfolio FOREIGN KEY (planportfolio, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
-- Name: tlkptestcapability fk_tlkptestcapability_testcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestcapability
    ADD CONSTRAINT fk_tlkptestcapability_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear);
-- Name: tlkptestcapability fk_tlkptestcapability_workgroup; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestcapability
    ADD CONSTRAINT fk_tlkptestcapability_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear);
