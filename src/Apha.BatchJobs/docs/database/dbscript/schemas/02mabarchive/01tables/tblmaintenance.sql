-- Table: mabarchive.tblmaintenance

CREATE TABLE mabarchive.tblmaintenance (
    formname character varying(50) NOT NULL,
    description character varying(50),
    usernotes character varying(250),
    "obsolete?" boolean NOT NULL,
    displayseq integer,
    CONSTRAINT pk_tblmaintenance PRIMARY KEY (formname)
);

