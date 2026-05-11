-- Index: fps.idx_testreqlog_sequenceno  (on testreq_log)

CREATE INDEX idx_testreqlog_sequenceno ON fps.testreq_log USING btree (sequenceno) WITH (fillfactor='100', deduplicate_items='true');
