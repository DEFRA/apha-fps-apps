CREATE TABLE IF NOT EXISTS fps.tblstagingmonthlyoutput (
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    month double precision NOT NULL,
    workgroup character varying(50) NOT NULL,
    volume double precision,
    failurecomments character varying,
    passed boolean
);
