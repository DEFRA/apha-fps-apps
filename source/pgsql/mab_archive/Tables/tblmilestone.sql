CREATE TABLE IF NOT EXISTS mabarchive.tblmilestone (
    project character varying(20) NOT NULL,
    number character varying(10) NOT NULL,
    description character varying(500),
    datedue timestamp without time zone NOT NULL,
    datecompleted timestamp without time zone,
    dateformreceived timestamp without time zone,
    undersdreview smallint DEFAULT 0,
    ontarget smallint DEFAULT 0,
    projectleadercomment character varying,
    capscomment character varying(250),
    idtype character(1),
    CONSTRAINT pk_tblmilestone PRIMARY KEY (project, number)
);

-- Foreign keys for mabarchive.tblmilestone
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblmilestone_g_tlkpproject_radtrackdata'
          AND conrelid = 'mabarchive.tblmilestone'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblmilestone
            ADD CONSTRAINT fk_tblmilestone_g_tlkpproject_radtrackdata FOREIGN KEY (project) REFERENCES mabarchive.g_tlkpproject_radtrackdata(parentproject);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblmilestone_tlkpmilestonetype'
          AND conrelid = 'mabarchive.tblmilestone'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblmilestone
            ADD CONSTRAINT fk_tblmilestone_tlkpmilestonetype FOREIGN KEY (idtype) REFERENCES mabarchive.tlkpmilestonetype(idtype);
    END IF;
END $$;
