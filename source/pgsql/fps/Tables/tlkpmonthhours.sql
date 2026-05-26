-- Table: fps.tlkpmonthhours
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpmonthhours; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpmonthhours (
    year smallint NOT NULL,
    month smallint NOT NULL,
    days numeric(5,1),
    cvlhours numeric(5,1),
    vidhours numeric(5,1),
    fmonth smallint,
    fpsyear integer NOT NULL
);
-- Name: tlkpmonthhours pk_tlkpmonthhours; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpmonthhours
    ADD CONSTRAINT pk_tlkpmonthhours PRIMARY KEY (year, month, fpsyear);
-- Name: tlkpmonthhours tlkpmonthhours_pk; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpmonthhours
    ADD CONSTRAINT tlkpmonthhours_pk UNIQUE (year, month);
-- Name: tlkpmonthhours fk_tlkpmonthhours_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpmonthhours
    ADD CONSTRAINT fk_tlkpmonthhours_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
