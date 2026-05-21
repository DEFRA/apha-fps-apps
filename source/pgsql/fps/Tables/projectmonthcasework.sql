-- Table: fps.projectmonthcasework
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: projectmonthcasework; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.projectmonthcasework (
    project character varying(20) NOT NULL,
    monthno integer NOT NULL,
    cwdebit double precision,
    cwcredit double precision
);
-- Name: projectmonthcasework pk_projectmonthcasework_1__10; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.projectmonthcasework
    ADD CONSTRAINT pk_projectmonthcasework_1__10 PRIMARY KEY (project, monthno);
