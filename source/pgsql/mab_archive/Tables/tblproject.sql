-- Table: mabarchive.tblproject
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblproject; Type: TABLE; Schema: mabarchive; Owner: -
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
    isdefraproject smallint
);
-- Name: tblproject aaaaatblproject_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblproject
    ADD CONSTRAINT aaaaatblproject_pk PRIMARY KEY (project);
