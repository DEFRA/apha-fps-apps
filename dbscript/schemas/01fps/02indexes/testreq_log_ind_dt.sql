-- Index: fps.testreq_log_ind_dt  (on testreq_log)

CREATE INDEX testreq_log_ind_dt ON fps.testreq_log USING btree (date_time);
