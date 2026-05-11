-- Index: mabarchive.tblprojectyeartbladditionalcosts  (on tbladditionalcosts)

CREATE INDEX tblprojectyeartbladditionalcosts ON mabarchive.tbladditionalcosts USING btree (project, year);
