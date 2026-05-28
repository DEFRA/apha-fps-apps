CREATE TABLE IF NOT EXISTS mabarchive.tblpublicationproject (
    publicationuid integer NOT NULL,
    parentproject character varying(20) NOT NULL,
    CONSTRAINT pk_tblpublicationproject PRIMARY KEY (publicationuid, parentproject)
);

-- Foreign keys for mabarchive.tblpublicationproject
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblpublicationproject_tblpublication'
          AND conrelid = 'mabarchive.tblpublicationproject'::regclass
    ) THEN
        ALTER TABLE mabarchive.tblpublicationproject
            ADD CONSTRAINT fk_tblpublicationproject_tblpublication FOREIGN KEY (publicationuid) REFERENCES mabarchive.tblpublication(uid);
    END IF;
END $$;
