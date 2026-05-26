-- Table: fps.costcentre
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: costcentre; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.costcentre (
    costcentre double precision NOT NULL,
    profitcentre public.citext NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: costcentre pk_costcentre; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.costcentre
    ADD CONSTRAINT pk_costcentre PRIMARY KEY (costcentre, fpsyear);
-- Name: costcentre fk_costcentre_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.costcentre
    ADD CONSTRAINT fk_costcentre_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: costcentre fk_costcentre_profitcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.costcentre
    ADD CONSTRAINT fk_costcentre_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
