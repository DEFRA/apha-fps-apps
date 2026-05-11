-- Table: mabarchive.temptbladditionalcosts

CREATE TABLE mabarchive.temptbladditionalcosts (
    ac_identity integer DEFAULT nextval('mabarchive.temptbladditionalcosts_ac_identity_seq'::regclass) NOT NULL,
    project integer DEFAULT 0,
    year integer DEFAULT 0,
    accountcat character varying(50),
    description character varying(20),
    itemcost double precision DEFAULT 0,
    costentered double precision DEFAULT 0,
    freq character varying(5),
    CONSTRAINT aaaaatemptbladditionalcosts_pk PRIMARY KEY (ac_identity)
);

