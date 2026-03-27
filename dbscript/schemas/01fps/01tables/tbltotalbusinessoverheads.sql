-- Table: fps.tbltotalbusinessoverheads

CREATE TABLE fps.tbltotalbusinessoverheads (
    totalbusinessoverheads money,
    fpsyear integer,
    CONSTRAINT tb_pk UNIQUE (totalbusinessoverheads)
);

