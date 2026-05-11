-- Table: mabarchive.tblstaffrequ

CREATE TABLE mabarchive.tblstaffrequ (
    sr_identity integer DEFAULT nextval('mabarchive.tblstaffrequ_sr_identity_seq'::regclass) NOT NULL,
    project character varying(50),
    year integer DEFAULT 0,
    wggrade character varying(20) NOT NULL,
    name character varying(50),
    nohours double precision DEFAULT 0,
    nodays double precision DEFAULT 0,
    chargerate double precision DEFAULT 0,
    payrate double precision,
    npr double precision,
    ohr double precision,
    CONSTRAINT aaaaatblstaffrequ_pk PRIMARY KEY (sr_identity)
);

