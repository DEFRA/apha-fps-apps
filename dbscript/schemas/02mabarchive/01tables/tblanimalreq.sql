-- Table: mabarchive.tblanimalreq

CREATE TABLE mabarchive.tblanimalreq (
    ar_identity integer DEFAULT nextval('mabarchive.tblanimalreq_ar_identity_seq'::regclass) NOT NULL,
    project character varying(50),
    year integer DEFAULT 0,
    animaltype character varying(50) NOT NULL,
    "number of days" double precision,
    "number of animals" double precision DEFAULT 0,
    dailyrate double precision DEFAULT 0,
    CONSTRAINT aaaaatblanimalreq_pk PRIMARY KEY (ar_identity)
);

