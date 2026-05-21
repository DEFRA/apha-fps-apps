CREATE TABLE IF NOT EXISTS mabarchive.temptblprojectyear (
    project integer DEFAULT 0 NOT NULL,
    yearno integer NOT NULL,
    CONSTRAINT pk_temptblprojectyear PRIMARY KEY (project, yearno)
);

-- Foreign keys for mabarchive.temptblprojectyear
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_temptblprojectyear_temptblproject'
          AND conrelid = 'mabarchive.temptblprojectyear'::regclass
    ) THEN
        ALTER TABLE mabarchive.temptblprojectyear
            ADD CONSTRAINT fk_temptblprojectyear_temptblproject FOREIGN KEY (project) REFERENCES mabarchive.temptblproject(project);
    END IF;
END $$;
