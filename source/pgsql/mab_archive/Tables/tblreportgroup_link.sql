CREATE TABLE IF NOT EXISTS mabarchive.tblreportgroup_link (
    reportid integer NOT NULL,
    groupid integer NOT NULL,
    CONSTRAINT pk_tblreportgroup_link PRIMARY KEY (reportid, groupid)
);

-- Foreign keys for mabarchive.tblreportgroup_link
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblreportgroup_link_tblreportgroup'
          AND conrelid = 'mabarchive.tblreportgroup_link'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblreportgroup_link
            ADD CONSTRAINT fk_tblreportgroup_link_tblreportgroup FOREIGN KEY (groupid) REFERENCES mabarchive.tblreportgroup(groupid);
    END IF;
END $$;
