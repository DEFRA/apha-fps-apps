-- Table: mabarchive.temptblstaffrequ

CREATE TABLE mabarchive.temptblstaffrequ (
    sr_identity integer DEFAULT nextval('mabarchive.temptblstaffrequ_sr_identity_seq'::regclass) NOT NULL,
    project integer DEFAULT 0,
    year integer DEFAULT 0,
    wggrade character varying(20),
    name character varying(50),
    nohours double precision DEFAULT 0,
    nodays double precision DEFAULT 0,
    chargerate double precision DEFAULT 0,
    CONSTRAINT aaaaatemptblstaffrequ_pk PRIMARY KEY (sr_identity)
);

