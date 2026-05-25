-- Table: mabarchive.tlkpcommenttopics
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpcommenttopics; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpcommenttopics (
    topic character varying(25) NOT NULL
);
-- Name: tlkpcommenttopics pk_tlkpcommenttopics; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpcommenttopics
    ADD CONSTRAINT pk_tlkpcommenttopics PRIMARY KEY (topic);
