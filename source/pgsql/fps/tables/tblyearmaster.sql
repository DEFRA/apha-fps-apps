CREATE TABLE IF NOT EXISTS fps.tblyearmaster (
    fpsyear integer NOT NULL,
    fpsyearcode varchar(20) NOT NULL,
    yearstatus varchar(10) NOT NULL,
    remarks text,
    active boolean DEFAULT true NOT NULL,
    createdon timestamptz DEFAULT now() NOT NULL,
    createdby varchar(100),
    CONSTRAINT pk_tblyearmaster PRIMARY KEY (fpsyear),
    CONSTRAINT uq_tblyearmaster_fpsyearcode UNIQUE (fpsyearcode),
    CONSTRAINT ck_tblyearmaster_yearstatus 
        CHECK (upper(yearstatus) IN ('OPEN', 'CLOSED', 'PLANNED'))
);

COMMENT ON TABLE fps.tblyearmaster IS 'Master table of fiscal / FPS years. Defines which years exist, their display codes, and their lifecycle status (Open, Closed, Planned).';
COMMENT ON COLUMN fps.tblyearmaster.fpsyear IS 'Four-digit calendar year that starts the fiscal period (e.g. 2025 for Apr 2025 Ã¢â‚¬â€œ Mar 2026). Primary key.';
COMMENT ON COLUMN fps.tblyearmaster.fpsyearcode IS 'Human-readable fiscal-year label, e.g. FPS2025-26.';
COMMENT ON COLUMN fps.tblyearmaster.yearstatus IS 'Lifecycle state: Open (transactions allowed), Closed (read-only), or Planned (configuration only).';
COMMENT ON COLUMN fps.tblyearmaster.remarks IS 'Free-text note explaining the status or special conditions.';
COMMENT ON COLUMN fps.tblyearmaster.active IS 'Soft-delete flag. TRUE = visible to the application.';
COMMENT ON COLUMN fps.tblyearmaster.createdon IS 'Timestamp when the row was first inserted (auto-set).';
COMMENT ON COLUMN fps.tblyearmaster.createdby IS 'User or service account that created the row.';
