-- Sequence: mabarchive.tblcomments_commentno_seq

CREATE SEQUENCE mabarchive.tblcomments_commentno_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.tblcomments_commentno_seq OWNED BY mabarchive.tblcomments.commentno;
