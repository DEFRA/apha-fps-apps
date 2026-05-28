Option Compare Database   'Use database order for string comparisons
'Global ODBC_Con_Str   As String

Function ldoCreateMonths()

    On Error GoTo ldoCreateMonths_Err

    Dim MyDB As Database
    Dim rst As Recordset
    Dim rstProjectMonth As Recordset
    Dim i As Integer

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("qptMissingProjects", DB_OPEN_SNAPSHOT)
    Set rstProjectMonth = MyDB.OpenRecordset("ProjectMonth", DB_OPEN_DYNASET)
    
    If rst.BOF And rst.EOF Then GoTo Tidy_up
    BeginTrans

    rst.MoveFirst
    Do Until rst.EOF
        For i = 1 To 12
            rstProjectMonth.AddNew
            rstProjectMonth!project = rst!ParentProject
            rstProjectMonth!MonthNo = i
            rstProjectMonth!CostProfile = 0
            rstProjectMonth.Update
        Next i
        rst.MoveNext
    Loop

    CommitTrans

Tidy_up:

    rst.Close
    rstProjectMonth.Close

Exit Function
ldoCreateMonths_Err:

    MsgBox Err & " " & Error
    Rollback

End Function

Sub ldoRecreateSummaries()

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim strSQL As String
    
    'If fnAD6000_Massage(Forms![frmRecreateSummaries]![Period]) = False Then
    '    Exit Sub
    'End If

    Set MyDB = CurrentDb()
    Set MyQuery = MyDB.CreateQueryDef("qptRecreateSummaries")

    strSQL = "sp_RecreateSummaries " & Forms![frmRecreateSummaries]![Period]

    MyQuery.Connect = ConnectStr()
    MyQuery.ReturnsRecords = False
    MyQuery.ODBCTimeout = 0
    MyQuery.sql = strSQL

    DoCmd.SetWarnings False
    DoCmd.Hourglass True
    DoCmd.OpenQuery "qptRecreateSummaries"
    DoCmd.Hourglass False
    DoCmd.SetWarnings True
    MyDB.QueryDefs.Delete "qptrecreateSummaries"

    DoCmd.OpenReport "rptxtbTestProjectMonthFinal", A_PREVIEW

    DoCmd.Close A_FORM, "frmRecreateSummaries"

End Sub