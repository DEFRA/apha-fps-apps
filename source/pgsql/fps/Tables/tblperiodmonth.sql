-- Table: fps.tblperiodmonth
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblperiodmonth; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblperiodmonth (
    endmonth double precision NOT NULL,
    monthno double precision NOT NULL
);
-- Name: tblperiodmonth aaaaatblkperiodmonth_pk; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblperiodmonth
    ADD CONSTRAINT aaaaatblkperiodmonth_pk PRIMARY KEY (endmonth, monthno);
-- Name: monthno; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX monthno ON fps.tblperiodmonth USING btree (monthno);
