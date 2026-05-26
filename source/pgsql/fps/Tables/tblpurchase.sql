-- Table: fps.tblpurchase
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblpurchase; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblpurchase (
    workgroup public.citext NOT NULL,
    account public.citext NOT NULL,
    itemdescription character varying(50) NOT NULL,
    amount money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tblpurchase pk_tblpurchase; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblpurchase
    ADD CONSTRAINT pk_tblpurchase PRIMARY KEY (workgroup, account, itemdescription, fpsyear);
-- Name: tblpurchase fk_tblpurchase_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblpurchase
    ADD CONSTRAINT fk_tblpurchase_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tblpurchase fk_tblpurchase_workgroup_account; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblpurchase
    ADD CONSTRAINT fk_tblpurchase_workgroup_account FOREIGN KEY (workgroup, account, fpsyear) REFERENCES fps.tblbid(workgroup, account, fpsyear);
