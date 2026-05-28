CREATE TABLE IF NOT EXISTS mabarchive.tblimages (
    imageid integer NOT NULL,
    image bytea,
    decription character varying(50),
    CONSTRAINT pk_tblimages PRIMARY KEY (imageid)
);
