-- Table: mabarchive.temptbltestreq

CREATE TABLE mabarchive.temptbltestreq (
    project integer DEFAULT 0 NOT NULL,
    year integer DEFAULT 0 NOT NULL,
    testcode character varying(50) NOT NULL,
    notests double precision DEFAULT 0,
    unitprice double precision DEFAULT 0,
    CONSTRAINT aaaaatemptbltestreq_pk PRIMARY KEY (project, year, testcode)
);

