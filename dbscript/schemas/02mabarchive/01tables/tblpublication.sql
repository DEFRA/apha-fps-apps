-- Table: mabarchive.tblpublication

CREATE TABLE mabarchive.tblpublication (
    uid integer DEFAULT nextval('mabarchive.tblpublication_uid_seq'::regclass) NOT NULL,
    identifier character varying(50) NOT NULL,
    type character varying(3) NOT NULL,
    program character varying(10) NOT NULL,
    subject character varying(500),
    leadauthor character varying(50),
    otherauthors character varying(255),
    targetdate date,
    submitted date,
    published boolean NOT NULL,
    comments text,
    CONSTRAINT pk_tblpublication PRIMARY KEY (uid),
    CONSTRAINT ix_tblpublication UNIQUE (identifier)
);

COMMENT ON COLUMN mabarchive.tblpublication.targetdate IS $$Converted from SMALLDATETIME to DATE$$;
COMMENT ON COLUMN mabarchive.tblpublication.submitted IS $$Converted from SMALLDATETIME to DATE$$;
