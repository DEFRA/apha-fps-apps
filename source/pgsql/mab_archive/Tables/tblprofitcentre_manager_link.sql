-- Table: mabarchive.tblprofitcentre_manager_link
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblprofitcentre_manager_link; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblprofitcentre_manager_link (
    profitcentre character varying(50) NOT NULL,
    manager character varying(50) NOT NULL
);
-- Name: tblprofitcentre_manager_link pk_tblprofitcentre_manager; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblprofitcentre_manager_link
    ADD CONSTRAINT pk_tblprofitcentre_manager PRIMARY KEY (profitcentre, manager);
