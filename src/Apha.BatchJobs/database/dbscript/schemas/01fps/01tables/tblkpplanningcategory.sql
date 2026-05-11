-- Table: fps.tblkpplanningcategory

CREATE TABLE fps.tblkpplanningcategory (
    planningcategory citext NOT NULL,
    plancategorydesc character varying(50),
    customergroup character varying(50),
    corporate character varying(50),
    divisional character varying(50),
    CONSTRAINT pk__tblkpplanningcat__05b8e52d PRIMARY KEY (planningcategory)
);

