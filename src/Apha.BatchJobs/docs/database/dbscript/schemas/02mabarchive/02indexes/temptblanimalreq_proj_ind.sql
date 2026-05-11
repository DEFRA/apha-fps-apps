-- Index: mabarchive.temptblanimalreq_proj_ind  (on temptblanimalreq)

CREATE INDEX temptblanimalreq_proj_ind ON mabarchive.temptblanimalreq USING btree (project, year, animaltype);
