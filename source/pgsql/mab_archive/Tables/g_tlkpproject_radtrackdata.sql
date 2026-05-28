CREATE TABLE IF NOT EXISTS mabarchive.g_tlkpproject_radtrackdata (
    parentproject character varying(20) NOT NULL,
    version character varying(10),
    fileref character varying(20),
    customerref character varying(20),
    startdate timestamp without time zone,
    enddate timestamp without time zone,
    finalreportreceived timestamp without time zone,
    finalreportsent timestamp without time zone,
    inflation smallint DEFAULT 0,
    closeddate timestamp without time zone,
    useprojectyear smallint DEFAULT 0 NOT NULL,
    status character varying(50),
    pcforecastspend double precision,
    riskid integer,
    costbooknumber character varying(10),
    revisedenddate timestamp without time zone,
    formrequired boolean DEFAULT true NOT NULL,
    overallcustincome money,
    CONSTRAINT pk_g_tlkpproject_radtrackdata PRIMARY KEY (parentproject)
);

-- Foreign keys for mabarchive.g_tlkpproject_radtrackdata
DO $$ BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_g_tlkpproject_radtrackdata_tlkprisk'
          AND conrelid = 'mabarchive.g_tlkpproject_radtrackdata'::regclass
    ) THEN
        ALTER TABLE mabarchive.g_tlkpproject_radtrackdata
            ADD CONSTRAINT fk_g_tlkpproject_radtrackdata_tlkprisk FOREIGN KEY (riskid) REFERENCES mabarchive.tlkprisk(riskid);
    END IF;
END $$;
