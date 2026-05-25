-- Table: fps.workgroup
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: workgroup; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.workgroup (
    workgroup public.citext NOT NULL,
    profitcentre public.citext NOT NULL,
    costcentre double precision,
    owner character varying(50),
    description character varying(45),
    centraloverhead money DEFAULT 0,
    sendemail smallint,
    cos90 smallint,
    costcentreold double precision,
    email_recipient character varying(50),
    fpsyear integer NOT NULL
);
-- Name: workgroup pk_workgroup; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroup
    ADD CONSTRAINT pk_workgroup PRIMARY KEY (workgroup, fpsyear);
-- Name: workgroup_profitcentre; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX workgroup_profitcentre ON fps.workgroup USING btree (profitcentre);
-- Name: workgroup fk_workgroup_costcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroup
    ADD CONSTRAINT fk_workgroup_costcentre FOREIGN KEY (costcentre, fpsyear) REFERENCES fps.costcentre(costcentre, fpsyear);
-- Name: workgroup fk_workgroup_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroup
    ADD CONSTRAINT fk_workgroup_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: workgroup fk_workgroup_profitcentre; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.workgroup
    ADD CONSTRAINT fk_workgroup_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre);
