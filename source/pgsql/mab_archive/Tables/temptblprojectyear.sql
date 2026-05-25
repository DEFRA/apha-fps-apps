-- Table: mabarchive.temptblprojectyear
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: temptblprojectyear; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.temptblprojectyear (
    project integer DEFAULT 0 NOT NULL,
    yearno integer NOT NULL
);
-- Name: temptblprojectyear aaaaatemptblprojectyear_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptblprojectyear
    ADD CONSTRAINT aaaaatemptblprojectyear_pk PRIMARY KEY (project, yearno);
-- Name: temptblprojecttemptblprojectyear; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblprojecttemptblprojectyear ON mabarchive.temptblprojectyear USING btree (project);
