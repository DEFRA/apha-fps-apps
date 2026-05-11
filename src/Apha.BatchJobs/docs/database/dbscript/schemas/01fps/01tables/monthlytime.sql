-- Table: fps.monthlytime

CREATE TABLE fps.monthlytime (
    pactstaffid citext NOT NULL,
    timecode citext NOT NULL,
    month double precision NOT NULL,
    parentproject citext NOT NULL,
    workgroup citext,
    hours double precision,
    fpsyear integer,
    CONSTRAINT pk_monthlytime PRIMARY KEY (pactstaffid, timecode, month, parentproject),
    CONSTRAINT fk_monthlytime_2__10 FOREIGN KEY (workgroup, timecode, parentproject) REFERENCES fps.timecodevalid(workgroup, timecode, parentproject),
    CONSTRAINT fk_monthlytime_pactstaffid FOREIGN KEY (pactstaffid) REFERENCES fps.tblwgemployee(pactid)
);

