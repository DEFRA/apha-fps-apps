CREATE TABLE IF NOT EXISTS mabarchive.tblaccesslevels (
    systemid integer NOT NULL,
    accesslevelid integer NOT NULL,
    accesslevel character varying(50),
    CONSTRAINT pk_tblaccesslevels PRIMARY KEY (systemid, accesslevelid)
);

-- Foreign keys for mabarchive.tblaccesslevels
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblaccesslevels_tblaccesssystems'
          AND conrelid = 'mabarchive.tblaccesslevels'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblaccesslevels
            ADD CONSTRAINT fk_tblaccesslevels_tblaccesssystems FOREIGN KEY (systemid) REFERENCES mabarchive.tblaccesssystems(systemid);
    END IF;
END $$;
