-- Table: mabarchive.tblcomments

CREATE TABLE mabarchive.tblcomments (
    commentno integer DEFAULT nextval('mabarchive.tblcomments_commentno_seq'::regclass) NOT NULL,
    project character varying(20) NOT NULL,
    year smallint NOT NULL,
    dateentered timestamp without time zone,
    topic character varying(25) NOT NULL,
    comment text,
    madeby character(20),
    CONSTRAINT pk_tblcomments PRIMARY KEY (commentno),
    CONSTRAINT ix_tblcomments UNIQUE (project, year, topic)
);

