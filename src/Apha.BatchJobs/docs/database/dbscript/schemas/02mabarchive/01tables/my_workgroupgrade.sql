-- Table: mabarchive.my_workgroupgrade

CREATE TABLE mabarchive.my_workgroupgrade (
    year integer NOT NULL,
    wggrade character varying(50) NOT NULL,
    profitcentregrade character varying(20) NOT NULL,
    gradecode character varying(10) NOT NULL,
    workgroup character varying(50) NOT NULL,
    CONSTRAINT pk__my_workgroupgrade__2de6d218 PRIMARY KEY (year, wggrade)
);

