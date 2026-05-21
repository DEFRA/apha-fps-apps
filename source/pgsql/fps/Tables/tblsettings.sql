-- Table: fps.tblsettings
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblsettings; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblsettings (
    id character varying(50) NOT NULL,
    setting character varying(255),
    notes text,
    fpsyear integer NOT NULL,
    updated_by character varying(100),
    updated_at timestamp with time zone DEFAULT now() NOT NULL
);
-- Name: TABLE tblsettings; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON TABLE fps.tblsettings IS 'Application-level configuration settings. Only business-logic constants belong here; infrastructure config moves to appsettings.json.';
-- Name: COLUMN tblsettings.id; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblsettings.id IS 'Unique setting key referenced by application code.';
-- Name: COLUMN tblsettings.setting; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblsettings.setting IS 'The setting value as text.';
-- Name: COLUMN tblsettings.notes; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblsettings.notes IS 'Free-text description of purpose, origin, and usage.';
-- Name: COLUMN tblsettings.fpsyear; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblsettings.fpsyear IS 'Fiscal year scope. NULL = not year-specific.';
-- Name: COLUMN tblsettings.updated_by; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblsettings.updated_by IS 'User or service account that last modified the row.';
-- Name: COLUMN tblsettings.updated_at; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblsettings.updated_at IS 'Timestamp of last modification (auto-set on insert).';
-- Name: tblsettings pk_tblsettings; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblsettings
    ADD CONSTRAINT pk_tblsettings PRIMARY KEY (id, fpsyear);
-- Name: tblsettings fk_tblsettings_fpsyear; Type: FK CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblsettings
    ADD CONSTRAINT fk_tblsettings_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear);
