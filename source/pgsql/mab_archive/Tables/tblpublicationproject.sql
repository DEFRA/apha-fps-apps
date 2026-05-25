-- Table: mabarchive.tblpublicationproject
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblpublicationproject; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblpublicationproject (
    publicationuid integer NOT NULL,
    parentproject character varying(20) NOT NULL
);
-- Name: tblpublicationproject pk_tblpublicationproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblpublicationproject
    ADD CONSTRAINT pk_tblpublicationproject PRIMARY KEY (publicationuid, parentproject);
