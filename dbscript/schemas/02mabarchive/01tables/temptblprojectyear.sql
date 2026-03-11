-- Table: mabarchive.temptblprojectyear

CREATE TABLE mabarchive.temptblprojectyear (
    project integer DEFAULT 0 NOT NULL,
    yearno integer NOT NULL,
    CONSTRAINT aaaaatemptblprojectyear_pk PRIMARY KEY (project, yearno)
);

