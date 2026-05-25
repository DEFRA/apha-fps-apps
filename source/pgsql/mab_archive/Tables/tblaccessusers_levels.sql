-- Table: mabarchive.tblaccessusers_levels
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblaccessusers_levels; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblaccessusers_levels (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    accesslevelid integer NOT NULL
);
-- Name: tblaccessusers_levels pk_tblaccessusers_levels; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblaccessusers_levels
    ADD CONSTRAINT pk_tblaccessusers_levels PRIMARY KEY (systemid, ntlogin, accesslevelid);
