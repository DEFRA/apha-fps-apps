-- Table: mabarchive.my_workgroup

CREATE TABLE mabarchive.my_workgroup (
    year smallint NOT NULL,
    workgroup character varying(50) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    costcentre double precision,
    owner character varying(50),
    description character varying(45),
    centraloverhead money,
    sendemail smallint,
    cos90 smallint,
    costcentreold double precision,
    email_recipient character varying(50),
    CONSTRAINT pk_my_workgroup PRIMARY KEY (year, workgroup)
);

