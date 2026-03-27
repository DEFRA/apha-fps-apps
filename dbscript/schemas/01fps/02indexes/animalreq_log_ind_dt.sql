-- Index: fps.animalreq_log_ind_dt  (on animalreq_log)

CREATE INDEX animalreq_log_ind_dt ON fps.animalreq_log USING btree (date_time);
