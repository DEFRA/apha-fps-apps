MODULE NAME: mdlReports
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function fnInReportMonthOrBefore(dt As Variant) As Boolean

Dim StartDate As Date
Dim EndDate As Date

If IsNull(dt) Then
    fnInReportMonthOrBefore = False
Else
    StartDate = fnReportStartOfYear()
    EndDate = fnReportEndOfMonth()
    
    If (dt >= StartDate) And (dt <= EndDate) Then
        fnInReportMonthOrBefore = True
    Else
        fnInReportMonthOrBefore = False
    End If

End If


End Function
Function fnInReportMonth(dt As Variant) As Boolean

Dim StartDate As Date
Dim EndDate As Date

If IsNull(dt) Then
    fnInReportMonth = False
Else
    StartDate = fnReportStartOfMonth()
    EndDate = fnReportEndOfMonth()
    
    If (dt >= StartDate) And (dt <= EndDate) Then
        fnInReportMonth = True
    Else
        fnInReportMonth = False
    End If

End If


End Function

Function fnReportStartOfMonth()
    fnReportStartOfMonth = CDate("01/" & Forms![frmMenuReports]![cbMonth] & "/" & Forms![frmMenuReports]![txtYear])
End Function
Function fnReportEndOfMonth() As Date
    fnReportEndOfMonth = DateAdd("d", -1, DateAdd("m", 1, fnReportStartOfMonth()))
End Function
Function fnReportStartOfYear() As Date
    fnReportStartOfYear = CDate("01/04/" & fnReportYear())
End Function
Function fnReportEndOfYear() As Date
    fnReportEndOfYear = CDate("31/03/" & fnReportYear() + 1)
End Function

Function fnInFinancialYear(dt) As Boolean
Dim StartDate As Date
Dim EndDate As Date

If IsNull(dt) Then
    fnInFinancialYear = False
Else
    StartDate = fnReportStartOfYear()
    EndDate = fnReportEndOfYear()
    dt = CDate(Format(CDate(dt), "dd/mm/yy"))
    If dt >= StartDate And dt <= EndDate Then
        fnInFinancialYear = True
    Else
        fnInFinancialYear = False
    End If
End If

End Function

Function fnInFinancialYearOrBefore(dt) As Boolean
Dim StartDate As Date
Dim EndDate As Date

If IsNull(dt) Then
    fnInFinancialYearOrBefore = False
Else
    'StartDate = fnReportStartOfYear()
    EndDate = fnReportEndOfYear()
    dt = CDate(Format(CDate(dt), "dd/mm/yy"))
    If dt <= EndDate Then
        fnInFinancialYearOrBefore = True
    Else
        fnInFinancialYearOrBefore = False
    End If
End If

End Function

Function fnReportYear()
'For reports using Financial years
If Forms![frmMenuReports]![cbMonth] >= 4 Then
    fnReportYear = Forms![frmMenuReports]![txtYear]
Else
    fnReportYear = Forms![frmMenuReports]![txtYear] - 1
End If

End Function

'Function fnReportFinancialYear()
'For the Deliverable Breakdown Report, where you can run it from different
'financial months/year for different finacial years.
' '   fnReportFinancialYear = Forms![frmMenuReports]![PickFYear]
'End Function


Function fnYear()
'For reports using calandar years only, Or Financial years where the month is not considered.

    fnYear = Forms![frmMenuReports]![txtYear]


End Function
'Function fnYearChosen()
'For reports Not using financial years
'fnYearChosen = Forms![frmMenuReports]![txtYear]
'End Function

Function fnDueYear(dt)
If IsNull(dt) Then
    fnDueYear = Null
Else
        dt = CDate(Format(CDate(dt), "dd/mm/yy"))
    If DatePart("m", dt) >= 4 Then
        fnDueYear = DatePart("yyyy", dt)
    Else
        fnDueYear = DatePart("yyyy", dt) - 1
    End If
End If
End Function

Sub Mailreports(rptname As String)
On Error GoTo Mailrep_err

