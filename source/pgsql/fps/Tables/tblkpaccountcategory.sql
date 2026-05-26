-- Table: fps.tblkpaccountcategory
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblkpaccountcategory; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblkpaccountcategory (
    accshortname public.citext NOT NULL,
    accountdescription character varying(50),
    constituentaccountcodes character varying(100),
    accounttype public.citext NOT NULL,
    projectspecific integer,
    rcspecific integer,
    csg7_group character(15),
    fpsyear integer NOT NULL,
    CONSTRAINT tblkpaccountcategory_ck_accounttype CHECK (((accounttype OPERATOR(public.=) 'Pay'::public.citext) OR (accounttype OPERATOR(public.=) 'NPRC'::public.citext)))
);
-- Name: tblkpaccountcategory pk_tblkpaccountcategory; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblkpaccountcategory
    ADD CONSTRAINT pk_tblkpaccountcategory PRIMARY KEY (accshortname, fpsyear);
-- Name: accounttype; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX accounttype ON fps.tblkpaccountcategory USING btree (accounttype);
-- Name: tblkpaccountcategory fk_tblkpaccountcategory_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblkpaccountcategory
    ADD CONSTRAINT fk_tblkpaccountcategory_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
