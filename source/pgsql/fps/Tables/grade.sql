-- Table: fps.grade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: grade; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.grade (
    gradecode public.citext NOT NULL,
    desc_long character varying(30) COLLATE public.latin1_general_ci_as,
    avsalary money DEFAULT 0,
    pactcode character varying(50) COLLATE public.latin1_general_ci_as,
    avleavehrs double precision DEFAULT 0,
    avsickhrs double precision DEFAULT 0,
    fpsyear integer NOT NULL
);
-- Name: grade pk_grade; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.grade
    ADD CONSTRAINT pk_grade PRIMARY KEY (gradecode, fpsyear);
-- Name: grade fk_grade_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.grade
    ADD CONSTRAINT fk_grade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
