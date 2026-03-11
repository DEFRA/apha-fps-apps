-- Sequence: fps.mt_log_sequenceno_seq

CREATE SEQUENCE fps.mt_log_sequenceno_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.mt_log_sequenceno_seq OWNED BY fps.mt_log.sequenceno;
