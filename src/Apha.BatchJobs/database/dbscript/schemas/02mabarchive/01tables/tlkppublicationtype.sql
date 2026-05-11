-- Table: mabarchive.tlkppublicationtype

CREATE TABLE mabarchive.tlkppublicationtype (
    type character varying(3) NOT NULL,
    description character varying(50),
    CONSTRAINT pk_tlkppublicationtype PRIMARY KEY (type)
);