Dim db As Database
Dim qd As QueryDef
Dim rsReportData As Recordset
Dim rsManagers As Recordset
Dim strSQLWhere As String
Dim strSQL As String
Dim rsName As String
Dim stEmailTitle As String
Dim stEmailText As String
Dim stManager As String
Dim rep As Report
Dim stEmailAddress As String
'Dim repRs
Set db = CurrentDb

Forms![frmMenuReports]![HiddenManager] = "*"
Application.Echo False
DoCmd.OpenReport rptname, acViewDesign
rsName = Reports(rptname).RecordSource
'rsName = qd.Name
DoCmd.Close acReport, rptname
Application.Echo True

strSQL = "SELECT * FROM tblReport WHERE reportName='" & rptname & "'"
Debug.Print strSQL
Set rsReportData = db.OpenRecordset(strSQL, dbOpenSnapshot, dbSeeChanges)
With rsReportData
    stEmailTitle = Nz(.Fields("MailTitle"))
    stEmailText = Nz(.Fields("MailComment"))
    'rsName = .Fields("queryname")
End With

Set rsReportData = Nothing
strSQLWhere = fnGetSQLWhere()
'Dim qdf As QueryDef
'Dim param As Parameter
'Set qdf = db.QueryDefs(rsName)
'For Each param In qdf.Parameters
 '   param.Value = Eval(param.Name)
'Next param

strSQL = "SELECT manager FROM " & rsName & IIf(Nz(strSQLWhere) = "", "", " WHERE " & strSQLWhere) & " GROUP BY manager;"
Debug.Print strSQL
Set rsManagers = db.OpenRecordset(strSQL)

With rsManagers
    If .EOF And .BOF Then
        MsgBox "No Data for this selection."

    Else
        .MoveFirst
        While Not .EOF
            stManager = Nz(.Fields("Manager"))
            'strSQL = "SELECT * FROM " & rsName & " WHERE Manager ='" & stManager & "' AND " & strSQLWhere
            Forms![frmMenuReports]![HiddenManager] = stManager
            'Debug.Print strSQL
            'Debug.Print "'" & .Fields("Manager") & "'"
            stEmailAddress = Nz(DLookup("[EMail]", "tblProjectManager", "[ProjectManager]='" & stManager & "'"))
            'db.QueryDefs(rsName & "_WHERE").SQL = strSQL
            DoCmd.SendObject acReport, rptname, "Snapshot Format", stEmailAddress, , , stEmailTitle & ": " & stManager, stEmailText, 0
            .MoveNext
        Wend
    End If
    
End With
Exit Sub
Mailrep_err:

    Application.Echo True
    MsgBox Err.Number & " " & Err.Description
End Sub

Sub ResetReportQuery(rptname As String)
Dim db As Database
Dim qd As QueryDef
Dim rsReportData As Recordset
Dim rsManagers As Recordset
Dim strSQLWhere As String
Dim strSQL As String
Dim rsName As String
Dim stEmailTitle As String
Dim stEmailText As String
Dim stManager As String
Dim rep As Report
Dim stEmailAddress As String

Set db = CurrentDb
strSQL = "SELECT * FROM tblReport WHERE reportName='" & rptname & "'"
Debug.Print strSQL
Set rsReportData = db.OpenRecordset(strSQL)
With rsReportData

    rsName = .Fields("queryname")
End With
strSQLWhere = Nz(fnGetSQLWhere())
strSQL = "SELECT * FROM " & rsName & IIf(strSQLWhere = "", "", " WHERE ") & strSQLWhere
db.QueryDefs(rsName & "_WHERE").sql = strSQL
End Sub

Function fnGetSQLWhere() As String
'Report "Filter" also set by looking in the tblReport.filter column
Dim sqlw As String
Dim ctrl As Control
sqlw = ""
If Forms![frmMenuReports]![Frame17] = 1 Then
    fnGetSQLWhere = ""
