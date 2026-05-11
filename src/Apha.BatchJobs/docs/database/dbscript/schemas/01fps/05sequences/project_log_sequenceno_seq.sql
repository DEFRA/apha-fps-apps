-- Sequence: fps.project_log_sequenceno_seq

CREATE SEQUENCE fps.project_log_sequenceno_seq
    AS bigint
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.project_log_sequenceno_seq OWNED BY fps.project_log.sequenceno;
