-- Table: mabarchive.tbl_settings
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbl_settings; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbl_settings (
    id character varying(50) NOT NULL,
    setting character varying(255),
    notes character varying(255),
    testsetting character varying(255),
    userupdateable boolean DEFAULT false
);
-- Name: tbl_settings aaaaatbl_settings_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbl_settings
    ADD CONSTRAINT aaaaatbl_settings_pk PRIMARY KEY (id);
-- Name: settingid; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX settingid ON mabarchive.tbl_settings USING btree (id);
