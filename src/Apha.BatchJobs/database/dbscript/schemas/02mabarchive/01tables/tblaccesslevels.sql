-- Table: mabarchive.tblaccesslevels

CREATE TABLE mabarchive.tblaccesslevels (
    systemid integer NOT NULL,
    accesslevelid integer NOT NULL,
    accesslevel character varying(50),
    CONSTRAINT pk_tblaccesslevels PRIMARY KEY (systemid, accesslevelid)
);

