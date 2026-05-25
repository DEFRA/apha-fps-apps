-- Table: fps.tblkpprofitcentre
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblkpprofitcentre; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblkpprofitcentre (
    profitcentre public.citext NOT NULL,
    profitcentrename character varying(40) NOT NULL,
    division public.citext DEFAULT 0 NOT NULL,
    conttarget money DEFAULT 0,
    profitcentrehead character varying(50),
    divisionid integer DEFAULT 0,
    email_recipient character varying(50),
    timesheetlayout smallint,
    timesheet integer,
    outputsheet integer,
    pactcoordinatoremailname character varying(50),
    highlevelsummary bytea
);
-- Name: tblkpprofitcentre pk__tblkpprofitcentr__1db06a4f; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblkpprofitcentre
    ADD CONSTRAINT pk__tblkpprofitcentr__1db06a4f PRIMARY KEY (profitcentre);
-- Name: division; Type: INDEX; Schema: fps; Owner: -
CREATE INDEX division ON fps.tblkpprofitcentre USING btree (division);
-- Name: tblkpprofitcentre fk_tblkpprofitcentre_division; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblkpprofitcentre
    ADD CONSTRAINT fk_tblkpprofitcentre_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname);
