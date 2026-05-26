Option Compare Database
Option Explicit

Function fnMaxMonth() As Integer


Dim db As Database
Dim rs As Recordset

Set db = CurrentDb
Set rs = db.OpenRecordset("SELECT Max(tblPeriod.EndPeriod) AS MaxOfEndPeriod FROM tblPeriod WHERE (((tblPeriod.FinalSummariesRun)=True));")
With rs
    .MoveFirst
    fnMaxMonth = IIf(IsNull(.Fields(0)), 0, .Fields(0))
End With
End Function

Function fnSnapshotMonth() As Integer

Dim db As Database
Dim rs As Recordset
Set db = CurrentDb
Set rs = db.OpenRecordset("Select DB_Var_Value from Snapshot_tblDB_Variables WHERE DB_Var_Name = 'Month' ")
With rs
    If .BOF And .EOF Then
        fnSnapshotMonth = 0
    Else
        .MoveFirst
        fnSnapshotMonth = IIf(IsNull(.Fields(0)), 0, .Fields(0))
    End If
    
End With
End Function

Function fnSnapshotDBName() As String

    'Chris Moore - Jan 2025
    'Get the FPS_Snapshot Database name from the local Settings table as still issue
    ' with SQL001 despite rollback of Compatibility Level.
    'No longer retrieved from SQL Server.
   
    Dim db As Database
    Dim rs As Recordset

    Set db = CurrentDb
    'Set rs = db.OpenRecordset("Select DB_Var_Value from Snapshot_tblDB_Variables WHERE DB_Var_Name = 'DB_Name' ")
    Set rs = db.OpenRecordset("SELECT Setting FROM [tbl Settings] WHERE ID = 'SnapshotDatabase' ", dbOpenDynaset)

    With rs
        If .BOF And .EOF Then
            fnSnapshotDBName = 0
        Else
            .MoveFirst
            'fnSnapshotDBName = .Fields(0)
            fnSnapshotDBName = .Fields("Setting")
        End If
    
    End With
    
End Function
Function fnDBName() As String

    'Chris Moore - Jan 2025
    'Get the FPS Database name from the local Settings table as still issue
    ' with SQL001 despite rollback of Compatibility Level.
    'No longer retrieved from SQL Server.
    Dim db As Database
    Dim rs As Recordset

    Set db = CurrentDb
    'Set rs = db.OpenRecordset("qryDBName")
    Set rs = db.OpenRecordset("SELECT Setting FROM [tbl Settings] WHERE ID = 'SQLDatabase' ", dbOpenDynaset)

    With rs
        If .BOF And .EOF Then
            fnDBName = 0
        Else
            .MoveFirst
            'fnDBName = .Fields(0)
            fnDBName = .Fields("Setting")
        End If
    
    End With
    
End Function
Sub test()
Dim sqlstr As String
Dim db_name As String
db_name = Trim(DLookup("db_name", "qryDBName"))
sqlstr = "DELETE FROM tblFPSYearsToImport  WHERE tblFPSYearsToImport.FPSName ='" & db_name & "'"
DoCmd.RunSQL (sqlstr)

DoCmd.RunSQL ("INSERT INTO tblFPSYearsToImport (FPSName) SELECT '" & db_name & "'")

End Sub

Sub RunSnapshot()
'Used to put values in the tblDB_Variables in the Snapshot database and the FPS
'   database that the nightly SQL job uses to determine if the Take Snapshot job should run.
'   i.e. copy the current FPS into FPS_Snapshot.

Dim sqlstr As String
Dim db_name As String
db_name = Trim(DLookup("db_name", "qryDBName"))
'sqlstr = "DELETE FROM tblFPSYearsToImport  WHERE tblFPSYearsToImport.FPSName ='" & db_name & "'"

DoCmd.SetWarnings False
DoCmd.RunSQL ("DELETE  FROM Snapshot_tblDB_Variables")
DoCmd.RunSQL ("INSERT INTO Snapshot_tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Snapshot_Ready' AS Expr1, 'True' AS Expr2")
DoCmd.RunSQL ("INSERT INTO Snapshot_tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'DB_Name' AS Expr3, qryDBName.db_name FROM qryDBName")
DoCmd.RunSQL ("INSERT INTO Snapshot_tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Month'  AS Expr1, '" & fnMaxMonth() & "'")
'DoCmd.RunSQL (sqlstr)

DoCmd.RunSQL ("DELETE  FROM tblDB_Variables")
DoCmd.RunSQL ("INSERT INTO tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Snapshot_Ready' AS Expr1, 'False' AS Expr2")
DoCmd.RunSQL ("INSERT INTO tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'DB_Name' AS Expr3, qryDBName.db_name FROM qryDBName")
DoCmd.RunSQL ("INSERT INTO tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Month'  AS Expr1, '" & fnMaxMonth() & "'")
'DoCmd.RunSQL ("INSERT INTO tblFPSYearsToImport (FPSName) SELECT '" & db_name & "'")

'DoCmd.OpenQuery ("qptRunPMReports")
DoCmd.SetWarnings True

End Sub

Sub DontRunSnapshot()
'Reverses the RunSnapshot() procedure as detailed above, IF run on the same day.
'Will NOT reverse the transfer of the Snapshot and the loading of the MAB Warehouse
'   if run later.

Dim sqlstr As String
Dim db_name As String
db_name = Trim(DLookup("db_name", "qryDBName"))
'sqlstr = "DELETE FROM tblFPSYearsToImport  WHERE tblFPSYearsToImport.FPSName ='" & db_name & "'"

DoCmd.SetWarnings False
DoCmd.RunSQL ("DELETE FROM Snapshot_tblDB_Variables")
DoCmd.RunSQL ("INSERT INTO Snapshot_tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Snapshot_Ready' AS Expr1, 'False' AS Expr2")
DoCmd.RunSQL ("INSERT INTO Snapshot_tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'DB_Name' AS Expr3, qryDBName.db_name FROM qryDBName")
DoCmd.RunSQL ("INSERT INTO Snapshot_tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Month'  AS Expr1, '" & fnMaxMonth() & "'")

DoCmd.RunSQL ("DELETE FROM tblDB_Variables")
DoCmd.RunSQL ("INSERT INTO tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Snapshot_Ready' AS Expr1, 'False' AS Expr2")
DoCmd.RunSQL ("INSERT INTO tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'DB_Name' AS Expr3, qryDBName.db_name FROM qryDBName")
DoCmd.RunSQL ("INSERT INTO tblDB_Variables ( DB_Var_Name, DB_Var_Value ) SELECT 'Month'  AS Expr1, '" & fnMaxMonth() & "'")

'DoCmd.RunSQL (sqlstr)

DoCmd.SetWarnings True
End Sub



Function fnReleaseSummaries()
    
    Dim dbName As String
    Dim SnapshotDBName As String
    Dim DBMonth As Integer
    Dim SnapshotDBMonth As Integer
    
   
    dbName = fnDBName()
    SnapshotDBName = fnSnapshotDBName()
    DBMonth = fnMaxMonth()
    SnapshotDBMonth = fnSnapshotMonth()
    
    
        If DBMonth = 0 Then
            DontRunSnapshot
            Exit Function
        End If
        If dbName = SnapshotDBName Then
            If DBMonth >= SnapshotDBMonth Then
                RunSnapshot
            Else
                DontRunSnapshot
            End If
    
        ElseIf dbName > SnapshotDBName Then
            RunSnapshot
        ElseIf SnapshotDBMonth = 0 Then
            RunSnapshot
    
        End If
End Function