Else
    Set ctrl = Forms![frmMenuReports]![pickProgramme]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        sqlw = " Program = '" & ctrl & "'"
    End If
    Set ctrl = Forms![frmMenuReports]![pickProject]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        If sqlw <> "" Then sqlw = sqlw & " AND "
        sqlw = sqlw & " Project = '" & ctrl & "'"
    End If
    Set ctrl = Forms![frmMenuReports]![pickManager]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        If sqlw <> "" Then sqlw = sqlw & " AND "
        sqlw = sqlw & " Manager = '" & ctrl & "'"
    End If
    Set ctrl = Forms![frmMenuReports]![pickContract]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        If sqlw <> "" Then sqlw = sqlw & " AND "
        sqlw = sqlw & " Contract = '" & ctrl & "'"
    End If
    Set ctrl = Forms![frmMenuReports]![pickCustomer]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        If sqlw <> "" Then sqlw = sqlw & " AND "
        sqlw = sqlw & " Customer = '" & ctrl & "'"
    End If
    fnGetSQLWhere = sqlw
End If

End Function

Function fnGetSQLSummaryWhere() As String
'Report "Filter" also set by looking in the tblReport.filter column
'Used for summary queries that dont have Customer and Contract in the query
Dim sqlw As String
Dim ctrl As Control
sqlw = ""
If Forms![frmMenuReports]![Frame17] = 1 Then
    fnGetSQLSummaryWhere = ""
Else
    Set ctrl = Forms![frmMenuReports]![pickProgramme]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        sqlw = " Program = '" & ctrl & "'"
    End If
    Set ctrl = Forms![frmMenuReports]![pickProject]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        If sqlw <> "" Then sqlw = sqlw & " AND "
        sqlw = sqlw & " Project = '" & ctrl & "'"
    End If
    Set ctrl = Forms![frmMenuReports]![pickManager]
    If Not IsNull(ctrl) And ctrl.Enabled Then
        If sqlw <> "" Then sqlw = sqlw & " AND "
        sqlw = sqlw & " Manager = '" & ctrl & "'"
    End If


    fnGetSQLSummaryWhere = sqlw
End If

End Function

Function fnSummaryCustomer() As String

If Forms![frmMenuReports]![Frame17] = 1 Then
    fnSummaryCustomer = "*"
ElseIf Nz(Forms![frmMenuReports]![pickCustomer], "") = "" Then
    fnSummaryCustomer = "*"
Else
    fnSummaryCustomer = Forms![frmMenuReports]![pickCustomer]
End If

End Function

Function fnSummaryContract() As String

If Forms![frmMenuReports]![Frame17] = 1 Then
    fnSummaryContract = "*"
ElseIf Nz(Forms![frmMenuReports]![pickContract], "") = "" Then
    fnSummaryContract = "*"
Else
    fnSummaryContract = Forms![frmMenuReports]![pickContract]
End If
End Function

Function fnGetSQLWhereDesc()
    fnGetSQLWhereDesc = IIf(fnGetSQLWhere() = "", "", "Where " & fnGetSQLWhere())
End Function
Function fnDecodeYesNo(Val As Variant) As String
If IsNull(Val) Then
        fnDecodeYesNo = "No"
Else
    Select Case Val

        Case 0
            fnDecodeYesNo = "No"
        Case -1, 1
            fnDecodeYesNo = "Yes"
        Case Else
            fnDecodeYesNo = ""
    End Select
End If
    
End Function

Function fnGetReportDisplayDate()
Dim dtstr As String
dtstr = Forms![frmMenuReports]![cbMonth] & "/" & Forms![frmMenuReports]![txtYear]
fnGetReportDisplayDate = Format(dtstr, "mmmm yyyy")
End Function






Function fnInProjectMonthYear(proj As String, yr As Integer, fmnth As Integer) As Boolean
On Error GoTo PMY_err
Dim repYear As Integer
Dim repMonth As Integer
Dim bUseProjectYear As Integer
Dim ProjectStartMonth As Integer
Dim ProjectYearStarts As Date
Dim ProjectYearEnds As Date
Dim MonthDate As Date

bUseProjectYear = Nz(DLookup("UseProjectYear", "G_tlkpProject_RadTrackData", "ParentProject ='" & proj & "'"), 0)
ProjectStartMonth = DatePart("m", DLookup("StartDate", "G_tlkpProject_RadTrackData", "ParentProject ='" & proj & "'"))


