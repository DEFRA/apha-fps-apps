-- Index: fps.staffjob_log_ind_dt  (on staffjob_log)

CREATE INDEX staffjob_log_ind_dt ON fps.staffjob_log USING btree (date_time);
