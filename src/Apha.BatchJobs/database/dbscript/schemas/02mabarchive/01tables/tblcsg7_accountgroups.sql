-- Table: mabarchive.tblcsg7_accountgroups

CREATE TABLE mabarchive.tblcsg7_accountgroups (
    csg7group character varying(15) NOT NULL,
    useinflation boolean DEFAULT true,
    CONSTRAINT aaaaatblcsg7_accountgroups_pk PRIMARY KEY (csg7group)
);

