-- Sequence: mabarchive.temptbladditionalcosts_ac_identity_seq

CREATE SEQUENCE mabarchive.temptbladditionalcosts_ac_identity_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.temptbladditionalcosts_ac_identity_seq OWNED BY mabarchive.temptbladditionalcosts.ac_identity;
