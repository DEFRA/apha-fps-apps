-- Table: fps.tblsurvff_fees
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblsurvff_fees; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblsurvff_fees (
    pactcode character varying(50) NOT NULL,
    owning_vic character varying(50) NOT NULL,
    received timestamp without time zone,
    contract character varying(20) NOT NULL,
    record_id character varying(20) NOT NULL,
    volume double precision,
    totalfee money,
    fpsyear integer NOT NULL
);
-- Name: tblsurvff_fees pk_tblsurvff_fees; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblsurvff_fees
    ADD CONSTRAINT pk_tblsurvff_fees PRIMARY KEY (owning_vic, contract, record_id, fpsyear);
-- Name: tblsurvff_fees fk_tblsurvff_fees_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblsurvff_fees
    ADD CONSTRAINT fk_tblsurvff_fees_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
