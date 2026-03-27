-- Index: mabarchive.project_index  (on tblproposedproject)

CREATE UNIQUE INDEX project_index ON mabarchive.tblproposedproject USING btree (parentproject);
