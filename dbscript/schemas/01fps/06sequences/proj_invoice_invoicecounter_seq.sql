-- Sequence: fps.proj_invoice_invoicecounter_seq

CREATE SEQUENCE fps.proj_invoice_invoicecounter_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.proj_invoice_invoicecounter_seq OWNED BY fps.proj_invoice.invoicecounter;
