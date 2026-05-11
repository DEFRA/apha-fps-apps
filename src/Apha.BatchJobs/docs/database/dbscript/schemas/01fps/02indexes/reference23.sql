-- Index: fps.reference23  (on monthlytime)

CREATE INDEX reference23 ON fps.monthlytime USING btree (workgroup, timecode, parentproject);
