-- Table: fps.tblpaymentschedule

CREATE TABLE fps.tblpaymentschedule (
    contract citext NOT NULL,
    duedate timestamp without time zone NOT NULL,
    paid smallint NOT NULL,
    CONSTRAINT pk___1__10 PRIMARY KEY (contract, duedate),
    CONSTRAINT fk_tblpaymentschedule_contract FOREIGN KEY (contract) REFERENCES fps.tblcontract(contractno)
);

