-- Table: mabarchive.tblradtrackinvoice
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblradtrackinvoice; Type: TABLE; Schema: mabarchive; Owner: -
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
    invoicepaid smallint NOT NULL
);
-- Name: tblradtrackinvoice_invoicecounter_seq; Type: SEQUENCE; Schema: mabarchive; Owner: -
CREATE SEQUENCE mabarchive.tblradtrackinvoice_invoicecounter_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: tblradtrackinvoice_invoicecounter_seq; Type: SEQUENCE OWNED BY; Schema: mabarchive; Owner: -
ALTER SEQUENCE mabarchive.tblradtrackinvoice_invoicecounter_seq OWNED BY mabarchive.tblradtrackinvoice.invoicecounter;
-- Name: tblradtrackinvoice pk_tblradtrackinvoice; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblradtrackinvoice
    ADD CONSTRAINT pk_tblradtrackinvoice PRIMARY KEY (invoicecounter);
