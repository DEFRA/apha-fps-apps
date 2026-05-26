-- Table: fps.testorproduct
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: testorproduct; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.testorproduct (
    itemcode public.citext NOT NULL,
    itemdescription character varying(200),
    testmanager character varying(50),
    jobstatus character varying(2),
    unitpricevla money DEFAULT 0,
    priceahvg money,
    owner character varying(2),
    chargemethod character varying(5),
    shortdescription character(18),
    defraunitprice money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT testorproduct_owner_cannot_be_null CHECK (((owner IS NOT NULL) AND (((owner)::text = 'PT'::text) OR ((owner)::text = 'PA'::text) OR ((owner)::text = 'SD'::text) OR ((owner)::text = 'LT'::text))))
);
-- Name: testorproduct pk_testorproduct; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.testorproduct
    ADD CONSTRAINT pk_testorproduct PRIMARY KEY (itemcode, fpsyear);
-- Name: testorproduct fk_testorproduct_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.testorproduct
    ADD CONSTRAINT fk_testorproduct_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
