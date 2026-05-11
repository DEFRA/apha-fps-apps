-- Table: mabarchive.my_tblstaffjob

CREATE TABLE mabarchive.my_tblstaffjob (
    year smallint NOT NULL,
    staffid character varying(50) NOT NULL,
    jobcode character varying(20) NOT NULL,
    plannedhours double precision NOT NULL,
    systimestamp bytea,
    CONSTRAINT pk_my_tblstaffjob PRIMARY KEY (year, staffid, jobcode)
);

