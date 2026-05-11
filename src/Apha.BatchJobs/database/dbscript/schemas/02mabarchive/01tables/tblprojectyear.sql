-- Table: mabarchive.tblprojectyear

CREATE TABLE mabarchive.tblprojectyear (
    project character varying(50) NOT NULL,
    yearno integer NOT NULL,
    markup_time double precision,
    markup_tests double precision,
    markup_animals double precision,
    markup_additional double precision,
    profit_time double precision,
    profit_tests double precision,
    profit_animals double precision,
    profit_additional double precision,
    CONSTRAINT aaaaatblprojectyear_pk PRIMARY KEY (project, yearno)
);

