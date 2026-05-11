-- Table: mabarchive.tblproject

CREATE TABLE mabarchive.tblproject (
    project character varying(50) NOT NULL,
    plancat character varying(50),
    projecttitle character varying(100),
    programme character varying(50),
    projectworkgroup character varying(50),
    contractprice double precision,
    startdate date,
    disease character varying(50),
    startfyear double precision DEFAULT 0,
    "customer name" character varying(50),
    "contract number" character varying(50),
    submittedbyfname character varying(50),
    submittedbylname character varying(50),
    "date of submission" date,
    "prepared by" character varying(50),
    inflation integer DEFAULT 0,
    financialyears integer,
    notes character varying(255),
    euroconvrate double precision,
    isdefraproject smallint,
    CONSTRAINT aaaaatblproject_pk PRIMARY KEY (project)
);

