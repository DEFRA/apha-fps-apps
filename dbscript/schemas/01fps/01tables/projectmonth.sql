-- Table: fps.projectmonth

CREATE TABLE fps.projectmonth (
    project character varying(20) NOT NULL,
    monthno integer NOT NULL,
    costprofile money,
    fpsyear integer,
    CONSTRAINT pk_projectmonth_1__16 PRIMARY KEY (project, monthno)
);

