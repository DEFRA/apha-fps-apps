-- Table: fps.tlkpproject
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpproject; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkpproject (
    parentproject public.citext NOT NULL,
    projecttitle character varying(200) NOT NULL,
    program public.citext NOT NULL,
    customer public.citext NOT NULL,
    manager character varying(50),
    transferincome money NOT NULL,
    custincome money NOT NULL,
    wip_eoy money DEFAULT 0,
    wip_limit money,
    wip_current money,
    projectstatus public.citext NOT NULL,
    costbookno character varying(50),
    datecreated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    feccost money DEFAULT 0,
    profit money DEFAULT 0,
    budget_cvl money DEFAULT 0,
    datecosted timestamp without time zone,
    disease public.citext NOT NULL,
    contract public.citext DEFAULT 0 NOT NULL,
    projectparent character varying(50),
    shorttitle character varying(30),
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    finished smallint DEFAULT 0,
    owningrc character varying(50),
    comments text,
    carryover money,
    carryoverseed money,
    isdefraproject smallint NOT NULL,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode public.citext,
    projectgroup public.citext,
    incomeaccountcode public.citext NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: tlkpproject pk_tlkpproject; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT pk_tlkpproject PRIMARY KEY (parentproject, fpsyear);
-- Name: projectstatus; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX projectstatus ON fps.tlkpproject USING btree (projectstatus);
-- Name: tlkpproject fk_tlkpproject_1__10; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_1__10 FOREIGN KEY (projectstatus) REFERENCES fps.tblstatus(status);
-- Name: tlkpproject fk_tlkpproject_1__16; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_1__16 FOREIGN KEY (customer) REFERENCES fps.tlkpcustomer(customer);
-- Name: tlkpproject fk_tlkpproject_contract; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_contract FOREIGN KEY (contract, fpsyear) REFERENCES fps.tblcontract(contractno, fpsyear);
-- Name: tlkpproject fk_tlkpproject_disease; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_disease FOREIGN KEY (disease) REFERENCES fps.tbldisease(disease);
-- Name: tlkpproject fk_tlkpproject_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tlkpproject fk_tlkpproject_incomeaccountcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_incomeaccountcode FOREIGN KEY (incomeaccountcode) REFERENCES fps.tlkpaccountcode(code);
-- Name: tlkpproject fk_tlkpproject_program; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_program FOREIGN KEY (program, fpsyear) REFERENCES fps.tlkpprogram(programno, fpsyear);
-- Name: tlkpproject fk_tlkpproject_projectgroup; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_projectgroup FOREIGN KEY (projectgroup, fpsyear) REFERENCES fps.tlkpprojectgroup(projectgroup, fpsyear);
-- Name: tlkpproject fk_tlkpproject_subaccountcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkpproject
    ADD CONSTRAINT fk_tlkpproject_subaccountcode FOREIGN KEY (subaccountcode) REFERENCES fps.tlkpsubaccount(subaccountcode);
