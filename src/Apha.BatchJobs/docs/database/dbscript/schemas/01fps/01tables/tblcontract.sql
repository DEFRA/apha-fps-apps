-- Table: fps.tblcontract

CREATE TABLE fps.tblcontract (
    contractno citext NOT NULL,
    category citext NOT NULL,
    manager character varying(50),
    customer citext,
    title character varying(100),
    registereddate date,
    startdate date,
    enddate date,
    contractdoc bytea,
    duration integer,
    fpsyear integer,
    CONSTRAINT pk___2__10 PRIMARY KEY (contractno),
    CONSTRAINT fk_tblcontract_3__10 FOREIGN KEY (category) REFERENCES fps.tblcategory(category),
    CONSTRAINT fk_tblcontract_customer FOREIGN KEY (customer) REFERENCES fps.tlkpcustomer(customer)
);

