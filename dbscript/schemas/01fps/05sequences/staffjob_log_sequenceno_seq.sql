-- Sequence: fps.staffjob_log_sequenceno_seq

CREATE SEQUENCE fps.staffjob_log_sequenceno_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.staffjob_log_sequenceno_seq OWNED BY fps.staffjob_log.sequenceno;
