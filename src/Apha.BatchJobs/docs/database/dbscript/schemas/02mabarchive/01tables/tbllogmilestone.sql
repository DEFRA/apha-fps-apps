-- Table: mabarchive.tbllogmilestone

CREATE TABLE mabarchive.tbllogmilestone (
    project character varying(20),
    number character varying(10),
    description character varying(500),
    datedue timestamp without time zone,
    datecompleted timestamp without time zone,
    dateformreceived timestamp without time zone,
    undersdreview smallint,
    ontarget smallint,
    projectleadercomment text,
    capscomment character varying(250),
    idtype character(1),
    datechanged timestamp without time zone,
    changedby character varying(10),
    updatetype character(1),
    id integer DEFAULT nextval('mabarchive.tbllogmilestone_id_seq'::regclass) NOT NULL,
    CONSTRAINT pk_log_tblmilestone PRIMARY KEY (id)
);

