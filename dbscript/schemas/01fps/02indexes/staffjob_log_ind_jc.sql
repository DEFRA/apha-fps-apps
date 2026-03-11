-- Index: fps.staffjob_log_ind_jc  (on staffjob_log)

CREATE INDEX staffjob_log_ind_jc ON fps.staffjob_log USING btree (jobcode);
