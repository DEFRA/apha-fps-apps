Option Compare Database
Option Explicit

Global AD6000_ID As Integer
Global AD6000_WG As String
Global gVarFMonth As Integer
Global gVarFYear As Integer

Function GetgVarFMonth()
    GetgVarFMonth = gVarFMonth
End Function

Function GetgVarFYear()
    GetgVarFYear = gVarFYear
End Function

Function fnAD6000_Massage(fMonth As Integer) As Boolean
'takes all time put down to jobcode AD6000 and redistributes it to other projects,
'firstly as a set amount of hours as per a list in tblMTConversion, then any extra hours are
'spread over the main ED contracts, as per the number of submissions in the previous month.
'(From the FF Archive).

Dim ws As Workspace, db As Database
Dim sysErr, sysErrMsg
Dim strSQL As String
Dim fYear As Integer

On Error GoTo fnAD6000_Massage_err

If DSum("IsInvalid", "qryAD6000_CodeCheck") <> 0 Then
    MsgBox "There are invalid codes on the list of new codes for the AD6000 time, please check and correct.", , "AD6000 Time"
    fnAD6000_Massage = False
    Exit Function
End If


Set ws = DBEngine(0)
Set db = ws(0)

gVarFYear = CInt(Right(DLookup("DB_Var_Value", "tblDB_Variables", "DB_Var_Name='DB_Name'"), 4))
gVarFMonth = fMonth

DoCmd.Hourglass True
ws.BeginTrans
'clear temp tables
db.Execute "qryAD6000_DeleteTempTable", dbSeeChanges + dbFailOnError
db.Execute "qryAD6000_DeleteTempTable2", dbSeeChanges + dbFailOnError

'add hours put down to AD6000
db.Execute "qryAD6000_GetAndTransform", dbSeeChanges + dbFailOnError

'add hours already converted to new projects
db.Execute "qryAD6000_GetExisting", dbSeeChanges + dbFailOnError

'add flat rate hours to temp table 2
db.Execute "qryAD6000_FlatRateHours", dbSeeChanges + dbFailOnError

'add extra hours to temp table 2
db.Execute "qryAD6000_ExtraHours", dbSeeChanges + dbFailOnError

'Delete Existing Hours
db.Execute "qryAD6000_Delete1", dbSeeChanges + dbFailOnError
db.Execute "qryAD6000_Delete2", dbSeeChanges + dbFailOnError

'Add New Hours
db.Execute "qryAD6000_Save", dbSeeChanges + dbFailOnError
ws.CommitTrans
DoCmd.Hourglass False
fnAD6000_Massage = True

fnAD6000_Massage_ok:
    Exit Function
    
fnAD6000_Massage_err:
    MsgBox "Warning!  Cannot complete the massaging of data.  Please check all timecodes are valid.  Undoing all changes..."
    ws.Rollback
    DoCmd.Hourglass False
    'runActionQuery = False
    fnAD6000_Massage = False
    

    Resume fnAD6000_Massage_ok
End Function

Function fnIG()
Dim ws As Workspace, db As Database
Dim sysErr, sysErrMsg
Dim strSQL As String
On Error GoTo fnIG_Massage_err

Set ws = DBEngine(0)
Set db = ws(0)

'ws.BeginTrans
db.Execute "qryDelTable1", dbSeeChanges + dbFailOnError
MsgBox "OK"
Exit Function
fnIG_Massage_err:
MsgBox "NOT ok"
Exit Function

End Function

Sub setG()
 gVarFYear = CInt(Right(DLookup("DB_Var_Value", "tblDB_Variables", "DB_Var_Name='DB_Name'"), 4))
 gVarFMonth = 3
End Sub