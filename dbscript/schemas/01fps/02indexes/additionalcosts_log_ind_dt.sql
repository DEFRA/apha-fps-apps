-- Index: fps.additionalcosts_log_ind_dt  (on additionalcosts_log)

CREATE INDEX additionalcosts_log_ind_dt ON fps.additionalcosts_log USING btree (date_time);
