-- Table: fps.tbluser_testowner
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbluser_testowner; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbluser_testowner (
    user_id integer NOT NULL,
    test_owner character varying(2) NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbluser_testowner pk_tbluser_testowner; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_testowner
    ADD CONSTRAINT pk_tbluser_testowner PRIMARY KEY (test_owner, user_id, fpsyear);
-- Name: tbluser_testowner fk_tbluser_testowner_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_testowner
    ADD CONSTRAINT fk_tbluser_testowner_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
