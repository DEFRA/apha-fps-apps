-- Table: mabarchive.tblaccesslevels
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblaccesslevels; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblaccesslevels (
    systemid integer NOT NULL,
    accesslevelid integer NOT NULL,
    accesslevel character varying(50)
);
-- Name: tblaccesslevels pk_tblaccesslevels; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblaccesslevels
    ADD CONSTRAINT pk_tblaccesslevels PRIMARY KEY (systemid, accesslevelid);
