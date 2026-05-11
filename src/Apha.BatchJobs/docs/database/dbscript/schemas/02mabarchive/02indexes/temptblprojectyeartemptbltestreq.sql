-- Index: mabarchive.temptblprojectyeartemptbltestreq  (on temptbltestreq)

CREATE INDEX temptblprojectyeartemptbltestreq ON mabarchive.temptbltestreq USING btree (project, year);
