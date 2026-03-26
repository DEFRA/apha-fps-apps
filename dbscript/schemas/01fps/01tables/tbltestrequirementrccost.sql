-- Table: fps.tbltestrequirementrccost

CREATE TABLE fps.tbltestrequirementrccost (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    profitcentre citext NOT NULL,
    price money NOT NULL,
    fpsyear integer,
    CONSTRAINT pk_tbltestrequirementrccost PRIMARY KEY (testcode, buyer, profitcentre),
    CONSTRAINT fk_tbltestrequirementrccost_testcode_buyer FOREIGN KEY (testcode, buyer) REFERENCES fps.tlkptestreqmt(testcode, buyer),
    CONSTRAINT fk_tbltestrequirementrccost_testcode_profitcentre FOREIGN KEY (testcode, profitcentre) REFERENCES fps.tbltestrccost(testcode, profitcentre)
);

