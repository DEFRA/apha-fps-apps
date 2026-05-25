-- Table: fps.tlkpjobcode
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpjobcode; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpjobcode (
    jobcode character varying(50) NOT NULL,
    parentproject public.citext,
    jobcodeworkgroup character varying(50),
    newprog character varying(20),
    type character varying(15),
    jobcodename character varying(255),
    fpsyear integer NOT NULL,
    CONSTRAINT tlkpjobcode_ck_tlkpjobcode_1__11 CHECK ((type IS NOT NULL))
);
-- Name: tlkpjobcode pk_tlkpjobcode; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpjobcode
    ADD CONSTRAINT pk_tlkpjobcode PRIMARY KEY (jobcode, fpsyear);
-- Name: tlkpjobcode fk_tlkpjobcode_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpjobcode
    ADD CONSTRAINT fk_tlkpjobcode_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tlkpjobcode fk_tlkpjobcode_parentproject; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpjobcode
    ADD CONSTRAINT fk_tlkpjobcode_parentproject FOREIGN KEY (parentproject, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
