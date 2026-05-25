-- Table: fps.tbluser_workgroup
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbluser_workgroup; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbluser_workgroup (
    workgroup character varying(50) NOT NULL,
    user_id integer NOT NULL
);
-- Name: tbluser_workgroup pk___7__10; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbluser_workgroup
    ADD CONSTRAINT pk___7__10 PRIMARY KEY (user_id, workgroup);
