-- Table: fps.tbluser_category
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbluser_category; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbluser_category (
    user_id integer NOT NULL,
    category character varying(20) NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbluser_category pk_tbluser_category; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_category
    ADD CONSTRAINT pk_tbluser_category PRIMARY KEY (user_id, category, fpsyear);
-- Name: tbluser_category fk_tbluser_category_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_category
    ADD CONSTRAINT fk_tbluser_category_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
