-- Table: mabarchive.my_monthlytime
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_monthlytime; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_monthlytime (
    year smallint NOT NULL,
    pactstaffid character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    timecode character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    month double precision NOT NULL,
    parentproject character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    workgroup character varying(50) COLLATE pg_catalog."und-x-icu",
    hours double precision
);
-- Name: my_monthlytime pk_my_monthlytime; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_monthlytime
    ADD CONSTRAINT pk_my_monthlytime PRIMARY KEY (year, pactstaffid, timecode, month, parentproject);
