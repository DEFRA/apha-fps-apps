-- Index: mabarchive.tblprojectyeartbltestrequ  (on tbltestrequ)

CREATE INDEX tblprojectyeartbltestrequ ON mabarchive.tbltestrequ USING btree (project, year);
