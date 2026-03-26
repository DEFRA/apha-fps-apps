-- Index: fps.project_log_ind_jc  (on project_log)

CREATE INDEX project_log_ind_jc ON fps.project_log USING btree (jobcode);
