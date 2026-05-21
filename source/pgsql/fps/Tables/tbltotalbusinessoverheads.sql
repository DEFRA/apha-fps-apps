-- Table: fps.tbltotalbusinessoverheads
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbltotalbusinessoverheads; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbltotalbusinessoverheads (
    totalbusinessoverheads money,
    fpsyear integer NOT NULL
);
-- Name: tbltotalbusinessoverheads pk_tbltotalbusinessoverheads; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltotalbusinessoverheads
    ADD CONSTRAINT pk_tbltotalbusinessoverheads PRIMARY KEY (fpsyear);
-- Name: tbltotalbusinessoverheads tb_pk; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltotalbusinessoverheads
    ADD CONSTRAINT tb_pk UNIQUE (totalbusinessoverheads);
-- Name: tbltotalbusinessoverheads fk_tbltotalbusinessoverheads_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbltotalbusinessoverheads
    ADD CONSTRAINT fk_tbltotalbusinessoverheads_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
