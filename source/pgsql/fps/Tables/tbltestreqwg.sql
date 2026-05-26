-- Table: fps.tbltestreqwg
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbltestreqwg; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbltestreqwg (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    workgroup character varying(50) NOT NULL,
    amount integer DEFAULT 0,
    fpsyear integer NOT NULL
);
-- Name: tbltestreqwg pk_tbltestreqwg; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestreqwg
    ADD CONSTRAINT pk_tbltestreqwg PRIMARY KEY (testcode, buyer, workgroup, fpsyear);
-- Name: tbltestreqwg fk_tbltestreqwg_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestreqwg
    ADD CONSTRAINT fk_tbltestreqwg_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
