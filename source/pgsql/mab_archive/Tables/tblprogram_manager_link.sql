-- Table: mabarchive.tblprogram_manager_link
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblprogram_manager_link; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblprogram_manager_link (
    program character varying(50) NOT NULL,
    manager character varying(50) NOT NULL
);
-- Name: tblprogram_manager_link pk_tblprogram_manager; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblprogram_manager_link
    ADD CONSTRAINT pk_tblprogram_manager PRIMARY KEY (program, manager);
