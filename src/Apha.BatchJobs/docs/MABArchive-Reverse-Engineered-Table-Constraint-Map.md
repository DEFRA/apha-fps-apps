# MABArchive Reverse-Engineered Table + Constraint Map (Current Local Snapshot)

Date: 2026-05-04
Scope: practical "for now" map focused only on MABArchive read/write paths.

## 1) Source tables read by MABArchive loaders

From loader SQL implementation, the process reads the following FPS tables:

1. fps.tlkpprogram
2. fps.tlkpproject
3. fps.fpsyeartotals
4. fps.monthlyoutput
5. fps.monthlytime
6. fps.proj_invoice
7. fps.proj_subcontract
8. fps.projectmonthfinal
9. fps.tbladditionalcosts
10. fps.tblanimalreq
11. fps.tblcontract
12. fps.tblstaffjob
13. fps.timecostcalcs
14. fps.tlkptestreqmt
15. fps.workgroupgrade
16. fps.profitcentregrade
17. fps.tblkpprofitcentre
18. fps.testorproduct
19. fps.tblwgemployee
20. fps.tblemployee
21. fps.workgroup
22. fps.tblanimals
23. fps.tbldb_variables

Runtime gating also reads:

24. fps.tblyearmaster

## 2) Target tables written by MABArchive loaders

1. mabarchive.my_tlkpprogram
2. mabarchive.g_tlkpproject
3. mabarchive.my_tlkpproject
4. mabarchive.my_fpsyeartotals
5. mabarchive.my_monthlyoutput
6. mabarchive.my_monthlytime
7. mabarchive.my_proj_invoice
8. mabarchive.my_proj_subcontract
9. mabarchive.my_projectmonthfinal
10. mabarchive.my_tbladditionalcosts
11. mabarchive.my_tblanimalreq
12. mabarchive.my_tblcontract
13. mabarchive.my_tblstaffjob
14. mabarchive.my_timecostcalcs
15. mabarchive.my_tlkptestreqmt
16. mabarchive.tlkpyear
17. mabarchive.my_workgroupgrade
18. mabarchive.my_profitcentregrade
19. mabarchive.my_tblprofitcentre
20. mabarchive.my_testorproduct
21. mabarchive.my_staff
22. mabarchive.my_workgroup
23. mabarchive.my_tblanimals
24. mabarchive.my_tlkpproject_all

## 3) Constraints currently present (observed in local DB)

### 3.1 Source-side primary keys (good)

The 21 year-bearing core FPS source tables now have composite primary keys including fpsyear.
Examples:
- fps.tlkpproject: PRIMARY KEY (fpsyear, parentproject)
- fps.monthlyoutput: PRIMARY KEY (fpsyear, testcode, buyer, month, workgroup)
- fps.timecostcalcs: PRIMARY KEY (fpsyear, workgroup, jobcode, project, month, staffid)

### 3.2 Source-side foreign keys currently enforced

1. fps.tlkpproject (fpsyear, program) -> fps.tlkpprogram (fpsyear, programno)
2. fps.tbladditionalcosts (fpsyear, jobcode) -> fps.tlkpproject (fpsyear, parentproject)
3. fps.tblanimalreq (fpsyear, jobcode) -> fps.tlkpproject (fpsyear, parentproject)
4. fps.tblanimalreq (fpsyear, animaltype) -> fps.tblanimals (fpsyear, animaltype)
5. fps.tblstaffjob (fpsyear, jobcode) -> fps.tlkpproject (fpsyear, parentproject)
6. fps.tblstaffjob (fpsyear, staffid) -> fps.tblwgemployee (fpsyear, pactid)
7. fps.monthlytime (fpsyear, parentproject) -> fps.tlkpproject (fpsyear, parentproject)
8. fps.monthlytime (fpsyear, pactstaffid) -> fps.tblwgemployee (fpsyear, pactid)
9. fps.monthlyoutput (fpsyear, testcode, buyer) -> fps.tlkptestreqmt (fpsyear, testcode, buyer)
10. fps.workgroupgrade (fpsyear, workgroup) -> fps.workgroup (fpsyear, workgroup)

### 3.3 Target-side constraints currently enforced

All 24 MABArchive target tables currently have primary keys.
No foreign keys were observed on MABArchive tables in local snapshot.

## 4) Reverse-engineered "needed now" constraints for MABArchive correctness

This is the minimum practical set to keep the MABArchive path stable in local testing.

### 4.1 Must-have (already present)

- Composite PKs on all 21 year-bearing FPS source tables.
- The 10 source FKs listed above.
- Primary keys on all 24 MABArchive target tables.
- PK on fps.tblyearmaster(fpsyear).
- PK on fps.tbldb_variables(db_var_name).

### 4.2 High-value constraints still missing for data quality

These are not required for the process to run, but they protect against cross-year joins and orphaned dimensional data.

1. fps.tblwgemployee (fpsyear, spnumber) -> fps.tblemployee (fpsyear, spnumber)
   - Important because my_staff loader joins tblwgemployee to tblemployee.

2. fps.tblwgemployee (fpsyear, workgroupgrade) -> fps.workgroupgrade (fpsyear, wggrade)
   - Important for staff-to-grade consistency used in my_staff output.

3. fps.workgroupgrade (fpsyear, profitcentregrade) -> fps.profitcentregrade (fpsyear, pcgrade)
   - Important for grade hierarchy consistency used by loaders 17/18.

4. fps.profitcentregrade (profitcentre) -> fps.tblkpprofitcentre (profitcentre)
   - Non-year lookup integrity for loader 19 relationship chain.

5. Optional target-side FKs in mabarchive (if desired for stricter local integrity)
   - Not required for parity runs because delete/load order already enforces practical consistency.

## 5) Known blockers for fully restoring source FK model

Several candidate FKs from project-related tables cannot be recreated yet because of type mismatch:
- parent key: fps.tlkpproject.parentproject is citext
- child columns in some tables are varchar (for example project/jobcode fields)

This mismatch blocks direct composite FK creation until column types are aligned.

## 6) Actionable local recommendation (for now)

Use this as the "for now" policy for MABArchive work:

1. Keep current composite PK model and existing 10 source FKs.
2. Add the 4 high-value missing FKs listed in section 4.2 (type-compatible ones first).
3. Do not force project/jobcode FKs where child is varchar and parent is citext.
4. Keep MABArchive target tables PK-only unless you specifically need strict relational enforcement there.

This gives practical integrity with minimal extra migration risk.
