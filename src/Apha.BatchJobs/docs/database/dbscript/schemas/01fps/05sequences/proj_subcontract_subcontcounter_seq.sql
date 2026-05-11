-- Sequence: fps.proj_subcontract_subcontcounter_seq

CREATE SEQUENCE fps.proj_subcontract_subcontcounter_seq
    AS bigint
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.proj_subcontract_subcontcounter_seq OWNED BY fps.proj_subcontract.subcontcounter;
