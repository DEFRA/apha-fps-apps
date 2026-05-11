-- Table: mabarchive.tbladditionalcosts

CREATE TABLE mabarchive.tbladditionalcosts (
    ac_identity integer DEFAULT nextval('mabarchive.tbladditionalcosts_ac_identity_seq'::regclass) NOT NULL,
    project character varying(50),
    year integer DEFAULT 0,
    accountcat character varying(50) NOT NULL,
    description character varying(100) NOT NULL,
    itemcost double precision DEFAULT 0,
    costentered double precision DEFAULT 0 NOT NULL,
    freq character varying(5),
    CONSTRAINT aaaaatbladditionalcosts_pk PRIMARY KEY (ac_identity)
);

