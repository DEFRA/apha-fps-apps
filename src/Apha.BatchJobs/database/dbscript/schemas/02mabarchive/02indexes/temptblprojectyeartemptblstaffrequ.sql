-- Index: mabarchive.temptblprojectyeartemptblstaffrequ  (on temptblstaffrequ)

CREATE INDEX temptblprojectyeartemptblstaffrequ ON mabarchive.temptblstaffrequ USING btree (project, year);
