-- Table: mabarchive.my_proj_invoice
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_proj_invoice; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_proj_invoice (
    year smallint NOT NULL,
    projectparent character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    month integer,
    amount money,
    costofwork money,
    wip money,
    profitloss money,
    detail character varying(100) COLLATE pg_catalog."und-x-icu",
    invoicecounter integer NOT NULL,
    type character varying(10) COLLATE pg_catalog."und-x-icu"
);
-- Name: my_proj_invoice pk_my_proj_invoice; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_proj_invoice
    ADD CONSTRAINT pk_my_proj_invoice PRIMARY KEY (year, projectparent, invoicecounter);
