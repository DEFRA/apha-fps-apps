-- Table: mabarchive.tbltestrequ

CREATE TABLE mabarchive.tbltestrequ (
    project character varying(50) NOT NULL,
    year integer DEFAULT 0 NOT NULL,
    testcode character varying(50) NOT NULL,
    notests double precision DEFAULT 0,
    unitprice double precision DEFAULT 0,
    CONSTRAINT pk_tbltestrequ PRIMARY KEY (project, year, testcode)
);

