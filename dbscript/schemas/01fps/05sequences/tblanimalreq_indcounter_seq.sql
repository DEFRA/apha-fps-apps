-- Sequence: fps.tblanimalreq_indcounter_seq

CREATE SEQUENCE fps.tblanimalreq_indcounter_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.tblanimalreq_indcounter_seq OWNED BY fps.tblanimalreq.indcounter;
