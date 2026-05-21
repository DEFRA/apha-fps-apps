-- Table: mabarchive.tlkpreviewitem
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpreviewitem; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpreviewitem (
    itemid integer NOT NULL,
    item character varying(50)
);
-- Name: tlkpreviewitem pk_tlkpreviewitem; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpreviewitem
    ADD CONSTRAINT pk_tlkpreviewitem PRIMARY KEY (itemid);
