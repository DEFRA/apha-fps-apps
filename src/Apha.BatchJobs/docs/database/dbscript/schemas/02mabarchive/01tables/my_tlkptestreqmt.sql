-- Table: mabarchive.my_tlkptestreqmt

CREATE TABLE mabarchive.my_tlkptestreqmt (
    year smallint NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    source character(5),
    CONSTRAINT pk_my_tlkptestreqmt PRIMARY KEY (year, testcode, buyer)
);

