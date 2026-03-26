-- Table: fps.tblmtconversion

CREATE TABLE fps.tblmtconversion (
    oldproject character varying(40) NOT NULL,
    oldcode character varying(100) NOT NULL,
    newproject character varying(40) NOT NULL,
    newcode character varying(100) NOT NULL,
    percentage double precision,
    hours double precision,
    CONSTRAINT pk_tblmtconversion PRIMARY KEY (oldproject, oldcode, newproject, newcode)
);

