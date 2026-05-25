MODULE NAME: mdlCostbook
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function fnGetProject()
fnGetProject = [Forms]![frmProject1]![Project]
End Function



Function fnProjectYearCount(proj As String) As Variant
Dim db As Database
Dim pyrs As Recordset
Dim cnt As Integer

    Set db = CurrentDb
    Set pyrs = db.OpenRecordset("select Max(YearNo) as PYCount from tblProjectYear where project = '" & proj & "'")
    With pyrs
        .MoveFirst
        fnProjectYearCount = !pycount
        .Close
    End With
    
End Function




Function fnAddProjectYear(proj As String, Year As Integer) As Boolean


    Dim db As Database
    Dim pyrs As Recordset
    Dim rs As Recordset
    Dim sqlstr As String

    Set db = CurrentDb
    Set pyrs = db.OpenRecordset("tblProjectYear")
    If DLookup("Programme", "tblProject", "Project='" & proj & "'") <> "Comm" Then
        With pyrs
            .AddNew
            !Project = proj
            !YearNo = Year
            .Update
        End With
    Else
        'If values for Contingeny & Profit are empty, load from previous year, or default values.
        sqlstr = "select * from tblProjectYear where project ='" & proj & "' and [yearno] =" & (Year - 1)

        Set rs = db.OpenRecordset(sqlstr)
        If rs.BOF And rs.EOF Then
            With pyrs
                .AddNew
                !Project = proj
                !YearNo = Year
                !Profit_Time = SettingsGet_BE("Profitstaff")
                !Profit_Tests = SettingsGet_BE("Profittests")
                !Profit_Animals = SettingsGet_BE("ProfitAnimals")
                !Profit_Additional = SettingsGet_BE("ProfitExceptional")
                
                !Markup_Time = SettingsGet_BE("Markupstaff")
                !Markup_Tests = SettingsGet_BE("Markuptests")
                !Markup_Animals = SettingsGet_BE("MarkupAnimals")
                !Markup_Additional = SettingsGet_BE("MarkupExceptional")
                .Update
            End With
        Else
            rs.MoveFirst
            With pyrs
                .AddNew
                !Project = proj
                !YearNo = Year
                !Profit_Time = Nz(rs!Profit_Time, SettingsGet_BE("Profitstaff"))
                !Profit_Tests = Nz(rs!Profit_Tests, SettingsGet_BE("Profittests"))
                !Profit_Animals = Nz(rs!Profit_Animals, SettingsGet_BE("ProfitAnimals"))
                !Profit_Additional = Nz(rs!Profit_Additional, SettingsGet_BE("ProfitExceptional"))
                
                !Markup_Time = Nz(rs!Markup_Time, SettingsGet_BE("Markupstaff"))
                !Markup_Tests = Nz(rs!Markup_Tests, SettingsGet_BE("Markuptests"))
                !Markup_Animals = Nz(rs!Markup_Animals, SettingsGet_BE("MarkupAnimals"))
                !Markup_Additional = Nz(rs!Markup_Additional, SettingsGet_BE("MarkupExceptional"))
                .Update
            End With
    
    

    
        End If
    End If







    fnAddProjectYear = -1
    
    
End Function


Function fnInflation(InfType As String, proj As String, Year As Integer) As Double

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
Set prs = db.OpenRecordset("select  * from tblProject where project = '" & proj & "'")

With prs
    .MoveFirst
    inf = !Inflation
    'PlanCat = !PlanCat
    StartDate = !StartDate
    startYear = !StartFYear
    FinancialYears = !FinancialYears
    .Close
End With

If inf = False Then
    fnInflation = 1
ElseIf FinancialYears = True Then
    CurrentYear = SettingsGet_BE("CurrentYear")
    YearGap = Year - CurrentYear
    If YearGap < 0 Then YearGap = 0
    fnInflation = (1 + SettingsGet_BE(InfType) / 100) ^ YearGap
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
    fnInflation = Inflation + Inflation2
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


Function fnNextCBNumber(CBN As String) As String
Dim db As Database
Dim rs As Recordset
Dim sqlstr As String
Dim CBYear As String

If DatePart("m", Now()) <= 3 Then
    CBYear = DatePart("YYYY", Now()) - 1
