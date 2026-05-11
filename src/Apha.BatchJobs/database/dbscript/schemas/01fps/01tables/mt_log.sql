-- Table: fps.mt_log

CREATE TABLE fps.mt_log (
    sequenceno integer DEFAULT nextval('fps.mt_log_sequenceno_seq'::regclass) NOT NULL,
    pactstaffid character varying(50) NOT NULL,
    timecode character varying(50) NOT NULL,
    month double precision NOT NULL,
    parentproject character varying(20) NOT NULL,
    workgroup character varying(50),
    hours double precision,
    date_time timestamp without time zone,
    user_id character varying(20),
    insert_delete character(2),
    fpsyear integer,
    CONSTRAINT mt_log_pk PRIMARY KEY (sequenceno)
);

