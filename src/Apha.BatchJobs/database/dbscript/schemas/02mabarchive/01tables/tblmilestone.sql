-- Table: mabarchive.tblmilestone

CREATE TABLE mabarchive.tblmilestone (
    project character varying(20) NOT NULL,
    number character varying(10) NOT NULL,
    description character varying(500),
    datedue timestamp without time zone NOT NULL,
    datecompleted timestamp without time zone,
    dateformreceived timestamp without time zone,
    undersdreview smallint DEFAULT 0,
    ontarget smallint DEFAULT 0,
    projectleadercomment text,
    capscomment character varying(250),
    idtype character(1),
    CONSTRAINT pk_tblmilestone PRIMARY KEY (project, number)
);

