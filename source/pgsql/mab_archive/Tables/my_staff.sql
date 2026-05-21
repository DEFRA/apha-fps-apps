CREATE TABLE IF NOT EXISTS mabarchive.my_staff (
    year smallint NOT NULL,
    staffid character varying(50) NOT NULL,
    workgroupgrade character varying(50) NOT NULL,
    name character varying(50) NOT NULL,
    title character varying(4),
    personstatus character varying(10),
    personclass character varying(10),
    hrspaid double precision,
    leave double precision,
    sickspecial double precision,
    hrsavail double precision,
    CONSTRAINT pk_my_staff PRIMARY KEY (year, staffid)
);
