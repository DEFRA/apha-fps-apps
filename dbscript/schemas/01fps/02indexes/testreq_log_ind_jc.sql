-- Index: fps.testreq_log_ind_jc  (on testreq_log)

CREATE INDEX testreq_log_ind_jc ON fps.testreq_log USING btree (jobcode);
