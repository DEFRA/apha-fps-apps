-- Table: mabarchive.tblprojectmanager
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblprojectmanager; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblprojectmanager (
    projectmanager character varying(50) NOT NULL,
    email character varying(255),
    mnumber character varying(10),
    disable boolean DEFAULT false NOT NULL
);
-- Name: tblprojectmanager pk_tblprojectmanager; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblprojectmanager
    ADD CONSTRAINT pk_tblprojectmanager PRIMARY KEY (projectmanager);
