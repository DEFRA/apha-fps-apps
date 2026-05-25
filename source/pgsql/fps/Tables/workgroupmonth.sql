-- Table: fps.workgroupmonth
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: workgroupmonth; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.workgroupmonth (
    workgroup character varying(50) NOT NULL,
    month double precision NOT NULL,
    runningcost money NOT NULL,
    runcostprofile money NOT NULL,
    fpsyear integer NOT NULL
);
-- Name: workgroupmonth pk_workgroupmonth; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupmonth
    ADD CONSTRAINT pk_workgroupmonth PRIMARY KEY (workgroup, month, fpsyear);
-- Name: workgroupmonth fk_workgroupmonth_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroupmonth
    ADD CONSTRAINT fk_workgroupmonth_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
