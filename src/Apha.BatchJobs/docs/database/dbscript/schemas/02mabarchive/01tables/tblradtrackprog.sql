-- Table: mabarchive.tblradtrackprog

CREATE TABLE mabarchive.tblradtrackprog (
    program character varying(10) NOT NULL,
    radtrackprog boolean NOT NULL,
    publicationprefix character varying(5),
    CONSTRAINT pk_tblradtrackprog PRIMARY KEY (program)
);

