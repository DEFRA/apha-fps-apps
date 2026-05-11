-- Table: mabarchive.tbldbvariables

CREATE TABLE mabarchive.tbldbvariables (
    db_variable character varying(50) NOT NULL,
    nval double precision DEFAULT 0,
    CONSTRAINT aaaaatbldbvariables_pk PRIMARY KEY (db_variable)
);

