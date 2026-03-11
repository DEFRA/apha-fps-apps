-- Index: fps.idx_resourcecentremonth_pk  (on resourcecentremonth)

CREATE INDEX idx_resourcecentremonth_pk ON fps.resourcecentremonth USING btree (resourcecentre, monthno);
