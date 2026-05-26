-- Table: fps.resourcecentremonth
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: resourcecentremonth; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.resourcecentremonth (
    resourcecentre character varying(50) NOT NULL,
    monthno integer NOT NULL,
    payspent money,
    nonpayspent money,
    paybudget money,
    nonpaybudget money,
    spare1 money,
    spare2 money,
    spare3 money,
    spare4 money,
    spare5 money,
    spare6 money
);
-- Name: resourcecentremonth pk_resourcecentremonth; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.resourcecentremonth
    ADD CONSTRAINT pk_resourcecentremonth PRIMARY KEY (resourcecentre, monthno);
-- Name: idx_resourcecentremonth_pk; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX idx_resourcecentremonth_pk ON fps.resourcecentremonth USING btree (resourcecentre, monthno);
