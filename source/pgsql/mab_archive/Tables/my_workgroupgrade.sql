-- Table: mabarchive.my_workgroupgrade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_workgroupgrade; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_workgroupgrade (
    year integer NOT NULL,
    wggrade character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    profitcentregrade character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    gradecode character varying(10) NOT NULL COLLATE pg_catalog."und-x-icu",
    workgroup character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu"
);
-- Name: my_workgroupgrade pk__my_workgroupgrade__2de6d218; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_workgroupgrade
    ADD CONSTRAINT pk__my_workgroupgrade__2de6d218 PRIMARY KEY (year, wggrade);
