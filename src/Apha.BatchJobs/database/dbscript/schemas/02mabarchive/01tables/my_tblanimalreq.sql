-- Table: mabarchive.my_tblanimalreq

CREATE TABLE mabarchive.my_tblanimalreq (
    year smallint NOT NULL,
    jobcode character varying(20) NOT NULL,
    animaltype character varying(50) NOT NULL,
    numberofdays double precision NOT NULL,
    numberofanimals double precision NOT NULL,
    ar_counter integer DEFAULT nextval('mabarchive."my_tblanimalreq_AR_Counter_seq"'::regclass) NOT NULL,
    CONSTRAINT pk_my_tblanimalreq PRIMARY KEY (ar_counter)
);

