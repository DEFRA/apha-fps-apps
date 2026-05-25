-- Table: fps.divisiongrade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: divisiongrade; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.divisiongrade (
    divisiongrade public.citext NOT NULL,
    gradecode public.citext NOT NULL,
    division public.citext NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    fpsyear integer NOT NULL
);
-- Name: divisiongrade pk_divisiongrade; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.divisiongrade
    ADD CONSTRAINT pk_divisiongrade PRIMARY KEY (divisiongrade, fpsyear);
-- Name: divisiongrade fk_divisiongrade_division; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.divisiongrade
    ADD CONSTRAINT fk_divisiongrade_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname);
-- Name: divisiongrade fk_divisiongrade_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.divisiongrade
    ADD CONSTRAINT fk_divisiongrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: divisiongrade fk_divisiongrade_gradecode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.divisiongrade
    ADD CONSTRAINT fk_divisiongrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
