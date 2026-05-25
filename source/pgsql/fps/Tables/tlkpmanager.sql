-- Table: fps.tlkpmanager
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpmanager; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpmanager (
    manager character varying(50) NOT NULL,
    title character varying(10),
    workgroup character varying(50) NOT NULL,
    gradecode character varying(10) NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tlkpmanager pk_tlkpmanager; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpmanager
    ADD CONSTRAINT pk_tlkpmanager PRIMARY KEY (manager, fpsyear);
-- Name: tlkpmanager fk_tlkpmanager_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpmanager
    ADD CONSTRAINT fk_tlkpmanager_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
