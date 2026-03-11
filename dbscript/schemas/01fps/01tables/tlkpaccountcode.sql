-- Table: fps.tlkpaccountcode

CREATE TABLE fps.tlkpaccountcode (
    code citext NOT NULL,
    description character varying(50) NOT NULL,
    CONSTRAINT pk_tlkpaccountcode PRIMARY KEY (code)
);

