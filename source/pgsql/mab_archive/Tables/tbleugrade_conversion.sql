CREATE TABLE IF NOT EXISTS mabarchive.tbleugrade_conversion (
    vlagrade character varying(50) NOT NULL,
    eugrade character varying(50),
    CONSTRAINT pk_tbleugrade_conversion PRIMARY KEY (vlagrade)
);
