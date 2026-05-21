-- Table: fps.tblcontract
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblcontract; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblcontract (
    contractno public.citext NOT NULL,
    category public.citext NOT NULL,
    manager character varying(50),
    customer public.citext,
    title character varying(100),
    registereddate date,
    startdate date,
    enddate date,
    contractdoc bytea,
    duration integer,
    fpsyear integer NOT NULL
);
-- Name: tblcontract pk_tblcontract; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblcontract
    ADD CONSTRAINT pk_tblcontract PRIMARY KEY (contractno, fpsyear);
-- Name: tblcontract fk_tblcontract_3__10; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblcontract
    ADD CONSTRAINT fk_tblcontract_3__10 FOREIGN KEY (category) REFERENCES fps.tblcategory(category);
-- Name: tblcontract fk_tblcontract_customer; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblcontract
    ADD CONSTRAINT fk_tblcontract_customer FOREIGN KEY (customer) REFERENCES fps.tlkpcustomer(customer);
-- Name: tblcontract fk_tblcontract_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblcontract
    ADD CONSTRAINT fk_tblcontract_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
