-- Table: mabarchive.g_tlkpproject
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: g_tlkpproject; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.g_tlkpproject (
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200),
    costbookno character varying(50),
    disease character varying(50),
    contract character varying(10),
    shorttitle character varying(30),
    projectstatus character varying(50)
);
-- Name: g_tlkpproject pk_g_tlkpproject; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.g_tlkpproject
    ADD CONSTRAINT pk_g_tlkpproject PRIMARY KEY (parentproject);
