MODULE NAME: mdlApp
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Declare PtrSafe Function getUserName Lib "advapi32.dll" Alias "GetUserNameA" (ByVal lpBuffer As String, nSize As Long) As Long

Function fnGetUserName() As String

Dim zName As String * 128
Dim zLength As Long
Dim tmp As Long

zLength = 128

tmp = getUserName(zName, zLength)

fnGetUserName = Left(zName, zLength - 1)

End Function


Function fnCurrentYear()
Dim y As Integer

y = DatePart("yyyy", Date)

If DatePart("m", Date) < 4 Then
    y = y - 1
End If


fnCurrentYear = y

End Function

Function fnFMonthName(M As Integer)
'returns the month name from the financial month no
M = (M + 3) Mod 12
If M = 0 Then M = 12
fnFMonthName = Format("01/" & M & "/01", "MMM")
End Function

Function fnGetColour(ID As Variant)
fnGetColour = DLookup("Image", "TblImages", "tblImages.imageid = " & ID)
End Function



Function fnRadTrackInflation(InfType As String, proj As String, Year As Integer) As Double

Dim db As Database
Dim prs As Recordset
Dim inf As Boolean
Dim sfy As Integer
Dim startYear As Integer
Dim StartDate As Date
Dim CurrentYear As Integer
Dim YearGap As Integer
Dim PlanCat As String
Dim FYearStart As Date
Dim Inflation As Double
Dim Inflation2 As Double
Dim NoDays As Double
Dim PercentOfYear As Double
Dim FinancialYears As Integer

Dim InflationAsNumber As Double
Dim InflationAsNumber2 As Double

Set db = CurrentDb
Set prs = db.OpenRecordset("select  * from G_tlkpProject_RadTrackData where parentproject = '" & proj & "'")

With prs
    .MoveFirst
    If IsNull(!Inflation) Then
        inf = False
    Else
        inf = !Inflation
    End If
    'PlanCat = !PlanCat
    StartDate = !StartDate
    startYear = DatePart("yyyy", StartDate)
    .Close
End With

If inf = False Then
    fnRadTrackInflation = 1

Else
    FYearStart = CDate("01/04/" & startYear)
    CurrentYear = SettingsGet_BE("CurrentYear")
    YearGap = Year - CurrentYear
    PercentOfYear = Abs(FYearStart - StartDate) / 364
    
    If StartDate < FYearStart Then
        
        InflationAsNumber = (1 + (fnYearGapSign(YearGap - 1) * SettingsGet_BE(InfType)) / 100)
        Inflation = PercentOfYear * (InflationAsNumber ^ Abs(YearGap - 1))
        InflationAsNumber2 = (1 + (fnYearGapSign(YearGap) * SettingsGet_BE(InfType)) / 100)
        Inflation2 = (1 - PercentOfYear) * (InflationAsNumber2 ^ Abs(YearGap))
    Else
    
        InflationAsNumber = (1 + (fnYearGapSign(YearGap) * SettingsGet_BE(InfType)) / 100)
        Inflation = (1 - PercentOfYear) * (InflationAsNumber ^ Abs(YearGap))
        InflationAsNumber2 = (1 + (fnYearGapSign(YearGap + 1) * SettingsGet_BE(InfType)) / 100)
        Inflation2 = (PercentOfYear) * (InflationAsNumber2 ^ Abs(YearGap + 1))
    End If
    fnRadTrackInflation = Inflation + Inflation2
End If



End Function

Function fnYearGapSign(YearGap As Integer) As Integer
    Select Case YearGap
        Case Is = 0
            fnYearGapSign = 0
        Case Is > 0
            fnYearGapSign = 1
        Case Is < 0
            fnYearGapSign = -1
    End Select
    
End Function

