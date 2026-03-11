-- Sequence: mabarchive.tblreportgroup_groupid_seq

CREATE SEQUENCE mabarchive.tblreportgroup_groupid_seq
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;

ALTER SEQUENCE mabarchive.tblreportgroup_groupid_seq OWNED BY mabarchive.tblreportgroup.groupid;
