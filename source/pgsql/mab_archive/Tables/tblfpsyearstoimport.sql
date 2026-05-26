-- Table: mabarchive.tblfpsyearstoimport
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblfpsyearstoimport; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblfpsyearstoimport (
    fpsname character varying(10) NOT NULL
);
-- Name: tblfpsyearstoimport pk_tblyearstoimport; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblfpsyearstoimport
    ADD CONSTRAINT pk_tblyearstoimport PRIMARY KEY (fpsname);
