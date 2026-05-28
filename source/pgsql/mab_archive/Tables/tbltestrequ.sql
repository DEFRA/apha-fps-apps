CREATE TABLE IF NOT EXISTS mabarchive.tbltestrequ (
    project character varying(50) NOT NULL,
    year integer DEFAULT 0 NOT NULL,
    testcode character varying(50) NOT NULL,
    notests double precision DEFAULT 0,
    unitprice double precision DEFAULT 0,
    CONSTRAINT pk_tbltestrequ PRIMARY KEY (project, year, testcode)
);

-- Foreign keys for mabarchive.tbltestrequ
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tbltestrequ_tblprojectyear'
          AND conrelid = 'mabarchive.tbltestrequ'::regclass
    ) THEN
        ALTER TABLE mabarchive.tbltestrequ
            ADD CONSTRAINT fk_tbltestrequ_tblprojectyear FOREIGN KEY (year, project) REFERENCES mabarchive.tblprojectyear(yearno, project);
    END IF;
END $$;
