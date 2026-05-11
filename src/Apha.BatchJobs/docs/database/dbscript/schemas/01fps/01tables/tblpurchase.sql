-- Table: fps.tblpurchase

CREATE TABLE fps.tblpurchase (
    workgroup citext NOT NULL,
    account citext NOT NULL,
    itemdescription character varying(50) NOT NULL,
    amount money DEFAULT 0 NOT NULL,
    fpsyear integer,
    CONSTRAINT pk__tblpurchase__6acfe4c7 PRIMARY KEY (workgroup, account, itemdescription),
    CONSTRAINT fk_tblpurchase_workgroup_account FOREIGN KEY (workgroup, account) REFERENCES fps.tblbid(workgroup, account)
);

