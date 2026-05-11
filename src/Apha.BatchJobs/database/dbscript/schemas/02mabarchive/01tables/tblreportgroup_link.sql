-- Table: mabarchive.tblreportgroup_link

CREATE TABLE mabarchive.tblreportgroup_link (
    reportid integer NOT NULL,
    groupid integer NOT NULL,
    CONSTRAINT pk_tblreportgroup_link PRIMARY KEY (reportid, groupid)
);

