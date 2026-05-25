-- Table: fps.tblsurvff_submissions
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblsurvff_submissions; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblsurvff_submissions (
    sd_pact_wg character varying(50) NOT NULL,
    contract character varying(20) NOT NULL,
    countofjobname integer,
    fpsyear integer NOT NULL
);
-- Name: tblsurvff_submissions pk_tblsurvff_submissions; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblsurvff_submissions
    ADD CONSTRAINT pk_tblsurvff_submissions PRIMARY KEY (sd_pact_wg, contract, fpsyear);
-- Name: tblsurvff_submissions fk_tblsurvff_submissions_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblsurvff_submissions
    ADD CONSTRAINT fk_tblsurvff_submissions_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
