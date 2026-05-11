-- Table: fps.tlkpsubaccount

CREATE TABLE fps.tlkpsubaccount (
    subaccountcode citext NOT NULL,
    subaccount character varying(50),
    CONSTRAINT pk_tlkpsubaccount PRIMARY KEY (subaccountcode)
);

