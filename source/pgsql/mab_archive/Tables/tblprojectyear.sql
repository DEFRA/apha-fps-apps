CREATE TABLE IF NOT EXISTS mabarchive.tblprojectyear (
    project character varying(50) NOT NULL,
    yearno integer NOT NULL,
    markup_time double precision,
    markup_tests double precision,
    markup_animals double precision,
    markup_additional double precision,
    profit_time double precision,
    profit_tests double precision,
    profit_animals double precision,
    profit_additional double precision,
    CONSTRAINT pk_tblprojectyear PRIMARY KEY (project, yearno)
);

-- Foreign keys for mabarchive.tblprojectyear
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblprojectyear_tblproject'
          AND conrelid = 'mabarchive.tblprojectyear'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblprojectyear
            ADD CONSTRAINT fk_tblprojectyear_tblproject FOREIGN KEY (project) REFERENCES mabarchive.tblproject(project);
    END IF;
END $$;
