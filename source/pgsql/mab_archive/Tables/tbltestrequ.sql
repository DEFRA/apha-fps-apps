-- Table: mabarchive.tbltestrequ
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbltestrequ; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbltestrequ (
    project character varying(50) NOT NULL,
    year integer DEFAULT 0 NOT NULL,
    testcode character varying(50) NOT NULL,
    notests double precision DEFAULT 0,
    unitprice double precision DEFAULT 0
);
-- Name: tbltestrequ pk_tbltestrequ; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbltestrequ
    ADD CONSTRAINT pk_tbltestrequ PRIMARY KEY (project, year, testcode);
-- Name: tblprojectyeartbltestrequ; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tblprojectyeartbltestrequ ON mabarchive.tbltestrequ USING btree (project, year);
-- Name: tbltestrequ_tbltestrequproject; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tbltestrequ_tbltestrequproject ON mabarchive.tbltestrequ USING btree (project);
