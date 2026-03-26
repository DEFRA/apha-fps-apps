-- Sequence: fps.tlkpagency_agencyid_seq

CREATE SEQUENCE fps.tlkpagency_agencyid_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.tlkpagency_agencyid_seq OWNED BY fps.tlkpagency.agencyid;
