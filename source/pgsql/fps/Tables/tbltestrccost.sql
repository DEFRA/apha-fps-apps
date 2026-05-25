-- Table: fps.tbltestrccost
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbltestrccost; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbltestrccost (
    testcode public.citext NOT NULL,
    profitcentre public.citext NOT NULL,
    price money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbltestrccost pk_tbltestrccost; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrccost
    ADD CONSTRAINT pk_tbltestrccost PRIMARY KEY (testcode, profitcentre, fpsyear);
-- Name: tbltestrccost fk_tbltestrccost_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrccost
    ADD CONSTRAINT fk_tbltestrccost_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tbltestrccost fk_tbltestrccost_profitcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrccost
    ADD CONSTRAINT fk_tbltestrccost_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
-- Name: tbltestrccost fk_tbltestrccost_testcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrccost
    ADD CONSTRAINT fk_tbltestrccost_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear);
