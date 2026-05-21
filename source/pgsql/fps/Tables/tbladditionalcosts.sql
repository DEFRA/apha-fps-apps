-- Table: fps.tbladditionalcosts
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbladditionalcosts; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tbladditionalcosts (
    jobcode public.citext NOT NULL,
    account public.citext NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money DEFAULT 0 NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    fpsyear integer NOT NULL
);
-- Name: tbladditionalcosts pk_tbladditionalcosts; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbladditionalcosts
    ADD CONSTRAINT pk_tbladditionalcosts PRIMARY KEY (jobcode, account, description, fpsyear);
-- Name: tbladditionalcosts fk_tbladditionalcosts_account; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbladditionalcosts
    ADD CONSTRAINT fk_tbladditionalcosts_account FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory(accshortname, fpsyear);
-- Name: tbladditionalcosts fk_tbladditionalcosts_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbladditionalcosts
    ADD CONSTRAINT fk_tbladditionalcosts_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tbladditionalcosts fk_tbladditionalcosts_jobcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tbladditionalcosts
    ADD CONSTRAINT fk_tbladditionalcosts_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear);
