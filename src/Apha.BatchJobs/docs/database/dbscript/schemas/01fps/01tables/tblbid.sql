-- Table: fps.tblbid

CREATE TABLE fps.tblbid (
    workgroup citext NOT NULL,
    account citext NOT NULL,
    genbid money DEFAULT 0 NOT NULL,
    fpsyear integer,
    CONSTRAINT pk__tblbid__66ff53e3 PRIMARY KEY (workgroup, account),
    CONSTRAINT fk_tblbid_account FOREIGN KEY (account) REFERENCES fps.tblkpaccountcategory(accshortname),
    CONSTRAINT fk_tblbid_workgroup FOREIGN KEY (workgroup) REFERENCES fps.workgroup(workgroup)
);

