-- Table: fps.recreatesummaries_log

CREATE TABLE fps.recreatesummaries_log (
    id integer DEFAULT nextval('fps.recreatesummaries_log_id_seq'::regclass) NOT NULL,
    userid character varying(20),
    period smallint,
    datedone timestamp without time zone,
    fpsyear integer,
    CONSTRAINT pk_recreatesummaries_log PRIMARY KEY (id)
);

