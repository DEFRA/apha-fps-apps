-- Table: mabarchive.tlkpyear
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpyear; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpyear (
    year integer NOT NULL,
    latestmonthreleased integer
);
-- Name: tlkpyear pk_tlkpyear; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpyear
    ADD CONSTRAINT pk_tlkpyear PRIMARY KEY (year);
