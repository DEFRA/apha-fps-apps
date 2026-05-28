CREATE TABLE IF NOT EXISTS mabarchive.tblcsg7_accountgroups (
    csg7group character varying(15) NOT NULL,
    useinflation boolean DEFAULT true,
    CONSTRAINT pk_tblcsg7_accountgroups PRIMARY KEY (csg7group)
);
