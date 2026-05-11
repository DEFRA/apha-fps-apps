-- Sequence: fps.tblusers_user_id_seq

CREATE SEQUENCE fps.tblusers_user_id_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE fps.tblusers_user_id_seq OWNED BY fps.tblusers.user_id;
