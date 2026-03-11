-- Index: fps.animalreq_log_ind_jc  (on animalreq_log)

CREATE INDEX animalreq_log_ind_jc ON fps.animalreq_log USING btree (jobcode);
