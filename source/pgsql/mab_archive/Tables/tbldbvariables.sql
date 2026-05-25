-- Table: mabarchive.tbldbvariables
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbldbvariables; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbldbvariables (
    db_variable character varying(50) NOT NULL,
    nval double precision DEFAULT 0
);
-- Name: tbldbvariables aaaaatbldbvariables_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbldbvariables
    ADD CONSTRAINT aaaaatbldbvariables_pk PRIMARY KEY (db_variable);
