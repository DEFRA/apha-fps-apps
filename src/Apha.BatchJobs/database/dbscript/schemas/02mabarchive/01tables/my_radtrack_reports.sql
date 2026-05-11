-- Table: mabarchive.my_radtrack_reports

CREATE TABLE mabarchive.my_radtrack_reports (
    year smallint NOT NULL,
    project character varying(20) NOT NULL,
    type character varying(10) NOT NULL,
    reminder1 date,
    reminder2 date,
    replyreceived date,
    senttoprogmanager date,
    senttoprojleader date,
    emailedtocustomer date,
    signedcopytocustomer date,
    repduedate date,
    id integer DEFAULT nextval('mabarchive.my_radtrack_reports_id_seq'::regclass) NOT NULL,
    reportagreeddate date,
    CONSTRAINT pk_my_radtrack_reports PRIMARY KEY (id)
);

COMMENT ON COLUMN mabarchive.my_radtrack_reports.id IS $$Converted from IDENTITY(1,1) in MSSQL to serial in PostgreSQL$$;
