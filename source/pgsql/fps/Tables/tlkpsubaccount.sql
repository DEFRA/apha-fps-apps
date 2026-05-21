CREATE TABLE IF NOT EXISTS fps.tlkpsubaccount (
    subaccountcode character varying(50) NOT NULL,
    subaccount character varying(50),
    CONSTRAINT pk_tlkpsubaccount PRIMARY KEY (subaccountcode)
);
