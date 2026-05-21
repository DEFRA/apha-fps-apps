CREATE TABLE IF NOT EXISTS fps.tblstagingmonthlytime (
    pactstaffid character varying(50),
    timecode character varying(50),
    parentproject character varying(20),
    month double precision,
    workgroup character varying(50),
    hours double precision,
    failurecomments character varying,
    passed boolean,
    pactid character varying(50),
    newworkgroup character varying(50),
    oldtestcode character varying(20),
    name character varying(50)
);
