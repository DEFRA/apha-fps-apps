-- Table: fps.tblyearmaster
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblyearmaster; Type: TABLE; Schema: fps; Owner: -
CREATE TABLE fps.tblyearmaster (
    fpsyear integer NOT NULL,
    fpsyearcode character varying(20) NOT NULL,
    yearstatus character varying(10) NOT NULL,
    remarks text,
    active boolean DEFAULT true NOT NULL,
    createdon timestamp with time zone DEFAULT now() NOT NULL,
    createdby character varying(100),
    CONSTRAINT ck_tblyearmaster_yearstatus CHECK (((yearstatus)::text = ANY (ARRAY[('Open'::character varying)::text, ('Closed'::character varying)::text, ('Planned'::character varying)::text])))
);
-- Name: TABLE tblyearmaster; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON TABLE fps.tblyearmaster IS 'Master table of fiscal / FPS years. Defines which years exist, their display codes, and their lifecycle status (Open, Closed, Planned).';
-- Name: COLUMN tblyearmaster.fpsyear; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.fpsyear IS 'Four-digit calendar year that starts the fiscal period (e.g. 2025 for Apr 2025 â€“ Mar 2026). Primary key.';
-- Name: COLUMN tblyearmaster.fpsyearcode; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.fpsyearcode IS 'Human-readable fiscal-year label, e.g. FPS2025-26.';
-- Name: COLUMN tblyearmaster.yearstatus; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.yearstatus IS 'Lifecycle state: Open (transactions allowed), Closed (read-only), or Planned (configuration only).';
-- Name: COLUMN tblyearmaster.remarks; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.remarks IS 'Free-text note explaining the status or special conditions.';
-- Name: COLUMN tblyearmaster.active; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.active IS 'Soft-delete flag. TRUE = visible to the application.';
-- Name: COLUMN tblyearmaster.createdon; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.createdon IS 'Timestamp when the row was first inserted (auto-set).';
-- Name: COLUMN tblyearmaster.createdby; Type: COMMENT; Schema: fps; Owner: -
COMMENT ON COLUMN fps.tblyearmaster.createdby IS 'User or service account that created the row.';
-- Name: tblyearmaster pk_tblyearmaster; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblyearmaster
    ADD CONSTRAINT pk_tblyearmaster PRIMARY KEY (fpsyear);
-- Name: tblyearmaster uq_tblyearmaster_fpsyearcode; Type: CONSTRAINT; Schema: fps; Owner: -
ALTER TABLE ONLY fps.tblyearmaster
    ADD CONSTRAINT uq_tblyearmaster_fpsyearcode UNIQUE (fpsyearcode);
