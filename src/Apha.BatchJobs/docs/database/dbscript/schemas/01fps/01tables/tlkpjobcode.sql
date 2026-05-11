-- Table: fps.tlkpjobcode

CREATE TABLE fps.tlkpjobcode (
    jobcode character varying(50) NOT NULL,
    parentproject citext,
    jobcodeworkgroup character varying(50),
    newprog character varying(20),
    type character varying(15),
    jobcodename character varying(255),
    fpsyear integer,
    CONSTRAINT pk_tlkpjobcode_new_1__15 PRIMARY KEY (jobcode),
    CONSTRAINT tlkpjobcode_ck_tlkpjobcode_1__11 CHECK (type IS NOT NULL),
    CONSTRAINT fk_tlkpjobcode_1__11 FOREIGN KEY (parentproject) REFERENCES fps.tlkpproject(parentproject)
);