Sub ldoAttachMAB_Archive_ODBCTables()

    Dim Connect_MAB As String
    
    'If LiveOrTest = "Live" Then
     '   Connect_MAB = "ODBC;DRIVER=SQL Server;SERVER=vla44;WSID=PCH01839;DATABASE=MAB_Archive;Trusted_Connection=Yes;"
    Connect_MAB = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"

    Dim MyDB As Database
    Dim MyTable As TableDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.TableDefs.Count - 1
        Set MyTable = MyDB.TableDefs(i)
        If MyTable.Attributes And DB_ATTACHEDODBC And MyTable.Connect Like "*MAB_Archive*" Then
            'MyTable.Connect = " "
            MyTable.Connect = Connect_MAB
            MyTable.RefreshLink
            'MyDB.TableDefs.Refresh
            'DoEvents
            'Debug.Print MyTable.Name; "  "; MyTable.SourceTableName; "  "; MyTable.Connect;
        Else
            Debug.Print "Ignored: "; MyTable.Name; " "; MyTable.SourceTableName
        End If
        
    Next i
    

End Sub


Sub attach_Passthrough_query()
    Dim Connect_MAB As String
    
    'If LiveOrTest = "Live" Then
     '   Connect_MAB = "ODBC;DRIVER=SQL Server;SERVER=vla44;WSID=PCH01839;DATABASE=MAB_Archive;Trusted_Connection=Yes;"
    Connect_MAB = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"
    
    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.QueryDefs.Count - 1
        Set MyQuery = MyDB.QueryDefs(i)
        If MyQuery.Type = dbQSQLPassThrough Or MyQuery.Type = dbQSPTBulk Then
            'MyTable.Connect = " "
            MyQuery.Connect = Connect_MAB
            'MyQuery.RefreshLink
            'MyDB.TableDefs.Refresh
            'DoEvents
            Debug.Print MyQuery.Name;
        Else
            'Debug.Print "Ignored: "; MyQuery.Name;
        End If
        
    Next i
End Sub

Function AttachTablesAndPTQueries()
    attach_Passthrough_query
    ldoAttachMAB_Archive_ODBCTables
End Function


Function fnGetFPSAnimalCosts(Project As String, Year As Integer) As Variant
    
    fnGetFPSAnimalCosts = DLookup("TotalAnimalCosts", "MY_FPSYearTotals", "ParentProject='" & Project & "' AND Year =" & Year)
    
End Function

Function fnGetFPSNonAnimalCosts(Project As String, Year As Integer) As Variant
    
    fnGetFPSNonAnimalCosts = DLookup("TotalAdditionalCosts", "MY_FPSYearTotals", "ParentProject='" & Project & "' AND Year =" & Year)
    
End Function
Function fnGetFPSTestCosts(Project As String, Year As Integer) As Variant
    
    fnGetFPSTestCosts = DLookup("TotalTestCosts", "MY_FPSYearTotals", "ParentProject='" & Project & "' AND Year =" & Year)
    
End Function
Function fnGetFPSTimeCosts(Project As String, Year As Integer) As Variant
    
    fnGetFPSTimeCosts = DLookup("TotalStaffCosts", "MY_FPSYearTotals", "ParentProject='" & Project & "' AND Year =" & Year)
    
End Function
Function fnGetFPSManHours(Project As String, Year As Integer) As Variant
    
    fnGetFPSManHours = DLookup("TotalManHours", "qryTotalManHours", "Project='" & Project & "' AND Year =" & Year)
    
End Function

Sub formcaption()
    
    Dim frm As Form, ctl As Control

    
    ' Enumerate Forms collection.
    For Each frm In Forms
        ' Print name of form.
        Debug.Print frm.Name
        frm.caption = "Rad Track"
        ' Enumerate Controls collection of each form.
        'For Each ctl In frm.Controls
            ' Print name of each control.
            'Debug.Print ">>>"; ctl.Name
        'Next ctl
    Next frm

End Sub

Function fnProgram(proj As String) As Variant
    fnProgram = DLookup("Program", "vProjectLatestDetails", "ParentProject='" & proj & "'")
    
End Function

Function fnShowWhichProjects()
If Forms![frmProjectMain]![ogShowWhichProjects] = 1 Then
    fnShowWhichProjects = "Y"
Else
    fnShowWhichProjects = "*"
End If
End Function

 Function fnRadtrack_ReportsProject() As String
 On Error GoTo R_RP_err
 Dim p As Variant
 p = Forms![frmRadtrack_Reports]![pickProject]
