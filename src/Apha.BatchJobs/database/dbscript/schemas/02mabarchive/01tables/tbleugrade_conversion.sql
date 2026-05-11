-- Table: mabarchive.tbleugrade_conversion

CREATE TABLE mabarchive.tbleugrade_conversion (
    vlagrade character varying(50) NOT NULL,
    eugrade character varying(50),
    CONSTRAINT pk_tbleugrade_conversion PRIMARY KEY (vlagrade)
);

