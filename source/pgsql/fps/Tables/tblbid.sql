-- Table: fps.tblbid
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblbid; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblbid (
    workgroup public.citext NOT NULL,
    account public.citext NOT NULL,
    genbid money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tblbid pk_tblbid; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblbid
    ADD CONSTRAINT pk_tblbid PRIMARY KEY (workgroup, account, fpsyear);
-- Name: tblbid fk_tblbid_account; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblbid
    ADD CONSTRAINT fk_tblbid_account FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory(accshortname, fpsyear);
-- Name: tblbid fk_tblbid_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblbid
    ADD CONSTRAINT fk_tblbid_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tblbid fk_tblbid_workgroup; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblbid
    ADD CONSTRAINT fk_tblbid_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear);
