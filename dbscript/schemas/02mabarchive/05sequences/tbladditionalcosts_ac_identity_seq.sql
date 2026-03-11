-- Sequence: mabarchive.tbladditionalcosts_ac_identity_seq

CREATE SEQUENCE mabarchive.tbladditionalcosts_ac_identity_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.tbladditionalcosts_ac_identity_seq OWNED BY mabarchive.tbladditionalcosts.ac_identity;
