-- Table: fps.tblcategory
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblcategory; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblcategory (
    category public.citext NOT NULL
);
-- Name: tblcategory pk_tblcategory_1__10; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblcategory
    ADD CONSTRAINT pk_tblcategory_1__10 PRIMARY KEY (category);
