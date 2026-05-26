-- Table: fps.tbldb_variables
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbldb_variables; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbldb_variables (
    db_var_name character varying(20) NOT NULL,
    db_var_value character varying(20)
);
-- Name: tbldb_variables pk_tbldb_variables; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbldb_variables
    ADD CONSTRAINT pk_tbldb_variables PRIMARY KEY (db_var_name);
