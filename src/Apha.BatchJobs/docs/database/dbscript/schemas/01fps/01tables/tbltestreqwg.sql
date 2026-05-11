-- Table: fps.tbltestreqwg

CREATE TABLE fps.tbltestreqwg (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    workgroup character varying(50) NOT NULL,
    amount integer DEFAULT 0,
    fpsyear integer,
    CONSTRAINT pk_tbltestreqwg PRIMARY KEY (testcode, buyer, workgroup)
);

