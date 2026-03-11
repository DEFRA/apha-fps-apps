-- Table: mabarchive.tblprojectreviewitems

CREATE TABLE mabarchive.tblprojectreviewitems (
    project character varying(50) NOT NULL,
    itemid integer NOT NULL,
    frequencyid integer,
    CONSTRAINT pk_tblprojectreviewitems PRIMARY KEY (project, itemid)
);

