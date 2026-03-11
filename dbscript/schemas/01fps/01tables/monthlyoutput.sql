-- Table: fps.monthlyoutput

CREATE TABLE fps.monthlyoutput (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    month double precision NOT NULL,
    workgroup citext NOT NULL,
    volume double precision,
    wgbuyer character varying(50),
    fpsyear integer,
    CONSTRAINT pk_monthlyoutput PRIMARY KEY (testcode, buyer, month, workgroup),
    CONSTRAINT fk_monthlyoutput_testcode_buyer FOREIGN KEY (testcode, buyer) REFERENCES fps.tlkptestreqmt(testcode, buyer),
    CONSTRAINT fk_monthlyoutput_testcode_workgroup FOREIGN KEY (testcode, workgroup) REFERENCES fps.tlkptestcapability(testcode, workgroup)
);

