-- Index: mabarchive.tblanimalreq_proj_ind  (on tblanimalreq)

CREATE INDEX tblanimalreq_proj_ind ON mabarchive.tblanimalreq USING btree (project, year, animaltype);
