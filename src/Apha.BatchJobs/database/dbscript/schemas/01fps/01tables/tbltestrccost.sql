-- Table: fps.tbltestrccost

CREATE TABLE fps.tbltestrccost (
    testcode citext NOT NULL,
    profitcentre citext NOT NULL,
    price money DEFAULT 0 NOT NULL,
    fpsyear integer,
    CONSTRAINT pk_tbltestrccost PRIMARY KEY (testcode, profitcentre),
    CONSTRAINT fk_tbltestrccost_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre),
    CONSTRAINT fk_tbltestrccost_testcode FOREIGN KEY (testcode) REFERENCES fps.testorproduct(itemcode)
);

