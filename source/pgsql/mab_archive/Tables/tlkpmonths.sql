-- Table: mabarchive.tlkpmonths
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpmonths; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpmonths (
    fmonthno integer NOT NULL,
    monthno integer,
    monthname character varying(50) COLLATE pg_catalog."und-x-icu"
);
-- Name: tlkpmonths pk_tlkpmonths; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpmonths
    ADD CONSTRAINT pk_tlkpmonths PRIMARY KEY (fmonthno);
-- Name: tlkpmonths_fmonthno_idx; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX tlkpmonths_fmonthno_idx ON mabarchive.tlkpmonths USING btree (fmonthno);
