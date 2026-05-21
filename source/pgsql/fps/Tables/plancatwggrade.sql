-- Table: fps.plancatwggrade
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: plancatwggrade; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.plancatwggrade (
    plancategory public.citext NOT NULL,
    wggrade public.citext NOT NULL,
    hours integer DEFAULT 0,
    createdby character varying(10),
    selleragrees character varying(10),
    buyeragrees character varying(10),
    fpsyear integer NOT NULL
);
-- Name: plancatwggrade pk_plancatwggrade; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.plancatwggrade
    ADD CONSTRAINT pk_plancatwggrade PRIMARY KEY (plancategory, wggrade, fpsyear);
-- Name: plancatwggrade fk_plancatwggrade_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.plancatwggrade
    ADD CONSTRAINT fk_plancatwggrade_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
-- Name: plancatwggrade fk_plancatwggrade_plancategory; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.plancatwggrade
    ADD CONSTRAINT fk_plancatwggrade_plancategory FOREIGN KEY (plancategory) REFERENCES fps.tblkpplanningcategory(planningcategory);
-- Name: plancatwggrade fk_plancatwggrade_wggrade; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.plancatwggrade
    ADD CONSTRAINT fk_plancatwggrade_wggrade FOREIGN KEY (wggrade, fpsyear) REFERENCES fps.workgroupgrade(wggrade, fpsyear);
