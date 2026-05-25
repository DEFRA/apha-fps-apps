-- Table: fps.tlkptestreqmt
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkptestreqmt; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tlkptestreqmt (
    testcode public.citext NOT NULL,
    buyer public.citext NOT NULL,
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    datecreated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    active smallint DEFAULT 1,
    fpsyear integer NOT NULL
);
-- Name: tlkptestreqmt pk_tlkptestreqmt; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestreqmt
    ADD CONSTRAINT pk_tlkptestreqmt PRIMARY KEY (testcode, buyer, fpsyear);
-- Name: reference10; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference10 ON fps.tlkptestreqmt USING btree (testbuyercode);
-- Name: reference19; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference19 ON fps.tlkptestreqmt USING btree (projectbuyercode);
-- Name: reference31; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX reference31 ON fps.tlkptestreqmt USING btree (testcode);
-- Name: tlkptestreqmt fk_tlkptestreqmt_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestreqmt
    ADD CONSTRAINT fk_tlkptestreqmt_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: tlkptestreqmt fk_tlkptestreqmt_testcode; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tlkptestreqmt
    ADD CONSTRAINT fk_tlkptestreqmt_testcode FOREIGN KEY (testcode, fpsyear) REFERENCES fps.testorproduct(itemcode, fpsyear);
