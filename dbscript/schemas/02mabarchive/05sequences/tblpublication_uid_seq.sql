-- Sequence: mabarchive.tblpublication_uid_seq

CREATE SEQUENCE mabarchive.tblpublication_uid_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.tblpublication_uid_seq OWNED BY mabarchive.tblpublication.uid;