Else
    CBYear = DatePart("YYYY", Now())
End If
    
Set db = CurrentDb
 

If CBN = "" Then
    sqlstr = "Select Max(Mid(Project,6,3)) from tblProject where project like '" & CBYear & "/*'"
    Set rs = db.OpenRecordset(sqlstr)
    With rs
        .MoveFirst
        If IsNull(rs.Fields(0)) Then
            fnNextCBNumber = CBYear & "/001"
        Else
            fnNextCBNumber = CBYear & "/" & Format(1 + .Fields(0), "000")
        End If
    End With
Else
    If Len(CBN) = 9 And Right(CBN, 1) Like "[a-z]" Then
        sqlstr = " Select Max(Project) from tblProject where project like '" & Left(CBN, 8) & "*' "
    Else
        sqlstr = " Select Max(Project) from tblProject where project like '" & CBN & "*' "
    End If
    Set rs = db.OpenRecordset(sqlstr)
    With rs
        .MoveFirst
        If IsNull(rs.Fields(0)) Then
            fnNextCBNumber = CBN
        ElseIf (Len(CBN) = 8 Or Len(CBN) = 9) And Len(.Fields(0)) = 9 And Right(.Fields(0), 1) Like "[a-z]" Then
            fnNextCBNumber = Left(CBN, 8) & Chr(Asc(Right(.Fields(0), 1)) + 1)
        Else
            fnNextCBNumber = .Fields(0) & "a"
        End If
    End With
End If
rs.Close
End Function

