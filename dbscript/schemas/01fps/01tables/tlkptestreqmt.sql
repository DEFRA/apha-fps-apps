-- Table: fps.tlkptestreqmt

CREATE TABLE fps.tlkptestreqmt (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    datecreated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    active smallint DEFAULT 1,
    fpsyear integer,
    CONSTRAINT aaaaatlkptestreqmt_pk PRIMARY KEY (testcode, buyer),
    CONSTRAINT fk_tlkptestreqmt_testcode FOREIGN KEY (testcode) REFERENCES fps.testorproduct(itemcode)
);

