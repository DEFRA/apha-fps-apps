CREATE TABLE IF NOT EXISTS fps.tblperiodmonth (
    endmonth double precision NOT NULL,
    monthno double precision NOT NULL,
    CONSTRAINT aaaaatblkperiodmonth_pk PRIMARY KEY (endmonth, monthno)
);
