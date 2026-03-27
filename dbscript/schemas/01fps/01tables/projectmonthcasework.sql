-- Table: fps.projectmonthcasework

CREATE TABLE fps.projectmonthcasework (
    project character varying(20) NOT NULL,
    monthno integer NOT NULL,
    cwdebit double precision,
    cwcredit double precision,
    CONSTRAINT pk_projectmonthcasework_1__10 PRIMARY KEY (project, monthno)
);

