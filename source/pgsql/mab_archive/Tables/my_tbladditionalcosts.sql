-- Table: mabarchive.my_tbladditionalcosts
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tbladditionalcosts; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tbladditionalcosts (
    year smallint NOT NULL,
    jobcode character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    account character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    description character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    itemcost money NOT NULL,
    freq character varying(5) COLLATE pg_catalog."und-x-icu",
    supplier character varying(50) COLLATE pg_catalog."und-x-icu",
    ac_counter integer NOT NULL
);
-- Name: my_tbladditionalcosts pk_my_tbladditionalcosts; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tbladditionalcosts
    ADD CONSTRAINT pk_my_tbladditionalcosts PRIMARY KEY (ac_counter);
