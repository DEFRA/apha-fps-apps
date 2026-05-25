-- Table: fps.tbltestreqbaseline
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbltestreqbaseline; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbltestreqbaseline (
    program character varying(10) NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    norequired integer,
    unitprice money,
    fpsyear integer NOT NULL
);
-- Name: tbltestreqbaseline pk_tbltestreqbaseline; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestreqbaseline
    ADD CONSTRAINT pk_tbltestreqbaseline PRIMARY KEY (program, testcode, buyer, fpsyear);
-- Name: tbltestreqbaseline fk_tbltestreqbaseline_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestreqbaseline
    ADD CONSTRAINT fk_tbltestreqbaseline_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
