-- Table: fps.workgroupgrade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: workgroupgrade; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.workgroupgrade (
    wggrade public.citext NOT NULL,
    profitcentregrade public.citext NOT NULL,
    gradecode public.citext NOT NULL,
    workgroup public.citext NOT NULL,
    chargeratewg money,
    directratewg money DEFAULT 0,
    payratewg money DEFAULT 0,
    nprwg money DEFAULT 0,
    ohrwg money DEFAULT 0,
    avsalary money DEFAULT 0,
    hrschangedby character varying(50),
    fpsyear integer NOT NULL
);
-- Name: workgroupgrade pk_workgroupgrade; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupgrade
    ADD CONSTRAINT pk_workgroupgrade PRIMARY KEY (wggrade, fpsyear);
-- Name: workgroupgrade fk_workgroupgrade_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupgrade
    ADD CONSTRAINT fk_workgroupgrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: workgroupgrade fk_workgroupgrade_gradecode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupgrade
    ADD CONSTRAINT fk_workgroupgrade_gradecode FOREIGN KEY (gradecode, fpsyear) REFERENCES fps.grade(gradecode, fpsyear);
-- Name: workgroupgrade fk_workgroupgrade_profitcentregrade; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupgrade
    ADD CONSTRAINT fk_workgroupgrade_profitcentregrade FOREIGN KEY (profitcentregrade, fpsyear) REFERENCES fps.profitcentregrade(pcgrade, fpsyear);
-- Name: workgroupgrade fk_workgroupgrade_workgroup; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupgrade
    ADD CONSTRAINT fk_workgroupgrade_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear);
