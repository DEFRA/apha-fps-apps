-- Table: fps.tbladditionalcosts

CREATE TABLE fps.tbladditionalcosts (
    jobcode citext NOT NULL,
    account citext NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money DEFAULT 0 NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    fpsyear integer,
    CONSTRAINT pk__tbladditionalcos__160f4887 PRIMARY KEY (jobcode, account, description),
    CONSTRAINT fk_tbladditionalcosts_account FOREIGN KEY (account) REFERENCES fps.tblkpaccountcategory(accshortname),
    CONSTRAINT fk_tbladditionalcosts_jobcode FOREIGN KEY (jobcode) REFERENCES fps.tlkpproject(parentproject)
);

