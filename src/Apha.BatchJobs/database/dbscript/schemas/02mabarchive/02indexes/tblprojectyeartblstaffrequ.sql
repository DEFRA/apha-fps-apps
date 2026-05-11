-- Index: mabarchive.tblprojectyeartblstaffrequ  (on tblstaffrequ)

CREATE INDEX tblprojectyeartblstaffrequ ON mabarchive.tblstaffrequ USING btree (project, year);
