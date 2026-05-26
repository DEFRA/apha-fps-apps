Option Compare Database
Option Explicit

Global Const xlCenter = -4108
Global Const xlBottom = -4107
Global Const xlContext = -5002
Global Const xlSolid = 1
Global Const xlGeneral = 1
Global Const xlAutomatic = -4105
Global Const xlExcel8 = 56
Global Const xlCalculationAutomatic = -4105
Global Const xlCalculationManual = -4135
Global Const xlLocalSessionChanges = 2
Global Const xlNormal = -4143


'Global gCos90User As String
'
'Public Function GetCos90User()
'    GetCos90User = gCos90User
'End Function
'
'Public Function SetCos90User(c)
'     gCos90User = c
'End Function



Function MakeExcelCos90(pactID As String) As Boolean

    Dim XL As Object
    Dim WB As Object
    Dim WKS As Object
    Dim db As DAO.Database, rec As DAO.Recordset, f As DAO.Field
    Dim i As Integer, j As Integer
    Dim intDayNumber As Integer
    Dim intWorkingDays As Integer
    Dim intWorkingHours As Integer
    Dim MyDB As Database
    Dim rst As Recordset
    Dim MonthRs As Recordset
    Dim strSQL As String
    Dim strDiv As String
    Dim fMonth As Integer
    Dim intYear As Integer
    Dim intMonth As Integer
    Dim intDay As Integer
    Dim varDate As Variant
    Dim intDaysInMonth As Integer
    Dim intPeriod As Integer
    Dim MonthName As String
    Dim UserName As String
    Dim WG As String
    Dim GC As String
        
    On Error Resume Next
    Set XL = GetObject(, "Excel.Application")
    If Err.Number <> 0 Then
        Set XL = CreateObject("Excel.Application")
    Else
        'MsgBox "Please close Excel and try again." ' Doesnt play well with others...
        'Exit Sub
    End If
    
    'On Error GoTo cos90_err:
    
    Set WB = XL.Workbooks.Add(1)
    WB.Activate
    XL.Calculation = xlCalculationManual
    Set WKS = WB.ActiveSheet
    WKS.Name = "Cos90"

    Set db = CurrentDb()
    Set rec = db.OpenRecordset("select * from qryCos90Excel where pactID='" & pactID & "'")

    With rec
        .MoveFirst
        UserName = .Fields("Name")
        WKS.cells(2, 1).Value = "Name:"
        WKS.cells(2, 2).Value = UserName
        WKS.cells(3, 1).Value = "SP Number:"
        WKS.cells(3, 2).Value = .Fields("SPNumber")
        WKS.cells(6, 1).Value = "Time Code:"
        WKS.cells(6, 2).Value = "Project Code:"
        WG = .Fields("Workgroup")
        GC = .Fields("GradeCode")
        
        intMonth = DLookup("MonthNumber", "tlkpMonth", "AccntsPeriod =" & Forms![frmCOS90]![Month])
        intYear = Forms!frmCOS90!Year   'Year(Now)
        intDay = 1
        fMonth = Forms![frmCOS90]![Month]
        intDaysInMonth = DaysInMonth2(DateSerial(intYear, intMonth, intDay))
        MonthName = DLookup("MonthName", "tlkpMonth", "AccntsPeriod = " & fMonth)

        For i = 1 To intDaysInMonth
            WKS.cells(6, 3 + i).Value = i
            
            If Not i = intDaysInMonth + 1 Then
                intDayNumber = WeekDay(DateSerial(intYear, intMonth, i)) ' & Chr(13) & Chr(10) & Format$(i)
                
                WKS.Columns(3 + i).Select
                With XL.Selection
                    .HorizontalAlignment = xlCenter
                    .VerticalAlignment = xlBottom
                    .WrapText = False
                    .Orientation = 0
                    .AddIndent = False
                    .IndentLevel = 0
                    .ShrinkToFit = False
                    .ReadingOrder = xlContext
                    .MergeCells = False
                End With
                
                Select Case intDayNumber
                    Case 1
                        WKS.cells(5, 3 + i).Value = "Sun"

                        WKS.Columns(3 + i).Select
                        With XL.Selection.Interior
                            .ColorIndex = 15
                            .pattern = xlSolid
                            .PatternColorIndex = xlAutomatic
                        End With
                    Case 2
                        WKS.cells(5, 3 + i).Value = "Mon"
                    Case 3
                        WKS.cells(5, 3 + i).Value = "Tues"
                    Case 4
                        WKS.cells(5, 3 + i).Value = "Wed"
                    Case 5
                        WKS.cells(5, 3 + i).Value = "Thur"
                    Case 6
                        WKS.cells(5, 3 + i).Value = "Fri"
                    Case 7
                        WKS.cells(5, 3 + i).Value = "Sat"
                        WKS.Columns(3 + i).Select
                        With XL.Selection.Interior
                            .ColorIndex = 15
                            .pattern = xlSolid
                            .PatternColorIndex = xlAutomatic
                        End With
                End Select
            End If

            WKS.cells(5, 3 + i).Select
            With XL.Selection
                    .HorizontalAlignment = xlCenter
                    .VerticalAlignment = xlBottom
                    .WrapText = False
                    .Orientation = 90
                    .AddIndent = False
                    .IndentLevel = 0
                    .ShrinkToFit = False
                    .ReadingOrder = xlContext
                    .MergeCells = False
            End With
        Next i

        i = 6
        WKS.cells(i, 4 + intDaysInMonth).Value = "Total"
        Do
            i = i + 1
            WKS.cells(i, 1).Value = .Fields("Timecode")
            WKS.cells(i, 2).Value = .Fields("ParentProject")
            WKS.cells(i, 3).Value = .Fields("Description")
            
            WKS.cells(i, 4 + intDaysInMonth).Select
            With XL.Selection
                XL.ActiveCell.FormulaR1C1 = "=SUM(RC[-" & intDaysInMonth & "]:RC[-1])"
            End With
            .MoveNext
        Loop Until .EOF
        
        WKS.Columns("C:C").Select
        XL.Selection.ColumnWidth = 50
        With XL.Selection
            .HorizontalAlignment = xlGeneral
            .VerticalAlignment = xlBottom
            .WrapText = True
            .Orientation = 0
            .AddIndent = False
            .IndentLevel = 0
            .ShrinkToFit = False
            .ReadingOrder = xlContext
            .MergeCells = False
        End With
    
        WKS.cells.Columns.AutoFit
        
        WKS.cells(2, 4).Value = "Workgroup:"
        WKS.cells(2, 7).Value = WG
        WKS.cells(3, 4).Value = "Grade:"
        WKS.cells(3, 7).Value = GC
        WKS.cells(2, 11).Value = "Month:"
        WKS.cells(3, 11).Value = "Period:"
    
        WKS.cells(2, 14).Value = MonthName
        WKS.cells(3, 14).Value = fMonth
        intWorkingDays = DLookup("Days", "tlkpMonthHours", "Month = " & intMonth & " AND Year = " & intYear)
        intWorkingHours = DLookup("CVLHours", "tlkpMonthHours", "Month = " & intMonth & " AND Year = " & intYear)
        WKS.cells(2, 18).Value = "There are " & intWorkingDays & " working days in " & MonthName & ". Working hours in the month: " & intWorkingHours
        WKS.cells(3, 18).Value = "NB. Time is to be recorded in hours to the nearest half hour."
    End With
    
    WKS.Rows("1:3").Select
    With XL.Selection
        .HorizontalAlignment = xlGeneral
        .VerticalAlignment = xlBottom
        .WrapText = False
        .Orientation = 0
        .AddIndent = False
        .IndentLevel = 0
        .ShrinkToFit = False
        .ReadingOrder = xlContext
        .MergeCells = False
    End With
    
    WKS.cells(1, 1).Select
    XL.Calculation = xlCalculationAutomatic
    XL.displayalerts = False
    intPeriod = Forms![frmCOS90]![Month]
    If Eval(XL.Version) = 11 Then ' Office 2003
        ChDir SettingsGet("Cos90Location")
        WB.SaveAs SettingsGet("Cos90Location") & "\" & WG & "_" & intPeriod & "_" & UserName & "_Cos90.xls", xlNormal, ConflictResolution:=xlLocalSessionChanges
    Else
        WB.SaveAs SettingsGet("Cos90Location") & "\" & WG & "_" & intPeriod & "_" & UserName & "_Cos90.xls", xlExcel8, ConflictResolution:=xlLocalSessionChanges
    End If
    XL.displayalerts = True
    Set WKS = Nothing
    Set WB = Nothing
    XL.Quit
    Set XL = Nothing
    Set db = Nothing
    Set rec = Nothing
    MakeExcelCos90 = True
    Exit Function
    
cos90_err:
    MsgBox Err.Number & " " & Err.Description
    MakeExcelCos90 = False
    Set WKS = Nothing
    Set WB = Nothing
    XL.Quit
    Set XL = Nothing
    Set db = Nothing
    Set rec = Nothing
    Exit Function
End Function



Private Function DaysInMonth2(d As Variant) As Variant
'
' Returns the number of days in a month
' Requires a date argument, since February can change if it's a leap year
' Lets Access figure it out
'
' ?DaysInMonth2(DateSerial(96,2,1))  returns 29

  If varType(d) <> 7 Then
    DaysInMonth2 = Null
  Else
    DaysInMonth2 = DateSerial(Year(d), Month(d) + 1, 1) - DateSerial(Year(d), Month(d), 1)
  End If
End Function