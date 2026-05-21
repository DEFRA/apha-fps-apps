-- Table: fps.proj_invoice
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: proj_invoice; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.proj_invoice (
    projectparent public.citext NOT NULL,
    month integer,
    amount money,
    costofwork money,
    wip money,
    profitloss money,
    detail character varying(100),
    invoicecounter integer NOT NULL,
    x character varying(5),
    type character varying(10),
    fpsyear integer NOT NULL,
    CONSTRAINT proj_invoice_ck_proj_invoice_2__22 CHECK ((((type)::text = 'PVSIncome'::text) OR ((type)::text = 'CVOGIncome'::text)))
);
-- Name: proj_invoice_invoicecounter_seq; Type: SEQUENCE; Schema: fps; Owner: -
CREATE SEQUENCE fps.proj_invoice_invoicecounter_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;
-- Name: proj_invoice_invoicecounter_seq; Type: SEQUENCE OWNED BY; Schema: fps; Owner: -
ALTER SEQUENCE fps.proj_invoice_invoicecounter_seq OWNED BY fps.proj_invoice.invoicecounter;
-- Name: proj_invoice pk_proj_invoice; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_invoice
    ADD CONSTRAINT pk_proj_invoice PRIMARY KEY (invoicecounter, fpsyear);
-- Name: proj_invoice fk_proj_invoice_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_invoice
    ADD CONSTRAINT fk_proj_invoice_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: proj_invoice fk_proj_invoice_projectparent; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.proj_invoice
    ADD CONSTRAINT fk_proj_invoice_projectparent FOREIGN KEY (projectparent, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
