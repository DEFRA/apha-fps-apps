-- Table: fps.tblmtconversion
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblmtconversion; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblmtconversion (
    oldproject character varying(40) NOT NULL,
    oldcode character varying(100) NOT NULL,
    newproject character varying(40) NOT NULL,
    newcode character varying(100) NOT NULL,
    percentage double precision,
    hours double precision
);
-- Name: tblmtconversion pk_tblmtconversion; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblmtconversion
    ADD CONSTRAINT pk_tblmtconversion PRIMARY KEY (oldproject, oldcode, newproject, newcode);
