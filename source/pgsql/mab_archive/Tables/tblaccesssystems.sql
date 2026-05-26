-- Table: mabarchive.tblaccesssystems
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblaccesssystems; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblaccesssystems (
    systemid integer NOT NULL,
    systemname character varying(50) NOT NULL
);
-- Name: tblaccesssystems pk_tblaccesssystems; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblaccesssystems
    ADD CONSTRAINT pk_tblaccesssystems PRIMARY KEY (systemid);
