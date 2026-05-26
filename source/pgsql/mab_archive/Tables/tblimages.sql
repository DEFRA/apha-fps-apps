-- Table: mabarchive.tblimages
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblimages; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblimages (
    imageid integer NOT NULL,
    image bytea,
    decription character varying(50)
);
-- Name: tblimages pk_tblimages; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblimages
    ADD CONSTRAINT pk_tblimages PRIMARY KEY (imageid);
