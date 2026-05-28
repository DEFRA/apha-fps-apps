CREATE TABLE IF NOT EXISTS mabarchive.temptbltestreq (
    project integer DEFAULT 0 NOT NULL,
    year integer DEFAULT 0 NOT NULL,
    testcode character varying(50) NOT NULL,
    notests double precision DEFAULT 0,
    unitprice double precision DEFAULT 0,
    CONSTRAINT pk_temptbltestreq PRIMARY KEY (project, year, testcode)
);

-- Foreign keys for mabarchive.temptbltestreq
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_temptbltestreq_temptblprojectyear'
          AND conrelid = 'mabarchive.temptbltestreq'::regclass
    ) THEN
        ALTER TABLE mabarchive.temptbltestreq
            ADD CONSTRAINT fk_temptbltestreq_temptblprojectyear FOREIGN KEY (year, project) REFERENCES mabarchive.temptblprojectyear(yearno, project);
    END IF;
END $$;
