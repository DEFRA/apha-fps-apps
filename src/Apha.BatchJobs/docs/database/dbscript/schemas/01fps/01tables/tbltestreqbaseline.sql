-- Table: fps.tbltestreqbaseline

CREATE TABLE fps.tbltestreqbaseline (
    program character varying(10) NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    norequired integer,
    unitprice money,
    fpsyear integer,
    CONSTRAINT pk_tbltestreqbaseline_1__18 PRIMARY KEY (program, testcode, buyer)
);

