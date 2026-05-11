-- Table: fps.tlkptestcapability

CREATE TABLE fps.tlkptestcapability (
    testcode citext NOT NULL,
    workgroup citext NOT NULL,
    planportfolio citext NOT NULL,
    unitcost money DEFAULT 0,
    predoutturn double precision DEFAULT 0,
    sop character varying(50),
    smscode character varying(50),
    fpsyear integer,
    CONSTRAINT pk__tlkptestcapabili__4e53a1aa PRIMARY KEY (testcode, workgroup),
    CONSTRAINT fk_tlkptestcapability_1__15 FOREIGN KEY (workgroup) REFERENCES fps.workgroup(workgroup),
    CONSTRAINT fk_tlkptestcapability_1__18 FOREIGN KEY (planportfolio) REFERENCES fps.tlkpproject(parentproject),
    CONSTRAINT fk_tlkptestcapability_2__18 FOREIGN KEY (testcode) REFERENCES fps.testorproduct(itemcode)
);

