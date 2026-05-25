-- Table: mabarchive.tlkppublicationtype
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkppublicationtype; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkppublicationtype (
    type character varying(3) NOT NULL,
    description character varying(50)
);
-- Name: tlkppublicationtype pk_tlkppublicationtype; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkppublicationtype
    ADD CONSTRAINT pk_tlkppublicationtype PRIMARY KEY (type);
