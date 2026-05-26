-- Table: fps.tblkpplanningcategory
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblkpplanningcategory; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblkpplanningcategory (
    planningcategory public.citext NOT NULL,
    plancategorydesc character varying(50),
    customergroup character varying(50),
    corporate character varying(50),
    divisional character varying(50)
);
-- Name: tblkpplanningcategory pk__tblkpplanningcat__05b8e52d; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblkpplanningcategory
    ADD CONSTRAINT pk__tblkpplanningcat__05b8e52d PRIMARY KEY (planningcategory);
-- Name: customergroup; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX customergroup ON fps.tblkpplanningcategory USING btree (customergroup);
