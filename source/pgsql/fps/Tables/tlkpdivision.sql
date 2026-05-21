CREATE TABLE IF NOT EXISTS fps.tlkpdivision (
    divisionid integer,
    agencyid integer NOT NULL,
    divname character varying(10) NOT NULL,
    centoverhead money DEFAULT 0,
    CONSTRAINT pk__tlkpdivision__10566f31 PRIMARY KEY (divname)
);

-- Foreign keys for fps.tlkpdivision
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tlkpdivision_agencyid'
          AND conrelid = 'fps.tlkpdivision'::regclass
    ) THEN
        ALTER TABLE fps.tlkpdivision
            ADD CONSTRAINT fk_tlkpdivision_agencyid FOREIGN KEY (agencyid) REFERENCES fps.tlkpagency(agencyid);
    END IF;
END $$;
