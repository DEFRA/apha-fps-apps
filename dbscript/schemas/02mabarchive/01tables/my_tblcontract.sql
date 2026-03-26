-- Table: mabarchive.my_tblcontract

CREATE TABLE mabarchive.my_tblcontract (
    year smallint NOT NULL,
    contractno character varying(10) NOT NULL,
    category character varying(20) NOT NULL,
    manager character varying(50),
    customer character varying(50),
    title character varying(100),
    registereddate date,
    startdate date,
    enddate date,
    contractdoc bytea,
    duration integer,
    CONSTRAINT pk_my_tblcontract PRIMARY KEY (year, contractno)
);

