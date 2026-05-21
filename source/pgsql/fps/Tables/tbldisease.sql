-- Table: fps.tbldisease
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbldisease; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbldisease (
    disease public.citext NOT NULL
);
-- Name: tbldisease pk___4__10; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbldisease
    ADD CONSTRAINT pk___4__10 PRIMARY KEY (disease);
