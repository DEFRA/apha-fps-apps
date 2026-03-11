-- Table: mabarchive.tblradtrackinvoice

CREATE TABLE mabarchive.tblradtrackinvoice (
    invoicecounter integer DEFAULT 0 NOT NULL,
    project character varying(20),
    plannedamount double precision,
    dueamount double precision,
    duedate timestamp without time zone,
    actualamount double precision,
    dateinvoiced timestamp without time zone,
    contract character varying(10),
    datejobsheetraised timestamp without time zone,
    invoiceref character varying(50),
    invoicepaid smallint NOT NULL,
    CONSTRAINT pk_tblradtrackinvoice PRIMARY KEY (invoicecounter)
);

