-- Table: fps.tblkpaccountcategory

CREATE TABLE fps.tblkpaccountcategory (
    accshortname citext NOT NULL,
    accountdescription character varying(50),
    constituentaccountcodes character varying(100),
    accounttype citext NOT NULL,
    projectspecific integer,
    rcspecific integer,
    csg7_group character(15),
    fpsyear integer,
    CONSTRAINT pk__tblkpaccountcate__02dc7882 PRIMARY KEY (accshortname),
    CONSTRAINT tblkpaccountcategory_ck_accounttype CHECK (accounttype = 'Pay'::citext OR accounttype = 'NPRC'::citext)
);

