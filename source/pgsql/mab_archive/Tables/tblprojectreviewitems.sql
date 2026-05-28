CREATE TABLE IF NOT EXISTS mabarchive.tblprojectreviewitems (
    project character varying(50) NOT NULL,
    itemid integer NOT NULL,
    frequencyid integer,
    CONSTRAINT pk_tblprojectreviewitems PRIMARY KEY (project, itemid)
);

-- Foreign keys for mabarchive.tblprojectreviewitems
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblprojectreviewitems_tlkpfrequency'
          AND conrelid = 'mabarchive.tblprojectreviewitems'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblprojectreviewitems
            ADD CONSTRAINT fk_tblprojectreviewitems_tlkpfrequency FOREIGN KEY (frequencyid) REFERENCES mabarchive.tlkpfrequency(frequencyid);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblprojectreviewitems_tlkpreviewitem'
          AND conrelid = 'mabarchive.tblprojectreviewitems'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblprojectreviewitems
            ADD CONSTRAINT fk_tblprojectreviewitems_tlkpreviewitem FOREIGN KEY (itemid) REFERENCES mabarchive.tlkpreviewitem(itemid);
    END IF;
END $$;
