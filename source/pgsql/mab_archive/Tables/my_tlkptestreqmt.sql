-- Table: mabarchive.my_tlkptestreqmt
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tlkptestreqmt; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tlkptestreqmt (
    year smallint NOT NULL,
    testcode character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    buyer character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50) COLLATE pg_catalog."und-x-icu",
    testbuyercode character varying(50) COLLATE pg_catalog."und-x-icu",
    source character(5) COLLATE pg_catalog."und-x-icu"
);
-- Name: my_tlkptestreqmt pk_my_tlkptestreqmt; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tlkptestreqmt
    ADD CONSTRAINT pk_my_tlkptestreqmt PRIMARY KEY (year, testcode, buyer);
