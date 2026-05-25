-- Table: fps.tbladminusers
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbladminusers; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbladminusers (
    mnumber character varying(50) NOT NULL COLLATE public.latin1_general_ci_as,
    name character varying(50) NOT NULL COLLATE public.latin1_general_ci_as,
    seedeptincome boolean DEFAULT false NOT NULL,
    seedbwindow boolean DEFAULT false NOT NULL,
    dt2number character varying(50) COLLATE public.latin1_general_ci_as,
    fpsyear integer NOT NULL
);
-- Name: tbladminusers pk_tbladminusers; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbladminusers
    ADD CONSTRAINT pk_tbladminusers PRIMARY KEY (mnumber, fpsyear);
-- Name: tbladminusers fk_tbladminusers_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbladminusers
    ADD CONSTRAINT fk_tbladminusers_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
