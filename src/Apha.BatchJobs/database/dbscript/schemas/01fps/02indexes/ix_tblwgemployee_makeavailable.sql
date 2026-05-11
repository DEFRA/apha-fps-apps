-- Index: fps.ix_tblwgemployee_makeavailable  (on tblwgemployee)

CREATE INDEX ix_tblwgemployee_makeavailable ON fps.tblwgemployee USING btree (makeavailable) INCLUDE (pactid, spnumber, workgroupgrade);
