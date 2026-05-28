CREATE TABLE IF NOT EXISTS mabarchive.tblaccessusers_levels (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    accesslevelid integer NOT NULL,
    CONSTRAINT pk_tblaccessusers_levels PRIMARY KEY (systemid, ntlogin, accesslevelid)
);

-- Foreign keys for mabarchive.tblaccessusers_levels
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblaccessusers_levels_tblaccesslevels'
          AND conrelid = 'mabarchive.tblaccessusers_levels'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblaccessusers_levels
            ADD CONSTRAINT fk_tblaccessusers_levels_tblaccesslevels FOREIGN KEY (systemid, accesslevelid) REFERENCES mabarchive.tblaccesslevels(systemid, accesslevelid);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblaccessusers_levels_tblaccessusers'
          AND conrelid = 'mabarchive.tblaccessusers_levels'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblaccessusers_levels
            ADD CONSTRAINT fk_tblaccessusers_levels_tblaccessusers FOREIGN KEY (systemid, ntlogin) REFERENCES mabarchive.tblaccessusers(systemid, ntlogin);
    END IF;
END $$;
