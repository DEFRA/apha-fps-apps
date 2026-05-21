-- Table: fps.profitcentregrade_nondefra
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: profitcentregrade_nondefra; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.profitcentregrade_nondefra (
    pcgrade character varying(20) NOT NULL COLLATE public.latin1_general_ci_as,
    divisiongrade public.citext NOT NULL,
    gradecode public.citext NOT NULL,
    profitcentre public.citext NOT NULL,
    chargerate money DEFAULT 0,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    fpsyear integer NOT NULL
);
-- Name: profitcentregrade_nondefra pk_profitcentregrade_nondefra; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade_nondefra
    ADD CONSTRAINT pk_profitcentregrade_nondefra PRIMARY KEY (pcgrade, fpsyear);
-- Name: profitcentregrade_nondefra fk_profitcentregrade_nondefra_divisiongrade; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade_nondefra
    ADD CONSTRAINT fk_profitcentregrade_nondefra_divisiongrade FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade(divisiongrade, fpsyear);
-- Name: profitcentregrade_nondefra fk_profitcentregrade_nondefra_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade_nondefra
    ADD CONSTRAINT fk_profitcentregrade_nondefra_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: profitcentregrade_nondefra fk_profitcentregrade_nondefra_gradecode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade_nondefra
    ADD CONSTRAINT fk_profitcentregrade_nondefra_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
-- Name: profitcentregrade_nondefra fk_profitcentregrade_nondefra_profitcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade_nondefra
    ADD CONSTRAINT fk_profitcentregrade_nondefra_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
