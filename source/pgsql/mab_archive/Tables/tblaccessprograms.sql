CREATE TABLE IF NOT EXISTS mabarchive.tblaccessprograms (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    program character varying(10) NOT NULL,
    CONSTRAINT pk_tblaccessprograms PRIMARY KEY (systemid, ntlogin, program)
);

-- Foreign keys for mabarchive.tblaccessprograms
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblaccessprograms_tblaccessusers'
          AND conrelid = 'mabarchive.tblaccessprograms'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblaccessprograms
            ADD CONSTRAINT fk_tblaccessprograms_tblaccessusers FOREIGN KEY (systemid, ntlogin) REFERENCES mabarchive.tblaccessusers(systemid, ntlogin);
    END IF;
END $$;
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblaccessprograms_tblradtrackprog'
          AND conrelid = 'mabarchive.tblaccessprograms'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblaccessprograms
            ADD CONSTRAINT fk_tblaccessprograms_tblradtrackprog FOREIGN KEY (program) REFERENCES mabarchive.tblradtrackprog(program);
    END IF;
END $$;
