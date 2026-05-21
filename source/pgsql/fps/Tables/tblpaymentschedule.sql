-- Table: fps.tblpaymentschedule
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblpaymentschedule; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblpaymentschedule (
    contract public.citext NOT NULL,
    duedate timestamp without time zone NOT NULL,
    paid smallint NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tblpaymentschedule pk_tblpaymentschedule; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblpaymentschedule
    ADD CONSTRAINT pk_tblpaymentschedule PRIMARY KEY (contract, duedate, fpsyear);
-- Name: tblpaymentschedule fk_tblpaymentschedule_contract; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblpaymentschedule
    ADD CONSTRAINT fk_tblpaymentschedule_contract FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract(contractno, fpsyear);
-- Name: tblpaymentschedule fk_tblpaymentschedule_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblpaymentschedule
    ADD CONSTRAINT fk_tblpaymentschedule_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
