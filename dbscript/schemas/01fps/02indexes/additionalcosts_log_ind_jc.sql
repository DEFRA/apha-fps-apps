-- Index: fps.additionalcosts_log_ind_jc  (on additionalcosts_log)

CREATE INDEX additionalcosts_log_ind_jc ON fps.additionalcosts_log USING btree (jobcode);
