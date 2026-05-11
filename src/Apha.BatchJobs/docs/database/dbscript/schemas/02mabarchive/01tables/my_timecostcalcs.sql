-- Table: mabarchive.my_timecostcalcs

CREATE TABLE mabarchive.my_timecostcalcs (
    year smallint NOT NULL,
    workgroup character varying(50) NOT NULL,
    jobcode character varying(50) NOT NULL,
    project character varying(20) NOT NULL,
    month double precision NOT NULL,
    staffid character varying(50) NOT NULL,
    gradecode character varying(10),
    name character varying(50),
    chargerate money,
    class character varying(255),
    time double precision,
    cost double precision,
    division character varying(10),
    jobcodeold character varying(14),
    pay money,
    nonpay money,
    overhead money,
    CONSTRAINT pk_my_timecostcalcs PRIMARY KEY (year, workgroup, jobcode, project, month, staffid)
);

