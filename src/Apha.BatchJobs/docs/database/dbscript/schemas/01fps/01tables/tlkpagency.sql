-- Table: fps.tlkpagency

CREATE TABLE fps.tlkpagency (
    agencyid integer DEFAULT nextval('fps.tlkpagency_agencyid_seq'::regclass) NOT NULL,
    agencyname character varying(18) NOT NULL,
    CONSTRAINT pk__tlkpagency__089551d8 PRIMARY KEY (agencyid),
    CONSTRAINT agencyname UNIQUE (agencyname)
);

