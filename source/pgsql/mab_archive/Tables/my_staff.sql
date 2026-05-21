-- Table: mabarchive.my_staff
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_staff; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_staff (
    year smallint NOT NULL,
    staffid character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    workgroupgrade character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    name character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    title character varying(4) COLLATE pg_catalog."und-x-icu",
    personstatus character varying(10) COLLATE pg_catalog."und-x-icu",
    personclass character varying(10) COLLATE pg_catalog."und-x-icu",
    hrspaid double precision,
    leave double precision,
    sickspecial double precision,
    hrsavail double precision
);
-- Name: my_staff pk_my_staff; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_staff
    ADD CONSTRAINT pk_my_staff PRIMARY KEY (year, staffid);
