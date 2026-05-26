-- Table: mabarchive.tblaccessusers
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblaccessusers; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblaccessusers (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    username character varying(50),
    dt2login character varying(50),
    useremail character varying(255)
);
-- Name: tblaccessusers pk_tblaccessusers; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblaccessusers
    ADD CONSTRAINT pk_tblaccessusers PRIMARY KEY (systemid, ntlogin);
