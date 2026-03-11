-- Sequence: mabarchive.my_tblanimalreq_AR_Counter_seq

CREATE SEQUENCE mabarchive."my_tblanimalreq_AR_Counter_seq"
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive."my_tblanimalreq_AR_Counter_seq" OWNED BY mabarchive.my_tblanimalreq.ar_counter;
