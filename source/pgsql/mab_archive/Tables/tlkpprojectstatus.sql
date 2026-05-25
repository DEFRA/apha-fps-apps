-- Table: mabarchive.tlkpprojectstatus
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpprojectstatus; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpprojectstatus (
    projectstatus character varying(50) NOT NULL,
    is_fps boolean NOT NULL,
    is_pims boolean NOT NULL
);
-- Name: tlkpprojectstatus pk_tlkpprojectstatus; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpprojectstatus
    ADD CONSTRAINT pk_tlkpprojectstatus PRIMARY KEY (projectstatus);
