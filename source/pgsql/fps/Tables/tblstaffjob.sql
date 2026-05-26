-- Table: fps.tblstaffjob
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblstaffjob; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblstaffjob (
    staffid public.citext NOT NULL,
    jobcode public.citext NOT NULL,
    plannedhours double precision DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tblstaffjob pk_tblstaffjob; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblstaffjob
    ADD CONSTRAINT pk_tblstaffjob PRIMARY KEY (staffid, jobcode, fpsyear);
-- Name: tblstaffjob fk_tblstaffjob_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblstaffjob
    ADD CONSTRAINT fk_tblstaffjob_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tblstaffjob fk_tblstaffjob_jobcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblstaffjob
    ADD CONSTRAINT fk_tblstaffjob_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
-- Name: tblstaffjob fk_tblstaffjob_staffid; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblstaffjob
    ADD CONSTRAINT fk_tblstaffjob_staffid FOREIGN KEY (staffid, fpsyear) REFERENCES fps.tblwgemployee(pactid, fpsyear);
