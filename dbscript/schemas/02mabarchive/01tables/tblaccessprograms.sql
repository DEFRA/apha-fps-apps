-- Table: mabarchive.tblaccessprograms

CREATE TABLE mabarchive.tblaccessprograms (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    program character varying(10) NOT NULL,
    CONSTRAINT pk_tblaccessprograms PRIMARY KEY (systemid, ntlogin, program)
);

