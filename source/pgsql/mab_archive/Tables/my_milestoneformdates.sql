CREATE TABLE IF NOT EXISTS mabarchive.my_milestoneformdates (
    year smallint NOT NULL,
    parentproject character varying(20) NOT NULL,
    jan timestamp without time zone,
    feb timestamp without time zone,
    mar timestamp without time zone,
    apr timestamp without time zone,
    may timestamp without time zone,
    jun timestamp without time zone,
    jul timestamp without time zone,
    aug timestamp without time zone,
    sep timestamp without time zone,
    oct timestamp without time zone,
    nov timestamp without time zone,
    dec timestamp without time zone,
    CONSTRAINT pk_my_milestoneformdates PRIMARY KEY (year, parentproject)
);

-- Foreign keys for mabarchive.my_milestoneformdates
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_my_milestoneformdates_g_tlkpproject_radtrackdata'
          AND conrelid = 'mabarchive.my_milestoneformdates'::regclass
    ) THEN
        ALTER TABLE mabarchive.my_milestoneformdates
            ADD CONSTRAINT fk_my_milestoneformdates_g_tlkpproject_radtrackdata FOREIGN KEY (parentproject) REFERENCES mabarchive.g_tlkpproject_radtrackdata(parentproject);
    END IF;
END $$;
