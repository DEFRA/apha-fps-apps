Option Compare Database   'Use database order for string comparisons
Global RCvalue As String
Global WGValue As String

Sub ldoImportExcel()

    Dim strTemp As String

    'On Error GoTo 0
    
    'Purpose: Import a specified spreadsheet


    'Open form for user to stipulate sheet to import, work group and period it is for
    '  and cell range.
      
     strTemp = ldoImportRoutine()

     DoCmd.OpenForm "frmImportDialog"

    'View any import errors.
        
        'ldoViewImportErrors

    'View tmpTable Discard unwanted columns

    

    'Flatten out crosstab


    'Delete "0 hour" records


    'Validate names and job codes


    'Import validated tmptable into MonthlyTime replacing names with PACTids

    
    'Delete tmptable with xtab in


    'Deal with any rejected records from import


Exit Sub
ldoImportExcel_Err:

    Select Case Err
       Case 2501
               'DoCmd Close A_FORM, "frmImportDialog"
               Exit Sub
       Case Else
            Resume Next
    End Select

    

End Sub

 Function ldoImportRoutine() As String
        
On Error GoTo ldoImportRoutine_Err


        DoCmd.DoMenuItem 1, 0, 5
        ldoImportRoutine = "True"

ldoImportRoutine_Err:

    ldoImportRoutine = "False"
    Exit Function

End Function

Sub ldoMakeOutputSheet(strWorkGroup As String, strMonth As String, strRecipient As String)
    
    Forms!frmProcessing!Records = "Creating output spreadsheet for " & strWorkGroup
    DoCmd.RepaintObject A_FORM, "frmProcessing"

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim strFileName As String
    Dim strSQL As String

    strSQL = strSQL & "SELECT DISTINCTROW tlkpTestCapability.WorkGroup, tlkpTestCapability.TestCode, TestOrProduct.ItemDescription, tlkpTestReqmt.Buyer, " & Chr(34) & strMonth & Chr(34) & " AS Month, Null AS Volume "
    strSQL = strSQL & "FROM TestOrProduct INNER JOIN (tlkpTestReqmt INNER JOIN tlkpTestCapability ON tlkpTestReqmt.TestCode = tlkpTestCapability.TestCode) ON TestOrProduct.ItemCode = tlkpTestCapability.TestCode "
    strSQL = strSQL & "WHERE tlkpTestReqmt.Active <> 0 AND ((tlkpTestCapability.WorkGroup=" & Chr(34) & strWorkGroup & Chr(34) & ")) "
    strSQL = strSQL & "ORDER BY tlkpTestCapability.WorkGroup, tlkpTestCapability.TestCode, tlkpTestReqmt.Buyer;"

    
    'Debug.Print strSQL

    If Len(strMonth) = 1 Then strMonth = "0" & strMonth
    strFileName = strWorkGroup & strMonth & "OP"

    Set MyDB = CurrentDb()
    Set MyQuery = MyDB.CreateQueryDef(strFileName)
    MyQuery.ODBCTimeout = 0
    MyQuery.sql = strSQL

    Forms!frmProcessing!Records = "Sending output spreadsheet for " & strWorkGroup & " to " & strRecipient
    DoCmd.RepaintObject A_FORM, "frmProcessing"
    DoCmd.OutputTo acOutputQuery, strFileName, A_FORMATXLS, CurrentProject.path & "\" & strFileName & ".xls", False

    SendMessage strRecipient, _
                  "MARS Output Sheets" & " - " & strFileName & ".xls", _
                  "Please complete and return to APHA Gatekeeper - OTL Mailbox. [Mailto:CAPSMailbox@vla.defra.gsi.gov.uk]. Thank you.", _
                  CurrentProject.path & "\" & strFileName & ".xls", _
                  ""

    MyDB.QueryDefs.Delete strFileName
    Kill CurrentProject.path & "\" & strFileName & ".xls"
End Sub

