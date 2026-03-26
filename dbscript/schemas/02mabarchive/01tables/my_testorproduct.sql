-- Table: mabarchive.my_testorproduct

CREATE TABLE mabarchive.my_testorproduct (
    year smallint NOT NULL,
    itemcode character varying(20) NOT NULL,
    itemdescription character varying(200),
    testmanager character varying(50),
    jobstatus character varying(2),
    unitpricevla money,
    priceahvg money,
    owner character varying(2),
    chargemethod character varying(5),
    shortdescription character(18),
    defraunitprice money,
    CONSTRAINT pk_my_testorproduct PRIMARY KEY (year, itemcode)
);

