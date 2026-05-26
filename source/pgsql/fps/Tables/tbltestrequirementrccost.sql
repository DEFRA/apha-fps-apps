-- Table: fps.tbltestrequirementrccost
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbltestrequirementrccost; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbltestrequirementrccost (
    testcode public.citext NOT NULL,
    buyer public.citext NOT NULL,
    profitcentre public.citext NOT NULL,
    price money NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tbltestrequirementrccost pk_tbltestrequirementrccost; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrequirementrccost
    ADD CONSTRAINT pk_tbltestrequirementrccost PRIMARY KEY (testcode, buyer, profitcentre, fpsyear);
-- Name: tbltestrequirementrccost fk_tbltestrequirementrccost_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrequirementrccost
    ADD CONSTRAINT fk_tbltestrequirementrccost_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tbltestrequirementrccost fk_tbltestrequirementrccost_testcode_buyer; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrequirementrccost
    ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer FOREIGN KEY (testcode, buyer, fpsyear) REFERENCES fps.tlkptestreqmt(testcode, buyer, fpsyear);
-- Name: tbltestrequirementrccost fk_tbltestrequirementrccost_testcode_profitcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltestrequirementrccost
    ADD CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre FOREIGN KEY (testcode, profitcentre, fpsyear) REFERENCES fps.tbltestrccost(testcode, profitcentre, fpsyear);