repYear = Forms![frmProjectRadTrackData_Update]![Year]

If Not bUseProjectYear Then
    If yr = repYear Then
        fnInProjectMonthYear = True
    Else
        fnInProjectMonthYear = False
    End If
Else
    ProjectYearEnds = CDate(Format("01/" & ProjectStartMonth & "/" & repYear + 1, "dd/mm/yyyy"))
    ProjectYearStarts = DateAdd("YYYY", -1, ProjectYearEnds)
    MonthDate = DateAdd("m", 3, CDate(Format("01/" & fmnth & "/" & yr, "dd/mm/yyyy")))
    If MonthDate >= ProjectYearStarts And MonthDate < ProjectYearEnds Then
        fnInProjectMonthYear = True
    Else
        fnInProjectMonthYear = False
    End If

End If

Exit Function
PMY_err:
        fnInProjectMonthYear = False
        Exit Function
End Function


Function fnGetHiddenManager()
On Error Resume Next

Dim x As Variant

x = CStr(Forms![frmMenuReports]![HiddenManager])
If IsNull(x) Then
    fnGetHiddenManager = "*"
Else
    fnGetHiddenManager = x
End If
End Function

Function fnMonthToFMonth(M As Integer) As Integer
Dim x
x = (M - 3 + 12) Mod 12
If x = 0 Then x = 12
fnMonthToFMonth = x
End Function

Function fnReportingPeriod()
fnReportingPeriod = "Apr - " & [Forms]![frmMenuReports]![cbMonth].Column(1) & " " & [Forms]![frmMenuReports]![txtYear]
End Function

Function fnRepMilestoneWithDeliverable(txt As String, Program) As String
If IsNull(Program) Then
    fnRepMilestoneWithDeliverable = txt
ElseIf Program Like "*Res" Then
    fnRepMilestoneWithDeliverable = Replace(txt, "/Deliverable", "")
Else
    fnRepMilestoneWithDeliverable = Replace(txt, "Milestone/", "")
End If
End Function

Function Replace(strExpr As String, strFind As String, strReplace As String, Optional lngStart As Long = 1) As String
   Dim strOut As String
   Dim lngLenExpr As Long
   Dim lngLenFind As Long
   Dim lng As Long

   lngLenExpr = Len(strExpr)
   lngLenFind = Len(strFind)

   If (lngLenExpr > 0) And (lngLenFind > 0) And (lngLenExpr >= lngStart) Then
       lng = lngStart
       If lng > 1 Then
           strOut = Left$(strExpr, lng - 1)
       End If
       Do While lng <= lngLenExpr
           If Mid(strExpr, lng, lngLenFind) = strFind Then
               strOut = strOut & strReplace
               lng = lng + lngLenFind
           Else
               strOut = strOut & Mid(strExpr, lng, 1)
               lng = lng + 1
           End If
       Loop
       Replace = strOut
   End If
End Function


Function fnResearchProgram(Program) As Boolean
If Program Like "*Res" Then
    fnResearchProgram = True
Else
    fnResearchProgram = False
End If
End Function

Function fnSurvProgram(Program) As Boolean
If Program Like "*surv" Then
    fnSurvProgram = True
Else
    fnSurvProgram = False
End If
End Function

Function fnDecodeMilestoneDeliverable(Program) As String
If fnSurvProgram(Program) Then
    fnDecodeMilestoneDeliverable = "D"
Else
    fnDecodeMilestoneDeliverable = "M"
End If

End Function

Function fnForPeriodShortText()
    fnForPeriodShortText = " for Period: "
End Function

Function fnForPeriodText()
    fnForPeriodText = " for period to end "
End Function

Function fnForYearText()
    fnForYearText = " for "
End Function

Function fnForMonthText()
    fnForMonthText = " of "
End Function

Function fnForFinancialyearFullText()
    fnForFinancialyearFullText = " for financial year " & Forms![frmMenuReports]![txtYear] & "/" & (Forms![frmMenuReports]![txtYear] + 1) & ".  As at " & Date & "."
End Function


