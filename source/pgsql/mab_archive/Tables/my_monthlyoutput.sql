-- Table: mabarchive.my_monthlyoutput
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_monthlyoutput; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_monthlyoutput (
    year smallint NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    month double precision NOT NULL,
    workgroup character varying(50) NOT NULL,
    volume double precision,
    wgbuyer character varying(50)
);
-- Name: my_monthlyoutput pk_my_monthlyoutput; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_monthlyoutput
    ADD CONSTRAINT pk_my_monthlyoutput PRIMARY KEY (year, testcode, buyer, month, workgroup);
-- Name: my_mo_month; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX my_mo_month ON mabarchive.my_monthlyoutput USING btree (month);
-- Name: my_mo_testcode; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX my_mo_testcode ON mabarchive.my_monthlyoutput USING btree (testcode);
-- Name: my_mo_year; Type: INDEX; Schema: mabarchive; Owner: -
CREATE INDEX my_mo_year ON mabarchive.my_monthlyoutput USING btree (year);
