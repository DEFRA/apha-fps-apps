-- Table: mabarchive.tblaccessusers_levels

CREATE TABLE mabarchive.tblaccessusers_levels (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    accesslevelid integer NOT NULL,
    CONSTRAINT pk_tblaccessusers_levels PRIMARY KEY (systemid, ntlogin, accesslevelid)
);

