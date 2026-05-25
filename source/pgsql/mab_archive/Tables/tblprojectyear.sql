-- Table: mabarchive.tblprojectyear
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblprojectyear; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblprojectyear (
    project character varying(50) NOT NULL,
    yearno integer NOT NULL,
    markup_time double precision,
    markup_tests double precision,
    markup_animals double precision,
    markup_additional double precision,
    profit_time double precision,
    profit_tests double precision,
    profit_animals double precision,
    profit_additional double precision
);
-- Name: tblprojectyear aaaaatblprojectyear_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblprojectyear
    ADD CONSTRAINT aaaaatblprojectyear_pk PRIMARY KEY (project, yearno);
-- Name: tblprojecttblprojectyear; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblprojecttblprojectyear ON mabarchive.tblprojectyear USING btree (project);