Sub ldoMakeTimeSheet(strWorkGroup As String, strMonth As String, strRecipient As String, intLayout As Integer)

    On Error GoTo ldoMakeTimeSheet_Err

    Forms!frmProcessing!Records = "Creating time spreadsheet for " & strWorkGroup
    DoCmd.RepaintObject A_FORM, "frmProcessing"

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim strFileName As String
    Dim strSQL As String

    If intLayout = 1 Then
        'FLAT FILE
        strSQL = strSQL & "SELECT DISTINCTROW TimeCodeValid.WorkGroup, tblStaff.Name, TimeCodeValid.TimeCode, TimeCodeValid.ParentProject, " & Chr(34) & strMonth & Chr(34) & " AS Month, Null AS Hours "
        strSQL = strSQL & "FROM (TimeCodeValid INNER JOIN WorkGroupGrade ON TimeCodeValid.WorkGroup = WorkGroupGrade.WorkGroup) INNER JOIN tblStaff ON WorkGroupGrade.[WG_Grade] = tblStaff.WorkGroupGrade "
        strSQL = strSQL & "WHERE ((TimeCodeValid.WorkGroup=" & Chr(34) & strWorkGroup & Chr(34) & ") AND (TimeCodeValid.Active<>0)) "
        strSQL = strSQL & "ORDER BY TimeCodeValid.WorkGroup, tblStaff.Name, TimeCodeValid.TimeCode, TimeCodeValid.ParentProject;"
    ElseIf intLayout = 2 Then
        'XTAB SQL
        strSQL = "TRANSFORM Null AS Hours "
        strSQL = strSQL & "SELECT TimeCodeValid.TimeCode, First(IIf(IsNull([TimeCodeValid].[JobCode]),[ItemDescription],[JobCodeName])) AS Description, TimeCodeValid.ParentProject "
        strSQL = strSQL & "FROM (tlkpJobCode RIGHT JOIN ((TimeCodeValid INNER JOIN WorkGroupGrade ON TimeCodeValid.WorkGroup = WorkGroupGrade.WorkGroup) INNER JOIN tblStaff ON WorkGroupGrade.WG_Grade = tblStaff.WorkGroupGrade) ON tlkpJobCode.JobCode = TimeCodeValid.JobCode) LEFT JOIN TestOrProduct ON TimeCodeValid.TestCode = TestOrProduct.ItemCode "
        strSQL = strSQL & "WHERE ((TimeCodeValid.WorkGroup=" & Chr(34) & strWorkGroup & Chr(34) & ") AND (TimeCodeValid.Active<>0) AND tblStaff.PersonStatus <> 'I') "
        strSQL = strSQL & "GROUP BY TimeCodeValid.WorkGroup, TimeCodeValid.TimeCode,  TimeCodeValid.ParentProject "
        strSQL = strSQL & "ORDER BY TimeCodeValid.TimeCode, TimeCodeValid.ParentProject, TimeCodeValid.WorkGroup, tblStaff.Name "
        strSQL = strSQL & "PIVOT tblStaff.Name;"
    
    End If
    'Debug.Print strSQL
    If Len(strMonth) = 1 Then strMonth = "0" & strMonth
    strFileName = strWorkGroup & strMonth & "TS"

    Set MyDB = CurrentDb()
    Set MyQuery = MyDB.CreateQueryDef(strFileName)
    MyQuery.ODBCTimeout = 0
    If IsNull(strSQL) = False Then MyQuery.sql = strSQL

    Forms!frmProcessing!Records = "Sending time spreadsheet for " & strWorkGroup & " to " & strRecipient
    DoCmd.RepaintObject A_FORM, "frmProcessing"
    DoCmd.OutputTo acOutputQuery, strFileName, A_FORMATXLS, CurrentProject.path & "\" & strFileName & ".xls", False

    SendMessage strRecipient, _
                  "MARS Time Sheets" & " - " & strFileName & ".xls", _
                  "Please complete and return to APHA Gatekeeper - OTL Mailbox. [Mailto:CAPSMailbox@vla.defra.gsi.gov.uk]. Thank you.", _
                  CurrentProject.path & "\" & strFileName & ".xls", ""

    MyDB.QueryDefs.Delete strFileName
    Kill CurrentProject.path & "\" & strFileName & ".xls"
Exit Sub
ldoMakeTimeSheet_Err:

Select Case Err
    
    Case 3012
        MyDB.QueryDefs.Delete strFileName
        Resume
    Case 3146
        DoEvents
        Resume
    Case Else
        MsgBox Err & " " & Error
        Exit Sub
    End Select
    

End Sub

