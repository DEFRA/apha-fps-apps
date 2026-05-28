CREATE TABLE IF NOT EXISTS mabarchive.tbldbvariables (
    db_variable character varying(50) NOT NULL,
    nval double precision DEFAULT 0,
    CONSTRAINT pk_tbldbvariables PRIMARY KEY (db_variable)
);
