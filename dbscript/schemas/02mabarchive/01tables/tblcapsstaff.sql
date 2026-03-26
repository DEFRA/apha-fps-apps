-- Table: mabarchive.tblcapsstaff

CREATE TABLE mabarchive.tblcapsstaff (
    mnumber character varying(50) NOT NULL,
    name character varying(50) NOT NULL,
    dt2number character varying(50),
    CONSTRAINT pk_tblcapsstaff PRIMARY KEY (mnumber)
);

