-- Table: fps.profitcentregrade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: profitcentregrade; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.profitcentregrade (
    pcgrade public.citext NOT NULL,
    divisiongrade public.citext NOT NULL,
    gradecode public.citext NOT NULL,
    profitcentre public.citext NOT NULL,
    chargerate money,
    directrate money DEFAULT 0,
    payrate money DEFAULT 0,
    npr money DEFAULT 0,
    ohr money DEFAULT 0,
    hrsavailable double precision DEFAULT 0,
    oldchargerate money DEFAULT 0,
    defrachargerate money,
    fpsyear integer NOT NULL
);
-- Name: profitcentregrade pk_profitcentregrade; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade
    ADD CONSTRAINT pk_profitcentregrade PRIMARY KEY (pcgrade, fpsyear);
-- Name: profitcentregrade_profitcentre; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX profitcentregrade_profitcentre ON fps.profitcentregrade USING btree (profitcentre);
-- Name: profitcentregrade fk_profitcentregrade_divisiongrade; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade
    ADD CONSTRAINT fk_profitcentregrade_divisiongrade FOREIGN KEY (divisiongrade, fpsyear) REFERENCES fps.divisiongrade(divisiongrade, fpsyear);
-- Name: profitcentregrade fk_profitcentregrade_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade
    ADD CONSTRAINT fk_profitcentregrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: profitcentregrade fk_profitcentregrade_gradecode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade
    ADD CONSTRAINT fk_profitcentregrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
-- Name: profitcentregrade fk_profitcentregrade_profitcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.profitcentregrade
    ADD CONSTRAINT fk_profitcentregrade_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
