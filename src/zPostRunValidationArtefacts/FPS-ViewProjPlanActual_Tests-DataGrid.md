# DataGrid Analysis — FPS ViewProjPlanActual_Tests

## fsubCompareTests1 — "Planned Time (FPS)"

**RecordSource:** `qryCompareTests1` → `SELECT tblTestRequ.JobCode, TestCode, NoTests, TestPrice, [testprice]*[notests] AS Charge FROM tblTestRequ`
**Link:** `LinkChildFields=jobcode`, `LinkMasterFields=parentproject`

```
Grid Operations Profile — CompareTests1
AllowAdd:    true   (AllowAdditions not listed — Access default = true)
AllowEdit:   true   (AllowEdits not listed — Access default = true)
AllowDelete: true   (AllowDeletions not listed — Access default = true)
Editable fields:   TestCode, Charge
Read-only fields:  NoTests (WorkGroupGrade — Enabled=NotDefault AND Locked=NotDefault),
                   TestPrice (ChargeRate — Enabled=NotDefault AND Locked=NotDefault)
Computed fields:   none in ControlSource formula format
```

| Field | Editable? | Reason |
|---|---|---|
| `TestCode` | Yes | Neither `Enabled` nor `Locked` set |
| `NoTests` | No (read-only) | Both `Enabled = NotDefault` and `Locked = NotDefault` |
| `TestPrice` | No (read-only) | Both `Enabled = NotDefault` and `Locked = NotDefault` |
| `Charge` | Yes | Neither `Enabled` nor `Locked` set |

> **Note:** The planned tests grid reuses `TestPlanJobController.LoadTestPlanGrid` — `BindGridUrl = /FPS/TestPlanJob/LoadTestPlanGrid`.
> `TestPlanItem` already covers TestCode, NoRequired, UnitPrice, TestCost. Add/Edit/Delete delegate to existing `TestPlanJobController` actions.

---

## fsubCompareTests2 — "Actual Tests (PACT)"

**RecordSource:** `qryCompareTests2` → `SELECT MonthlyOutput.Buyer, TestCode, Month, Volume, tlkpTestReqmt.UnitPrice AS TestPrice, [unitprice]*[volume] AS Charge, WorkGroup FROM MonthlyOutput INNER JOIN tlkpTestReqmt ON ...`
**Link:** `LinkChildFields=Buyer`, `LinkMasterFields=parentproject`

```
Grid Operations Profile — CompareTests2
AllowAdd:    false  (HTML prototype: Add button is commented out)
AllowEdit:   false  (HTML prototype: no Actions column in Actual Tests table)
AllowDelete: true   (following ProjectStaffPlanActual pattern for PACT actuals)
Editable fields:   TestCode, Month, WorkGroup
Read-only fields:  Volume (WorkGroupGrade — Enabled=NotDefault AND Locked=NotDefault),
                   TestPrice (ChargeRate — Enabled=NotDefault AND Locked=NotDefault)
Computed fields:   Charge (=[unitprice]*[volume] computed in SQL query)
```

| Field | Editable? | Reason |
|---|---|---|
| `TestCode` | Yes | Neither `Enabled` nor `Locked` set |
| `Volume` | No (read-only) | Both `Enabled = NotDefault` and `Locked = NotDefault` |
| `TestPrice` | No (read-only) | Both `Enabled = NotDefault` and `Locked = NotDefault` |
| `Charge` | No (computed) | `[unitprice]*[volume]` computed in `qryCompareTests2` SQL |
| `Month` | Yes | Neither `Enabled` nor `Locked` set |
| `WorkGroup` | Yes | Neither `Enabled` nor `Locked` set |
