-- Table: mabarchive.temptbltestreq
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: temptbltestreq; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.temptbltestreq (
    project integer DEFAULT 0 NOT NULL,
    year integer DEFAULT 0 NOT NULL,
    testcode character varying(50) NOT NULL,
    notests double precision DEFAULT 0,
    unitprice double precision DEFAULT 0
);
-- Name: temptbltestreq aaaaatemptbltestreq_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.temptbltestreq
    ADD CONSTRAINT aaaaatemptbltestreq_pk PRIMARY KEY (project, year, testcode);
-- Name: temptblprojectyeartemptbltestreq; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptblprojectyeartemptbltestreq ON mabarchive.temptbltestreq USING btree (project, year);
-- Name: temptbltestreq_tbltestrequproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX temptbltestreq_tbltestrequproject ON mabarchive.temptbltestreq USING btree (project);
