-- Table: mabarchive.my_workgroup
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_workgroup; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_workgroup (
    year smallint NOT NULL,
    workgroup character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    profitcentre character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    costcentre double precision,
    owner character varying(50) COLLATE pg_catalog."und-x-icu",
    description character varying(45) COLLATE pg_catalog."und-x-icu",
    centraloverhead money,
    sendemail smallint,
    cos90 smallint,
    costcentreold double precision,
    email_recipient character varying(50) COLLATE pg_catalog."und-x-icu"
);
-- Name: my_workgroup pk_my_workgroup; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_workgroup
    ADD CONSTRAINT pk_my_workgroup PRIMARY KEY (year, workgroup);