Function fnInsertProject(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " INSERT INTO tblProject ( Project, PlanCat, ProjectTitle, ProjectWorkGroup, ContractPrice, StartDate, Disease, StartFYear, [Customer Name], [Contract Number], [SubmittedByFName],[SubmittedByLName], [Date of Submission], [Prepared by], Inflation, FinancialYears, Notes,EuroConvRate,programme, isdefraproject ) "
sqlstr = sqlstr & " SELECT '" & newCBN & "' AS Expr1, tblProject.PlanCat, tblProject.ProjectTitle, tblProject.ProjectWorkGroup, tblProject.ContractPrice, tblProject.StartDate, tblProject.Disease, tblProject.StartFYear, tblProject.[Customer Name], tblProject.[Contract Number], tblProject.[SubmittedByFName],tblProject.[SubmittedByLName], tblProject.[Date of Submission], tblProject.[Prepared by], tblProject.Inflation, tblProject.FinancialYears, tblProject.Notes,tblProject.EuroConvRate, tblProject.programme, tblProject.isdefraproject "
sqlstr = sqlstr & " FROM tblProject WHERE project ='" & oldCBN & " ';"

Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnInsertProject = Err.Number
Debug.Print sqlstr

End Function

Function fnInsertProjectYear(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = "INSERT INTO tblProjectYear ( Project, YearNo ) "
sqlstr = sqlstr & " SELECT '" & newCBN & "', tblProjectYear.YearNo "
sqlstr = sqlstr & " FROM tblProjectYear "
sqlstr = sqlstr & " WHERE (((tblProjectYear.Project)='" & oldCBN & "'));"



Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnInsertProjectYear = Err.Number
Debug.Print sqlstr

End Function

Function fnInsertProjectAdditionalCosts(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = " INSERT INTO tblAdditionalCosts ( Project, Year, AccountCat, Description,  ItemCost, CostEntered, Freq )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', tblAdditionalCosts.Year, tblAdditionalCosts.AccountCat, tblAdditionalCosts.Description, tblAdditionalCosts.ItemCost, tblAdditionalCosts.CostEntered, tblAdditionalCosts.Freq"
sqlstr = sqlstr & " FROM tblAdditionalCosts "
sqlstr = sqlstr & " WHERE (((tblAdditionalCosts.Project)='" & oldCBN & "'));"


Debug.Print sqlstr


Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnInsertProjectAdditionalCosts = Err.Number
Debug.Print sqlstr

End Function

Function fnInsertProjectAnimals(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = " INSERT INTO tblAnimalReq ( Project, Year, AnimalType, [Number of Days], [Number of Animals], DailyRate )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', tblAnimalReq.Year, tblAnimalReq.AnimalType, tblAnimalReq.[Number of Days], tblAnimalReq.[Number of Animals], tblAnimalReq.DailyRate"
sqlstr = sqlstr & " FROM  tblAnimalReq "
sqlstr = sqlstr & " WHERE (((tblAnimalReq.Project)='" & oldCBN & "'));"



Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnInsertProjectAnimals = Err.Number
Debug.Print sqlstr

End Function

Function fnInsertProjectStaff(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " INSERT INTO tblStaffRequ ( Project, Year, WGGrade, Name, NoHours, NoDays, ChargeRate, PayRate,NPR,OHR )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', tblStaffRequ.Year, tblStaffRequ.WGGrade, tblStaffRequ.name, tblStaffRequ.NoHours, tblStaffRequ.NoDays, tblStaffRequ.ChargeRate, PayRate,NPR,OHR "
sqlstr = sqlstr & " FROM tblStaffRequ "
sqlstr = sqlstr & " WHERE (((tblStaffRequ.Project)='" & oldCBN & "'));"

Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnInsertProjectStaff = Err.Number
Debug.Print sqlstr

End Function


Function fnInsertProjectTests(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = " INSERT INTO tblTestRequ ( Project, Year, TestCode, NoTests, UnitPrice )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', tblTestRequ.Year, tblTestRequ.TestCode, tblTestRequ.NoTests, tblTestRequ.UnitPrice"
sqlstr = sqlstr & " FROM tblTestRequ "
sqlstr = sqlstr & " WHERE (((tblTestRequ.Project)='" & oldCBN & "'));"

Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnInsertProjectTests = Err.Number


End Function

Function ldoAttachFPS_ODBCTables()

    Dim MyDB As Database
    Dim MyTable As TableDef
    Dim i As Integer
    Dim strPCHNo As String
    Dim ODBC_str As String
    
    'ODBC_str = "ODBC;DRIVER=SQL Server;SERVER=vla44;WSID=PCH01839;DATABASE=FPS" & Settingsset_BE("CurrentYear") & ";QueryLogFile=Yes;Trusted_Connection=Yes;"
    ODBC_str = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";WSID=PCH01839;DATABASE=FPS" & SettingsGet_BE("CurrentYear") & ";Trusted_Connection=Yes;"
    
    Set MyDB = CurrentDb()
    For i = 0 To MyDB.TableDefs.Count - 1
        Set MyTable = MyDB.TableDefs(i)
        If MyTable.Attributes And DB_ATTACHEDODBC And MyTable.Connect Like "*FPS*" Then
            'MyTable.Connect = " "
            MyTable.Connect = ODBC_str
            MyTable.RefreshLink
            'MyDB.TableDefs.Refresh
            'DoEvents
            Debug.Print MyTable.Name; "  "; MyTable.SourceTableName; "  "; MyTable.Connect;
        Else
            Debug.Print "Ignored: "; MyTable.Name; " "; MyTable.SourceTableName
        End If
        
    Next i
    

End Function
'Function fnReattachFPSTables()
'DoEvents
'ldoAttachODBCTables
'End Function

Function fnRecostProject(Project As String) As Integer
Dim db As Database
Dim pt As Recordset
Dim sqlstr As String
Dim inflatedCost As Double
Dim IsDefraProject As Boolean
Dim qName As String

If DLookup("isdefraproject", "tblProject", "project='" & Project & "'") = 0 Then
    IsDefraProject = False
Else
    IsDefraProject = True
End If

Dim fyear As Integer
fyear = CInt(SettingsGet_BE("CurrentYear"))

sqlstr = "Select * from tblTestRequ where project='" & Project & "' and year>=" & fyear
Set db = CurrentDb
Set pt = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)

With pt
    If Not (.EOF And .BOF) Then
        .MoveFirst
        While Not .EOF
            .Edit
            If IsDefraProject Then
                .Fields("UnitPrice") = DLookup("[DefraUnitPrice]", "tblTest", "itemCode='" & .Fields("TestCode") & "'") * fnInflation("InflationTests", Project, .Fields("Year"))
            Else
                .Fields("UnitPrice") = DLookup("[UnitPriceVLA]", "tblTest", "itemCode='" & .Fields("TestCode") & "'") * fnInflation("InflationTests", Project, .Fields("Year"))
            End If
            .Update
            .MoveNext
        Wend
        
    End If
End With
pt.Close

If IsDefraProject Then
    qName = "qrypayRates_defra"
Else
    qName = "qrypayRates_nondefra"
End If
sqlstr = "Select * from tblstaffRequ where project='" & Project & "' and year>=" & fyear
Set pt = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)

With pt
    If Not (.EOF And .BOF) Then
        .MoveFirst
        While Not .EOF
            .Edit
            .Fields("chargerate") = Nz(DLookup("[chargerate]", qName, "WGGrade='" & .Fields("WGGrade") & "'"), 0) * fnInflation("InflationStaff", Project, .Fields("Year"))
            .Fields("PayRate") = Nz(DLookup("[payrate]", qName, "WGGrade='" & .Fields("WGGrade") & "'"), 0) * fnInflation("InflationStaff", Project, .Fields("Year"))
            .Fields("NPR") = Nz(DLookup("[NPR]", qName, "WGGrade='" & .Fields("WGGrade") & "'"), 0) * fnInflation("InflationStaff", Project, .Fields("Year"))
            .Fields("OHR") = Nz(DLookup("[OHR]", qName, "WGGrade='" & .Fields("WGGrade") & "'"), 0) * fnInflation("InflationStaff", Project, .Fields("Year"))
            .Update
            .MoveNext
        Wend
        
    End If
End With
pt.Close
sqlstr = "Select * from tblAnimalReq where project='" & Project & "' and year>=" & fyear
Set pt = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)

With pt
    If Not (.EOF And .BOF) Then
        .MoveFirst
        While Not .EOF
            .Edit
            If IsDefraProject Then
                .Fields("Dailyrate") = DLookup("[defradailyrate]", "tblAnimals", "AnimalType='" & .Fields("AnimalType") & "'") * fnInflation("InflationAnimals", Project, .Fields("Year"))
            Else
                .Fields("Dailyrate") = DLookup("[dailyrate]", "tblAnimals", "AnimalType='" & .Fields("AnimalType") & "'") * fnInflation("InflationAnimals", Project, .Fields("Year"))
            End If
            .Update
            .MoveNext
        Wend
        
    End If
End With
pt.Close
sqlstr = "Select * from tblAdditionalCosts where project='" & Project & "' and year>=" & fyear
Set pt = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)

With pt
    If Not (.EOF And .BOF) Then
        .MoveFirst
        While Not .EOF
            If fnUseInflation(.Fields("AccountCat")) Then
                inflatedCost = .Fields("CostEntered") * fnInflation("InflationExceptional", Project, .Fields("Year"))
            Else
                inflatedCost = .Fields("CostEntered")
            End If
            .Edit
            .Fields("ItemCost") = inflatedCost
            .Update
            .MoveNext
        Wend
        
    End If
End With
pt.Close
End Function

Function fnUseInflation(AccCode As String) As Boolean
Dim Val As Variant

Val = DLookup("UseInflation", "qryUseInflation", "AccShortName='" & AccCode & "'")
If IsNull(Val) Then
    fnUseInflation = False
Else
    fnUseInflation = Val
End If
End Function
Sub ExportToExcel(Project As String)
On Error Resume Next

Dim rs As Recordset
Dim rsd As Recordset
Dim db As Database
Dim sqlstr As String
Dim appExcel As Object
Dim bExit As Boolean
Dim startrownum As Integer
Dim rownum As Integer
Dim maxrownum As Integer

Set appExcel = GetObject(, "Excel.application") 'activate Excel if open
If Err <> 0 Then
    Set appExcel = CreateObject("Excel.application") 'Open Excel if not open
End If

On Error GoTo EE_Err
Set db = CurrentDb


With appExcel
    .Workbooks.Add
    sqlstr = "Select * from tblProject where project = '" & Project & "'"
    
    Set rs = db.OpenRecordset(sqlstr)
    With rs
        .MoveFirst
        appExcel.Cells(1, 1) = "Costbook Project Summary"
        appExcel.Cells(1, 1).Font.Size = 14
        appExcel.Cells(1, 1).RowHeight = 30
        appExcel.Cells(1, 1).Font.Bold = True
        
        appExcel.Cells(2, 1) = "Project"
        appExcel.Cells(2, 1).Font.Bold = True
        appExcel.Cells(2, 2) = .Fields("Project")
       ' appExcel.Cells(3, 1) = "Planning Category"
        'appExcel.Cells(3, 1).Font.Bold = True
        'appExcel.Cells(3, 2) = .Fields("PlanCat")
        appExcel.Cells(4, 1) = "Inflation"
        appExcel.Cells(4, 1).Font.Bold = True
        appExcel.Cells(4, 2) = .Fields("inflation")
        
        rownum = 6
    End With
    
    
    sqlstr = "Select * from tblProjectYear where project = '" & Project & "'"
    
    Set rs = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)
    startrownum = 7
    rownum = startrownum
    With rs
        If .BOF And .EOF Then
            bExit = True
        Else
            .MoveFirst
            
            'maxrownum = startrownum
            While Not .EOF
                
                appExcel.Cells(rownum, 1) = .Fields("YearNo")
                appExcel.Cells(rownum, 1).Font.Bold = True
                '******
                sqlstr = "Select * from tblStaffRequ where project = '" & Project & "' and Year = " & .Fields("yearno")
    
                Set rsd = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)
                With rsd
                    If .BOF And .EOF Then
                    Else
                        .MoveFirst
                        'rownum = startrownum
                        appExcel.Cells(rownum, 2) = ("WG Grade")
                        appExcel.Cells(rownum, 2).Font.Bold = True
                        appExcel.Cells(rownum, 3) = ("Charge Rate")
                        appExcel.Cells(rownum, 3).Font.Bold = True
                        appExcel.Cells(rownum, 4) = ("No Hours")
                        appExcel.Cells(rownum, 4).Font.Bold = True
                        appExcel.Cells(rownum, 6) = ("Cost")
                        appExcel.Cells(rownum, 6).Font.Bold = True
                        rownum = rownum + 1
 
                        While Not .EOF
                        
                            appExcel.Cells(rownum, 2) = .Fields("WGGrade")
                            appExcel.Cells(rownum, 3) = .Fields("ChargeRate")
                            'appExcel.Cells(rownum, 3).Format = "Currency"
                            appExcel.Cells(rownum, 4) = .Fields("NoHours")
                            appExcel.Cells(rownum, 6) = .Fields("Chargerate") * .Fields("NoHours")
                            .MoveNext
                            rownum = rownum + 1
                            'If rownum > maxrownum Then maxrownum = rownum
                        Wend
                    End If
                End With
                '********
                sqlstr = "Select * from tblTestRequ where project = '" & Project & "' and Year = " & .Fields("yearno")
    
                Set rsd = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)
                With rsd
                    If .BOF And .EOF Then
                    Else
                        .MoveFirst
                        'rownum = startrownum
                        
                        appExcel.Cells(rownum, 2) = ("Test Code")
                        appExcel.Cells(rownum, 2).Font.Bold = True
                        appExcel.Cells(rownum, 3) = ("Unit Price")
                        appExcel.Cells(rownum, 3).Font.Bold = True

                        appExcel.Cells(rownum, 4) = ("No Tests")
                        appExcel.Cells(rownum, 4).Font.Bold = True
                        appExcel.Cells(rownum, 6) = ("Cost")
                        appExcel.Cells(rownum, 6).Font.Bold = True
                        rownum = rownum + 1
                        While Not .EOF
                        
                            appExcel.Cells(rownum, 2) = .Fields("TestCode")
                            appExcel.Cells(rownum, 3) = .Fields("UnitPrice")
                            appExcel.Cells(rownum, 4) = .Fields("NoTests")
                            appExcel.Cells(rownum, 6) = .Fields("UnitPrice") * .Fields("NoTests")
                            .MoveNext
                            rownum = rownum + 1
                            'If rownum > maxrownum Then maxrownum = rownum
                        Wend
                    End If
                End With
                sqlstr = "Select * from tblAnimalReq where project = '" & Project & "' and Year = " & .Fields("yearno")
    
                Set rsd = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)
                With rsd
                    If .BOF And .EOF Then
                    Else
                        .MoveFirst
                        'rownum = startrownum
                        appExcel.Cells(rownum, 2) = ("Animal Type")
                        appExcel.Cells(rownum, 2).Font.Bold = True
                        appExcel.Cells(rownum, 3) = ("Daily Rate")
                        appExcel.Cells(rownum, 3).Font.Bold = True

                        appExcel.Cells(rownum, 4) = ("Number of Days")
                        appExcel.Cells(rownum, 4).Font.Bold = True
                        appExcel.Cells(rownum, 5) = ("Number of Animals")
                        appExcel.Cells(rownum, 5).Font.Bold = True
                        appExcel.Cells(rownum, 6) = ("Cost")
                        appExcel.Cells(rownum, 6).Font.Bold = True
                        rownum = rownum + 1
                        While Not .EOF
                        
                            appExcel.Cells(rownum, 2) = .Fields("AnimalType")
                            appExcel.Cells(rownum, 3) = .Fields("DailyRate")
                            appExcel.Cells(rownum, 4) = .Fields("Number of Days")
                            appExcel.Cells(rownum, 5) = .Fields("Number of Animals")
                            appExcel.Cells(rownum, 6) = .Fields("DailyRate") * .Fields("Number of Days") * .Fields("Number of Animals")
                            .MoveNext
                            rownum = rownum + 1
                            'If rownum > maxrownum Then maxrownum = rownum
                        Wend
                       '     Columns("C:C").Select
                        'Selection.NumberFormat = "$#,##0.00"
                        'Columns("E:E").Select
                         'Selection.NumberFormat = "$#,##0.00"
                        'Columns("G:G").Select
                        'Selection.NumberFormat = "$#,##0.00"
                        'Columns("I:I").Select
                        'Selection.NumberFormat = "$#,##0.00"
                        'Columns("K:K").Select
                        'Selection.NumberFormat = "$#,##0.00"
                        'Columns("N:N").Select
                        'Selection.NumberFormat = "$#,##0.00"
                        'Columns("R:R").Select
                        'Selection.NumberFormat = "$#,##0.00"
                        'Columns("S:S").Select
                        'Selection.NumberFormat = "$#,##0.00"
                    End If
                End With
                
                sqlstr = "Select * from tblAdditionalcosts where project = '" & Project & "' and Year = " & .Fields("yearno")
    
                Set rsd = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)
                With rsd
                    If .BOF And .EOF Then
                    Else
                        .MoveFirst
                        'rownum = startrownum
                        
                        appExcel.Cells(rownum, 2) = ("Description")
                        appExcel.Cells(rownum, 2).Font.Bold = True
                        appExcel.Cells(rownum, 3) = ("Account Cat")
                        appExcel.Cells(rownum, 3).Font.Bold = True
                        appExcel.Cells(rownum, 6) = ("Cost")
                        appExcel.Cells(rownum, 6).Font.Bold = True
                        rownum = rownum + 1
                        
                        While Not .EOF
                        
                            appExcel.Cells(rownum, 2) = .Fields("Description")
                            appExcel.Cells(rownum, 3) = .Fields("AccountCat")
                            'a ppExcel.Cells(rownum, 17) = .Fields("Number")
                            appExcel.Cells(rownum, 6) = .Fields("ItemCost")
                            'appExcel.Cells(rownum, 19) = .Fields("Number") * .Fields("ItemCost")
                            .MoveNext
                            rownum = rownum + 1
                            'If rownum > maxrownum Then maxrownum = rownum
                        Wend
                    End If
                End With
                .MoveNext
                rownum = rownum + 1
                
                'startrownum = maxrownum + 1
            Wend
        End If
    End With
    
    'appExcel.Columns("C").Select
    'Selection.NumberFormat = "$#,##0.00"
    'appExcel.Columns("F").Select
    'Selection.NumberFormat = "$#,##0.00"
    appExcel.Columns("C").NumberFormat = "$#,##0.00"
    appExcel.Columns("F").NumberFormat = "$#,##0.00"
    rs.Close
    appExcel.Visible = True
