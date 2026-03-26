-- Table: mabarchive.my_tlkpprogram

CREATE TABLE mabarchive.my_tlkpprogram (
    year smallint NOT NULL,
    programno character varying(10) NOT NULL,
    programname character varying(80),
    directorate character varying(15),
    minim character varying(7),
    sector_name character varying(50),
    customer character varying(50),
    target money,
    manager character varying(50),
    CONSTRAINT pk_my_tlkpprogram PRIMARY KEY (year, programno)
);

