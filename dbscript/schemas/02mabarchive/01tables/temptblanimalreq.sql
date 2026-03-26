-- Table: mabarchive.temptblanimalreq

CREATE TABLE mabarchive.temptblanimalreq (
    ar_identity integer DEFAULT nextval('mabarchive.temptblanimalreq_ar_identity_seq'::regclass) NOT NULL,
    project integer DEFAULT 0,
    year integer DEFAULT 0,
    animaltype character varying(50),
    "number of days" double precision DEFAULT 0,
    "number of animals" double precision DEFAULT 0,
    dailyrate double precision DEFAULT 0,
    CONSTRAINT aaaaatemptblanimalreq_pk PRIMARY KEY (ar_identity)
);

