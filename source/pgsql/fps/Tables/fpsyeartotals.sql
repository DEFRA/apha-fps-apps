-- Table: fps.fpsyeartotals
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: fpsyeartotals; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.fpsyeartotals (
    parentproject character varying(20) NOT NULL,
    program character varying(10) NOT NULL,
    totaladditionalcosts money,
    totalanimalcosts double precision,
    totalstaffcosts double precision,
    totaltestcosts double precision,
    totalcosts double precision,
    custincome money NOT NULL,
    transferincome money NOT NULL,
    totalincome money NOT NULL,
    budget_cvl money,
    requiredprofit money,
    manager character varying(50),
    customer character varying(50),
    projectstatus character varying(50),
    pvsincome money,
    plancaseworkdebit money,
    totalpaycosts double precision,
    fpsyear integer NOT NULL
);
-- Name: fpsyeartotals pk_fpsyeartotals; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.fpsyeartotals
    ADD CONSTRAINT pk_fpsyeartotals PRIMARY KEY (parentproject, fpsyear);
-- Name: fpsyeartotals fk_fpsyeartotals_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.fpsyeartotals
    ADD CONSTRAINT fk_fpsyeartotals_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
