-- Table: fps.tbluser_workgroup

CREATE TABLE fps.tbluser_workgroup (
    workgroup character varying(50) NOT NULL,
    user_id integer NOT NULL,
    CONSTRAINT pk___7__10 PRIMARY KEY (user_id, workgroup)
);

