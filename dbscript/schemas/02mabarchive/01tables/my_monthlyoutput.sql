-- Table: mabarchive.my_monthlyoutput

CREATE TABLE mabarchive.my_monthlyoutput (
    year smallint NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    month double precision NOT NULL,
    workgroup character varying(50) NOT NULL,
    volume double precision,
    wgbuyer character varying(50),
    CONSTRAINT pk_my_monthlyoutput PRIMARY KEY (year, testcode, buyer, month, workgroup)
);