Sub ldoSheetsForWorkGroups(strMonth As String)

    Dim MyDB As Database
    Dim rst As Recordset
    Dim rstPC As Recordset
    Dim strWG As String
    Dim strPCSQL As String
    Dim strPCRecipient As String
    Dim intLayout As Integer

    Set MyDB = CurrentDb()

    strPCSQL = "SELECT DISTINCTROW WorkGroup.WorkGroup, tblkpProfitCentre.ProfitCentre, tblkpProfitCentre.PACTCoordinatorEmailName, tblkpProfitCentre.TimeSheet, tblkpProfitCentre.OutputSheet, tblkpProfitCentre.TimeSheetLayout "
    strPCSQL = strPCSQL & "FROM tblkpProfitCentre INNER JOIN WorkGroup ON tblkpProfitCentre.ProfitCentre = WorkGroup.ProfitCentre "
    strPCSQL = strPCSQL & "ORDER BY tblkpProfitCentre.ProfitCentre;"
    
    Set rst = MyDB.OpenRecordset("SELECT DISTINCTROW WorkGroup.* FROM WorkGroup WHERE ((WorkGroup.SendEmail=Yes)) ORDER BY WorkGroup.WorkGroup;", DB_OPEN_SNAPSHOT)
    Set rstPC = MyDB.OpenRecordset(strPCSQL, DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        rstPC.FindFirst "WorkGroup =" & Chr(34) & rst!workgroup & Chr(34)
        If IsNull(rst!Email_Recipient) Then
            MsgBox "There is no email recipient stated for " & rst!workgroup & Chr(39) & ". So email could not be sent. "
            GoTo Next_Record
        End If
        strPCRecipient = rst!Email_Recipient
        strWG = rst!workgroup
        If rstPC!Timesheet = True Then intLayout = rstPC!TimesheetLayout
        If rstPC!Timesheet = True Then ldoMakeTimeSheet strWG, strMonth, strPCRecipient, intLayout
        If rstPC!OutputSheet = True Then ldoMakeOutputSheet strWG, strMonth, strPCRecipient
        DoEvents
Next_Record:
        rst.MoveNext
    Loop

    rst.Close
    rstPC.Close

    DoCmd.Close A_FORM, "frmProcessing"

End Sub

Sub ldoTimeSheet(strWorkGroup As String)

    Dim strSQL As String

    strSQL = "TRANSFORM Sum(MonthlyTime.Hours) AS SumOfHours "
    strSQL = strSQL & "SELECT MonthlyTime.TimeCode, IIf(IsNull([JobCodeName]),[ItemDescription],[JobCodeName]) AS Description " & Chr(13)
    strSQL = strSQL & "FROM WorkGroup INNER JOIN (WorkGroupGrade INNER JOIN (tblStaff INNER JOIN (tlkpProject RIGHT JOIN (TestOrProduct RIGHT JOIN (tlkpJobCode RIGHT JOIN MonthlyTime ON tlkpJobCode.JobCode = MonthlyTime.TimeCode) ON TestOrProduct.ItemCode = MonthlyTime.TimeCode) ON tlkpProject.ParentProject = MonthlyTime.TimeCode) ON tblStaff.PACTid = MonthlyTime.PACTStaffID) ON WorkGroupGrade.[WG_Grade] = tblStaff.WorkGroupGrade) ON WorkGroup.WorkGroup = WorkGroupGrade.WorkGroup " & Chr(13)
    strSQL = strSQL & "WHERE ((WorkGroup.WorkGroup=" & Chr(34) & strWorkGroup & Chr(34) & ")) " & Chr(13)
    strSQL = strSQL & "GROUP BY MonthlyTime.TimeCode, tlkpJobCode.JobCodeName, TestOrProduct.ItemDescription, WorkGroup.WorkGroup " & Chr(13)
    strSQL = strSQL & "PIVOT tblStaff.Name;" & Chr(13)

    Debug.Print strSQL



End Sub

Function ReturnRCvalue()
ReturnRCvalue = RCvalue
End Function

Function ReturnWGvalue()
ReturnWGvalue = WGValue
End Function


Sub ig()
 'DoCmd.SendObject A_QUERY, strFileName, A_FORMATXLS, strRecipient, , , "MARS Output Sheets" & " - " & strFileName & ".xls", "Please complete and return to CAPS Time & Test Processing Mailbox. [Mailto:CAPSMailbox@vla.defra.gsi.gov.uk]. Thank you.", 0

DoCmd.SendObject A_QUERY, "query1", A_FORMATXLS, "ian.galloway@ahvla.gsi.gov.uk", , , "MARS Output Sheets", "Please complete and return to CAPS Time & Test Processing Mailbox. [Mailto:CAPSMailbox@vla.defra.gsi.gov.uk]. Thank you.", 0

End Sub