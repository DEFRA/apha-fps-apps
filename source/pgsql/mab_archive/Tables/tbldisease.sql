-- Table: mabarchive.tbldisease
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbldisease; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbldisease (
    disease character varying(50) NOT NULL
);
-- Name: tbldisease pk_tbldisease; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbldisease
    ADD CONSTRAINT pk_tbldisease PRIMARY KEY (disease);
