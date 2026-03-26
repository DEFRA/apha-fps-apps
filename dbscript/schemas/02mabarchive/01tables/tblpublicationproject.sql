-- Table: mabarchive.tblpublicationproject

CREATE TABLE mabarchive.tblpublicationproject (
    publicationuid integer NOT NULL,
    parentproject character varying(20) NOT NULL,
    CONSTRAINT pk_tblpublicationproject PRIMARY KEY (publicationuid, parentproject)
);

