-- Sequence: mabarchive.my_radtrack_reports_id_seq

CREATE SEQUENCE mabarchive.my_radtrack_reports_id_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.my_radtrack_reports_id_seq OWNED BY mabarchive.my_radtrack_reports.id;
