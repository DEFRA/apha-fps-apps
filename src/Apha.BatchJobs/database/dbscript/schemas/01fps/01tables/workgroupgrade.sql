-- Table: fps.workgroupgrade

CREATE TABLE fps.workgroupgrade (
    wggrade citext NOT NULL,
    profitcentregrade citext NOT NULL,
    gradecode citext NOT NULL,
    workgroup citext NOT NULL,
    chargeratewg money,
    directratewg money DEFAULT 0,
    payratewg money DEFAULT 0,
    nprwg money DEFAULT 0,
    ohrwg money DEFAULT 0,
    avsalary money DEFAULT 0,
    hrschangedby character varying(50),
    fpsyear integer,
    CONSTRAINT pk__workgroupgrade__2de6d218 PRIMARY KEY (wggrade),
    CONSTRAINT fk_workgroupgrade_gradecode FOREIGN KEY (gradecode) REFERENCES fps.grade(gradecode),
    CONSTRAINT fk_workgroupgrade_profitcentregrade FOREIGN KEY (profitcentregrade) REFERENCES fps.profitcentregrade(pcgrade),
    CONSTRAINT fk_workgroupgrade_workgroup FOREIGN KEY (workgroup) REFERENCES fps.workgroup(workgroup)
);

