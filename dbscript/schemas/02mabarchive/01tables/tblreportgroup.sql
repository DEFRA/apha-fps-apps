-- Table: mabarchive.tblreportgroup

CREATE TABLE mabarchive.tblreportgroup (
    groupid integer DEFAULT nextval('mabarchive.tblreportgroup_groupid_seq'::regclass) NOT NULL,
    description character varying(50) NOT NULL,
    CONSTRAINT pk_tblreportgroup PRIMARY KEY (groupid)
);

