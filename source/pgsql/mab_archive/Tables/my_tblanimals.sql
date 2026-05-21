-- Table: mabarchive.my_tblanimals
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tblanimals; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tblanimals (
    year smallint NOT NULL,
    animaltype character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    species character varying(50) COLLATE pg_catalog."und-x-icu",
    security_level character varying(50) COLLATE pg_catalog."und-x-icu",
    dailyrate money,
    planbyweek boolean,
    defradailyrate money
);
-- Name: my_tblanimals pk__my_tblanimals__18ebb532; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tblanimals
    ADD CONSTRAINT pk__my_tblanimals__18ebb532 PRIMARY KEY (year, animaltype);