End With




Exit Sub
EE_Err:
    MsgBox Err.Number & " " & Err.Description
    Exit Sub
    
End Sub

Function fnCompleted(opt) As Boolean
If Forms![frmImportList]![ogCompleted] = 1 Then
    If opt Then
        fnCompleted = True
    Else
        fnCompleted = False
    End If
Else
    fnCompleted = True
End If

End Function


Function fnImportProject(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " INSERT INTO tblProject ( Project, PlanCat, ProjectTitle, ProjectWorkGroup, ContractPrice, StartDate, Disease, StartFYear, [Customer Name], [Contract Number], [Submitted by], [Date of Submission], [Prepared by], Inflation,FinancialYears,Notes ) "
sqlstr = sqlstr & " SELECT '" & newCBN & "' AS Expr1,  iif(temptblProject.PlanCat='None',NULL,temptblProject.PlanCat), temptblProject.ProjectTitle, temptblProject.ProjectWorkGroup, temptblProject.ContractPrice, temptblProject.StartDate, temptblProject.Disease, temptblProject.StartFYear, temptblProject.[Customer Name], temptblProject.[Contract Number], temptblProject.[Submitted by], temptblProject.[Date of Submission], temptblProject.[Prepared by], temptblProject.Inflation, temptblProject.FinancialYears, temptblProject.Notes "
sqlstr = sqlstr & " FROM temptblProject WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnImportProject = Err.Number
Debug.Print sqlstr

End Function

Function fnImportProjectYear(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = "INSERT INTO tblProjectYear ( Project, YearNo ) "
sqlstr = sqlstr & " SELECT '" & newCBN & "', temptblProjectYear.YearNo "
sqlstr = sqlstr & " FROM temptblProjectYear "
sqlstr = sqlstr & " WHERE (((temptblProjectYear.Project)=" & oldCBN & "));"



Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnImportProjectYear = Err.Number
Debug.Print sqlstr

End Function

Function fnImportProjectAdditionalCosts(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = " INSERT INTO tblAdditionalCosts ( Project, Year, AccountCat, Description,  ItemCost, CostEntered, Freq )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', temptblAdditionalCosts.Year, temptblAdditionalCosts.AccountCat, temptblAdditionalCosts.Description, temptblAdditionalCosts.Number, temptblAdditionalCosts.ItemCost, temptblAdditionalCosts.CostEntered, temptblAdditionalCosts.Freq"
sqlstr = sqlstr & " FROM temptblAdditionalCosts  "
sqlstr = sqlstr & " WHERE (((temptblAdditionalCosts.Project)=" & oldCBN & "));"





Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnImportProjectAdditionalCosts = Err.Number
Debug.Print sqlstr

End Function

Function fnImportProjectAnimals(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = " INSERT INTO tblAnimalReq ( Project, Year, AnimalType, [Number of Days], [Number of Animals], DailyRate )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', temptblAnimalReq.Year, temptblAnimalReq.AnimalType, temptblAnimalReq.[Number of Days], temptblAnimalReq.[Number of Animals], temptblAnimalReq.DailyRate"
sqlstr = sqlstr & " FROM  temptblAnimalReq  "
sqlstr = sqlstr & " WHERE (((temptblAnimalReq.Project)=" & oldCBN & "));"



Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnImportProjectAnimals = Err.Number
Debug.Print sqlstr

End Function

Function fnImportProjectStaff(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " INSERT INTO tblStaffRequ ( Project, Year, WGGrade, Name, NoHours, NoDays, ChargeRate )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', temptblStaffRequ.Year, temptblStaffRequ.WGGrade, temptblStaffRequ.name, temptblStaffRequ.NoHours, temptblStaffRequ.NoDays, temptblStaffRequ.ChargeRate"
sqlstr = sqlstr & " FROM temptblStaffRequ  "
sqlstr = sqlstr & " WHERE (((temptblStaffRequ.Project)=" & oldCBN & "));"

Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnImportProjectStaff = Err.Number
Debug.Print sqlstr

End Function


Function fnImportProjectTests(oldCBN As String, newCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next


sqlstr = " INSERT INTO tblTestRequ ( Project, Year, TestCode, NoTests, UnitPrice )"
sqlstr = sqlstr & " SELECT '" & newCBN & "', temptblTestReq.Year, temptblTestReq.TestCode, temptblTestReq.NoTests, temptblTestReq.UnitPrice"
sqlstr = sqlstr & " FROM temptblTestReq "
sqlstr = sqlstr & " WHERE (((temptblTestReq.Project)=" & oldCBN & "));"

Set db = CurrentDb
'
db.Execute sqlstr, dbFailOnError
fnImportProjectTests = Err.Number


End Function

Function fnDeleteTempProject(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM temptblProject  WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteTempProject = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteTempProjectYear(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM temptblProjectYear  WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteTempProjectYear = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteTempStaff(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM temptblStaffRequ  WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteTempStaff = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteTempTests(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM temptblTestReq  WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteTempTests = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteTempAdditionalCosts(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM temptblAdditionalCosts  WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteTempAdditionalCosts = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteTempAnimalReq(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM temptblAnimalReq WHERE project =" & oldCBN & " ;"

Set db = CurrentDb
'
'db.Execute sqlstr, dbFailOnError
db.Execute sqlstr, dbSeeChanges
fnDeleteTempAnimalReq = Err.Number
Debug.Print sqlstr

End Function

Function ldoAttachMAB_Archive_ODBCTables()
On Error GoTo Attatch_err

    Dim Connect_MAB As String
    'Connect_MAB = "ODBC;DRIVER=SQL Server;SERVER=vla39;WSID=PCH01839;DATABASE=MAB_Archive;Trusted_Connection=Yes;"
    Connect_MAB = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("AzureServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"
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
            Debug.Print MyTable.Name; "  "; MyTable.SourceTableName; "  "; MyTable.Connect; Chr(13) & Chr(10)
        Else
            Debug.Print "Ignored: "; MyTable.Name; " "; MyTable.SourceTableName; "  "; Chr(13) & Chr(10)
        End If
        
    Next i
Exit Function
Attatch_err:
            Debug.Print "error: "; MyTable.Name; " "; MyTable.SourceTableName; "  "; MyTable.Connect; Chr(13) & Chr(10)
            Resume Next
End Function

Function fnProjectSelected()
fnProjectSelected = [Forms]![frmProject]![Project]
End Function
Function fnDeleteProject(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM tblProject  WHERE project ='" & oldCBN & "' ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteProject = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteProjectYear(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM tblProjectYear  WHERE project ='" & oldCBN & "' ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteProjectYear = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteStaff(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM tblStaffRequ  WHERE project ='" & oldCBN & "' ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteStaff = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteTests(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM tblTestRequ  WHERE project ='" & oldCBN & "' ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteTests = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteAdditionalCosts(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM tblAdditionalCosts  WHERE project ='" & oldCBN & "' ;"

Set db = CurrentDb
'
db.Execute sqlstr, dbSeeChanges
fnDeleteAdditionalCosts = Err.Number
Debug.Print sqlstr

End Function

Function fnDeleteAnimalReq(oldCBN As String) As Integer
Dim db As Database
Dim sqlstr As String
Dim qd As QueryDef
On Error Resume Next

sqlstr = " Delete FROM tblAnimalReq WHERE project ='" & oldCBN & "' ;"

Set db = CurrentDb
'
'db.Execute sqlstr, dbFailOnError
db.Execute sqlstr, dbSeeChanges
fnDeleteAnimalReq = Err.Number
Debug.Print sqlstr

End Function



Function fnProfit(ptype As String) As Double
'was    fnProfit = (SettingsGet_BE(ptype) + 100) / 100
    Dim p As Double
    p = (SettingsGet_BE(ptype)) / 100
    fnProfit = 1 + (p / (1 - p))
End Function


Function fnIsCommercialProgram(pv As Variant) As Boolean
'On Error Resume Next
    If IsNull(pv) Or IsEmpty(pv) Or IsMissing(pv) Then
        fnIsCommercialProgram = False
    ElseIf pv = "Comm" Then
        fnIsCommercialProgram = True
    Else
        fnIsCommercialProgram = False
    End If

End Function

Function fnDefraRateAdjustment(IsDefraProject As Boolean) As Double

    If IsDefraProject Then
        fnDefraRateAdjustment = CDbl(SettingsGet("DefraRateAdjustment"))
    Else
        fnDefraRateAdjustment = 1
    End If
End Function
