-- Sequence: mabarchive.temptblanimalreq_ar_identity_seq

CREATE SEQUENCE mabarchive.temptblanimalreq_ar_identity_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.temptblanimalreq_ar_identity_seq OWNED BY mabarchive.temptblanimalreq.ar_identity;
