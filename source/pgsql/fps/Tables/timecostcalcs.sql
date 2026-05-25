-- Table: fps.timecostcalcs
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: timecostcalcs; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.timecostcalcs (
    workgroup character varying(50) NOT NULL,
    jobcode character varying(50) NOT NULL,
    project character varying(20) NOT NULL,
    month double precision NOT NULL,
    staffid character varying(50) NOT NULL,
    gradecode character varying(10),
    name character varying(50),
    chargerate money,
    class character varying(255),
    "time" double precision,
    cost double precision,
    division character varying(10),
    jobcodeold character varying(14),
    pay money,
    nonpay money,
    overhead money,
    fpsyear integer NOT NULL
);
-- Name: timecostcalcs pk_timecostcalcs; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.timecostcalcs
    ADD CONSTRAINT pk_timecostcalcs PRIMARY KEY (workgroup, jobcode, project, month, staffid, fpsyear);
-- Name: class; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX class ON fps.timecostcalcs USING btree (class);
-- Name: project; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX project ON fps.timecostcalcs USING btree (project);
-- Name: timecostcalcs fk_timecostcalcs_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.timecostcalcs
    ADD CONSTRAINT fk_timecostcalcs_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
