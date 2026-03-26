-- Table: mabarchive.temptblproject

CREATE TABLE mabarchive.temptblproject (
    project integer DEFAULT 0 NOT NULL,
    programme character varying(10),
    plancat character varying(50),
    projecttitle character varying(100),
    projectworkgroup character varying(50),
    contractprice double precision,
    startdate date,
    disease character varying(50),
    startfyear numeric DEFAULT 0,
    "customer name" character varying(50),
    "contract number" character varying(50),
    "submitted by" character varying(50),
    "date of submission" date,
    "prepared by" character varying(50),
    inflation boolean DEFAULT false,
    ready boolean DEFAULT false,
    financialyears boolean DEFAULT true,
    notes character varying(1000),
    CONSTRAINT aaaaatemptblproject_pk PRIMARY KEY (project),
    CONSTRAINT financialyears CHECK (financialyears = true)
);

