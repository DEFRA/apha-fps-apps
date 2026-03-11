-- Index: mabarchive.tblprojectyeartblanimalreq  (on tblanimalreq)

CREATE INDEX tblprojectyeartblanimalreq ON mabarchive.tblanimalreq USING btree (project, year);