If IsNull(p) Then
    fnRadtrack_ReportsProject = "*"
Else
    fnRadtrack_ReportsProject = p
End If
Exit Function
R_RP_err:
    fnRadtrack_ReportsProject = "*"
    Exit Function
End Function

Function fnReportCommentsFromPL()
fnReportCommentsFromPL = "A&F Report"
End Function

Function fnCustomerReportComments()
fnCustomerReportComments = "Outturn Report"
End Function

Function fnPMonitoringReportComments()
fnPMonitoringReportComments = "P&C Monitoring Report"
End Function

Function fnHoursInYear() As Double

fnHoursInYear = SettingsGet_BE("HoursInDay") * SettingsGet_BE("DaysInYear")
End Function

Function fnHoursInDay() As Double

fnHoursInDay = SettingsGet_BE("HoursInDay")
End Function

Function fnDaysInYear() As Double

fnDaysInYear = SettingsGet_BE("DaysInYear")
End Function

Function fn4DigitYear(yr As Variant) As Variant
If IsNull(yr) Then
    fn4DigitYear = Null
ElseIf yr < 90 Then
    fn4DigitYear = yr + 2000
ElseIf yr < 100 Then
    fn4DigitYear = yr + 1900
Else
    fn4DigitYear = yr
End If
End Function

Function fnSystemName()
    fnSystemName = "PIMS"

End Function

Function fnSystemID()
fnSystemID = DLookup("SystemID", "tblAccessSystems", "SystemName='" & fnSystemName() & "'")
End Function

Function fnMaintainanceLevel()
    fnMaintainanceLevel = "Maintainance"
End Function

Function IsFPSProject() As Boolean
    If Forms![frmProjectMain]![pickProject].Column(3) = "Yes" Then
        IsFPSProject = True
    Else
        IsFPSProject = False
    End If
End Function


Function fnShowStatus(Is_Pims, Is_FPS As Boolean) As Boolean
If Forms![frmProjectMain]![pickProject].Column(3) = "Yes" Then
    If Is_FPS Then
        fnShowStatus = True
    Else
        fnShowStatus = False
    End If
ElseIf Forms![frmProjectMain]![pickProject].Column(3) = "No" Then
    If Is_Pims Then
        fnShowStatus = True
    Else
        fnShowStatus = False
    End If
End If
End Function

Function fnChangeProposedProjectCode(oldcode, newcode As String) As Boolean
On Error GoTo fnChangeProposedProjectCode_err
Dim sqlstr As String
Dim ws As Workspace, db As Database

Set ws = DBEngine(0)
Set db = ws(0)

ws.BeginTrans
sqlstr = "UPDATE tblComments SET tblComments.Project = '" & newcode & "' "
sqlstr = sqlstr & "WHERE (((tblComments.Project)='" & oldcode & "'));"
db.Execute sqlstr, dbSeeChanges

sqlstr = "UPDATE tblProposedproject SET tblProposedproject.parentProject = '" & newcode & "' "
sqlstr = sqlstr & "WHERE (((tblProposedproject.parentProject)='" & oldcode & "'));"
db.Execute sqlstr, dbSeeChanges

sqlstr = "UPDATE G_tlkpProject_RadTrackData SET G_tlkpProject_RadTrackData.parentProject = '" & newcode & "' "
sqlstr = sqlstr & "WHERE (((G_tlkpProject_RadTrackData.parentProject)='" & oldcode & "'));"
db.Execute sqlstr, dbSeeChanges
ws.CommitTrans
fnChangeProposedProjectCode = True
Exit Function

fnChangeProposedProjectCode_err:

    ws.Rollback
    fnChangeProposedProjectCode = False
    MsgBox Err.Number & " " & Err.Description
End Function


Function fn_sfC_PickYear()
fn_sfC_PickYear = [Forms]![sf_ProjectComments]![pickYear]
End Function


Function fnzoom()
DoCmd.RunCommand acCmdZoomBox
End Function

Public Function fnIsErrorToZero(n) As Double
On Error Resume Next
    If Not IsNumeric(n) Then
        fnIsErrorToZero = 0
    Else
        fnIsErrorToZero = n
    End If
    

End Function
