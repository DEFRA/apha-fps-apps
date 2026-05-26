-- Table: fps.period_proj_subcontract
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: period_proj_subcontract; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.period_proj_subcontract (
    period smallint NOT NULL,
    subcontcounter integer NOT NULL,
    project character varying(20) COLLATE public.latin1_general_ci_as,
    oracleprojectcode character varying(50) COLLATE public.latin1_general_ci_as,
    subaccountcode character varying(50) COLLATE public.latin1_general_ci_as,
    isdefraproject character varying(3) NOT NULL COLLATE public.latin1_general_ci_as,
    opc character varying(50) COLLATE public.latin1_general_ci_as,
    occ double precision,
    month double precision,
    amount money,
    acctcode character varying(30) COLLATE public.latin1_general_ci_as
);
-- Name: period_proj_subcontract pk_period_proj_subcontract; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.period_proj_subcontract
    ADD CONSTRAINT pk_period_proj_subcontract PRIMARY KEY (period, subcontcounter);
