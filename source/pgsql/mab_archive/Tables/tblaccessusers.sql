CREATE TABLE IF NOT EXISTS mabarchive.tblaccessusers (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    username character varying(50),
    dt2login character varying(50),
    CONSTRAINT pk_tblaccessusers PRIMARY KEY (systemid, ntlogin)
);

-- Foreign keys for mabarchive.tblaccessusers
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblaccessusers_tblaccesssystems'
          AND conrelid = 'mabarchive.tblaccessusers'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblaccessusers
            ADD CONSTRAINT fk_tblaccessusers_tblaccesssystems FOREIGN KEY (systemid) REFERENCES mabarchive.tblaccesssystems(systemid);
    END IF;
END $$;
