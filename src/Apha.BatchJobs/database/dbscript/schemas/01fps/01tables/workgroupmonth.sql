-- Table: fps.workgroupmonth

CREATE TABLE fps.workgroupmonth (
    workgroup character varying(50) NOT NULL,
    month double precision NOT NULL,
    runningcost money,
    runcostprofile money,
    fpsyear integer
);

