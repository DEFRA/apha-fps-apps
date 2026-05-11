-- Table: fps.costcentre

CREATE TABLE fps.costcentre (
    costcentre double precision NOT NULL,
    profitcentre citext NOT NULL,
    fpsyear integer,
    CONSTRAINT pk_costcentre_1 PRIMARY KEY (costcentre),
    CONSTRAINT fk_costcentre_profitcentre FOREIGN KEY (profitcentre) REFERENCES fps.tblkpprofitcentre(profitcentre)
);

