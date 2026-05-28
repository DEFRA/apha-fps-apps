CREATE TABLE IF NOT EXISTS fps.tblkpprofitcentre (
    profitcentre character varying(50) NOT NULL,
    profitcentrename character varying(40) NOT NULL,
    division character varying(10) DEFAULT 0 NOT NULL,
    conttarget money DEFAULT 0,
    profitcentrehead character varying(50),
    divisionid integer DEFAULT 0,
    email_recipient character varying(50),
    timesheetlayout smallint,
    timesheet integer,
    outputsheet integer,
    pactcoordinatoremailname character varying(50),
    highlevelsummary bytea,
    CONSTRAINT pk__tblkpprofitcentr__1db06a4f PRIMARY KEY (profitcentre)
);

-- Foreign keys for fps.tblkpprofitcentre
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_tblkpprofitcentre_division'
          AND conrelid = 'fps.tblkpprofitcentre'::regclass
    ) THEN
        ALTER TABLE fps.tblkpprofitcentre
            ADD CONSTRAINT fk_tblkpprofitcentre_division FOREIGN KEY (division) REFERENCES fps.tlkpdivision(divname);
    END IF;
END $$;
