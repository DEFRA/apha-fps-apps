-- Table: fps.timecostcalcs

CREATE TABLE fps.timecostcalcs (
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
    fpsyear integer,
    CONSTRAINT aaaaatimecostcalcs_pk PRIMARY KEY (workgroup, jobcode, project, month, staffid)
);

