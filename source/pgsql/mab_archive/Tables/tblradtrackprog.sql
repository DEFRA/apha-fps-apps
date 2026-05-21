CREATE TABLE IF NOT EXISTS mabarchive.tblradtrackprog (
    program character varying(10) NOT NULL,
    radtrackprog boolean DEFAULT true NOT NULL,
    publicationprefix character varying(5),
    CONSTRAINT pk_tblradtrackprog PRIMARY KEY (program)
);
