-- Index: mabarchive.temptblprojectyeartemptbladditionalcosts  (on temptbladditionalcosts)

CREATE INDEX temptblprojectyeartemptbladditionalcosts ON mabarchive.temptbladditionalcosts USING btree (project, year);
