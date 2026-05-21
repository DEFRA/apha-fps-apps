CREATE TABLE IF NOT EXISTS mabarchive.my_workgroupgrade (
    year integer NOT NULL,
    wggrade character varying(50) NOT NULL,
    profitcentregrade character varying(20) NOT NULL,
    gradecode character varying(10) NOT NULL,
    workgroup character varying(50) NOT NULL,
    CONSTRAINT pk_my_workgroupgrade PRIMARY KEY (year, wggrade)
);
