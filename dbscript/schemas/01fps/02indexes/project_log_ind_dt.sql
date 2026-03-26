-- Index: fps.project_log_ind_dt  (on project_log)

CREATE INDEX project_log_ind_dt ON fps.project_log USING btree (date_time);