Function fnForFinancialyearText()
    fnForFinancialyearText = " for financial year " & Forms![frmMenuReports]![txtYear] & "/" & (Forms![frmMenuReports]![txtYear] + 1) & "."
End Function

Function fnForFinancialyear()
    fnForFinancialyear = " " & Forms![frmMenuReports]![txtYear] & "/" & (Forms![frmMenuReports]![txtYear] + 1)
End Function

Function IsEU_Project(Project) As Boolean
If Project Like "EU*" Then
    IsEU_Project = True
Else
    IsEU_Project = False
End If

End Function


Function fnInThisFinancialYear(dt) As Boolean
Dim StartDate As Date
Dim EndDate As Date
Dim yr As Integer
If Month(Date) >= 4 Then
    yr = Year(Date)
Else
    yr = Year(Date) - 1
End If

If IsNull(dt) Then
    fnInThisFinancialYear = False
Else
    StartDate = CDate("01/Apr/" & yr)
    EndDate = CDate("31/Mar/" & yr + 1)
    dt = CDate(Format(CDate(dt), "dd/mm/yy"))
    If dt >= StartDate And dt <= EndDate Then
        fnInThisFinancialYear = True
    Else
        fnInThisFinancialYear = False
    End If
End If

End Function

Function fnDecodeNull(com As Variant) As String
If IsNull(com) Then
    fnDecodeNull = " IS NULL"
Else
    fnDecodeNull = "='" & com & "'"
End If
End Function

Function fnDoneInPeriod(Submitted) As Boolean

    Dim target As Date
    
    target = fnReportEndOfMonth()
    
    If IsNull(Submitted) Then
        fnDoneInPeriod = False
    ElseIf Submitted <= target Then
        fnDoneInPeriod = True
    Else
        fnDoneInPeriod = False
    End If

End Function


Function fnDateFormRecieved(p As String, yr As Integer) As Variant

    fnDateFormRecieved = DLookup(Forms![frmMenuReports]![cbMonth].Column(1), "MY_Milestoneformdates", "ParentProject='" & p & "' and [year]=" & yr)
End Function


Function fnMilestoneDeliverableDueYear(dt, dn, MilestoneDeliverable)
    If MilestoneDeliverable = "M" Then
        fnMilestoneDeliverableDueYear = fnMilestoneYear(dt)
    ElseIf MilestoneDeliverable = "D" Then
        fnMilestoneDeliverableDueYear = fnDeliverableYear(dn)
    Else
        fnMilestoneDeliverableDueYear = Null
    End If
End Function

Function fnMilestoneYear(dt)
    'Milestones due as per the date due
    If IsNull(dt) Then
        fnMilestoneYear = Null
    Else
            dt = CDate(Format(CDate(dt), "dd/mm/yy"))
        If DatePart("m", dt) >= 4 Then
            fnMilestoneYear = DatePart("yyyy", dt)
        Else
            fnMilestoneYear = DatePart("yyyy", dt) - 1
        End If
    End If
End Function

Function fnDeliverableYear(dn)
'Deliverables due as per
'turns second part of deliverable number into year.
'e.g. 01/05 returns 2005
Dim x As Integer
'Dim y As String
On Error Resume Next
'y = Mid(dn, 3)
x = CInt(Mid(dn, 4))
If Err <> 0 Then
    fnDeliverableYear = Null
Else
    fnDeliverableYear = 2000 + x
End If
End Function

Function fnFinancialYear(dt)

    If IsNull(dt) Then
        fnFinancialYear = Null
    Else
        If DatePart("m", dt) < 4 Then
            fnFinancialYear = DatePart("yyyy", dt) - 1
        Else
            fnFinancialYear = DatePart("yyyy", dt)
        End If
    End If
End Function


Function fnOnTime(dtReported, dtDue) As String
    If IsNull(dtDue) Then
        fnOnTime = ""
    ElseIf IsNull(dtReported) Then
        If dtDue > Date Then
            fnOnTime = "Not due yet."
        Else
            fnOnTime = "No"
        End If
    Else
        If (dtReported <= dtDue) Then
            fnOnTime = "Yes"
        Else
            fnOnTime = "No"
        End If
    End If

End Function
