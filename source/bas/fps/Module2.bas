Option Compare Database
Option Explicit

Public Function OutputToExcel(IPfrm, Optional varCaption As Variant = Null)

' Outputs the active form to an Excel spreadsheet
' Formatting data can be placed in the form's TAG
' as FormatType~value pairs. Separate pairs with ~ also
' e.g. TitleRows~1:1~FitToPageWide~1
'
' Possible format settings are..
'       TitleRows       Needs a range of rosw e.g. 1:1
'       TitleColumns    Needs a range of columns e.g. "a:a"
'       FitToPageWide   Needs a number e.g. 1
'       FitToPageTall   Needs a number e.g. 1
'       Orientation     Needs L for Landscape or P for Portrait <P>
'
'
' Note: to export forms rather than datasheets set the tag of each control in the detail row to "Export"


Dim opfile As String
Dim ExcelSheet As Object, ExcelWB As Object
Dim ctrls As Variant, n As Integer, w As Integer, c As Integer, R As Integer, nCols As Integer
Dim colAr() As Variant, arFormat(), intFormatCount As Integer
Dim wShape As Variant, frm As Form, ctrl As Control, selectTagged As Boolean

Const xlTop = -4160
Const xlBottom = -4107
Const xlDown = -4121
Const xlLeft = -4131
Const xlInsideVertical = 11
Const xlSolid = 1
Const xlContinuous = 1
Const xlThin = 2
Const xlAutomatic = -4105
Const xlEdgeTop = 8
Const xlEdgeBottom = 9
Const xlEdgeRight = 10
Const xlMaximized = -4137
Const xlLandscape = 2
Const xlPortrait = 1
Const xlDownThenOver = 1

Const msoTextOrientationHorizontal = 1

On Error GoTo ErrorHandler

'Set frm = Screen.ActiveForm
If IsNull(IPfrm) Then
    Set frm = Screen.ActiveForm
Else
    Set frm = IPfrm
End If

opfile = "C:\" & frm.Caption & ".xls"
If Dir(opfile) <> "" Then Kill opfile

If frm.CurrentView = 1 Then
    'form view
    If frm.Section(acDetail).Controls.Count = 0 Then ' appears to be an access error that doesn't always return controls in section
        Set ctrls = frm.Controls
        selectTagged = True
    Else
        Set ctrls = frm.Section(acDetail).Controls
        selectTagged = False
    End If
Else
    'datasheet view
    Set ctrls = frm.Controls
    selectTagged = False
End If

' populate array with column position details
ReDim colAr(0 To ctrls.Count - 1)
c = 0
For n = 0 To ctrls.Count - 1
    If ctrls(n).ControlType <> acLabel Then
        If selectTagged = True And ctrls(n).Tag <> "Export" Then GoTo nextCtrl
        On Error Resume Next
        If Not ctrls(n).ColumnHidden = True Then
            colAr(c) = Format(ctrls(n).ColumnOrder, "0000") & Format(n, "0000")
            c = c + 1
        End If
        On Error GoTo ErrorHandler
nextCtrl:
    End If
Next
nCols = c
If nCols = 0 Then
    MsgBox "No controls selected! If not datasheet set tag of each control to 'Export'"
    Exit Function
End If
ReDim Preserve colAr(0 To nCols - 1)

' sort into column order for further processing
ArraySort colAr, "A"

' create an excel sheet
Set ExcelSheet = CreateObject("Excel.Sheet")
        

With ExcelSheet
    
    ' copy the datasheet records to the sheet
    DoCmd.RunCommand acCmdSelectAllRecords
    DoCmd.RunCommand acCmdCopy
    .activesheet.PasteSpecial Format:="text"
    
    ' deselect all records and write a tiny bit of data to clipboard to prevent "lot of data in clipboard" warning message
    Set ctrl = ctrls(Val(Right$(colAr(0), 4)))
    ctrl.SetFocus
    ctrl.SelStart = 1
    ctrl.SelLength = Len(ctrl)
    DoCmd.RunCommand acCmdCopy

    With .activesheet.columns
        .EntireColumn.AutoFit
        .VerticalAlignment = xlTop
        .HorizontalAlignment = xlLeft
        .WrapText = True
    End With
    
    ' format header rows and columns
    
    With .activesheet.Rows("1:1")
        .Interior.ColorIndex = 15
        .Interior.pattern = xlSolid
        .Orientation = 45
        .Borders(xlEdgeTop).LineStyle = xlContinuous
        .Borders(xlEdgeTop).Weight = xlThin
        .Borders(xlEdgeBottom).LineStyle = xlContinuous
        .Borders(xlEdgeBottom).Weight = xlThin
        .Borders(xlEdgeRight).LineStyle = xlContinuous
        .Borders(xlEdgeRight).Weight = xlThin
        .Borders(xlInsideVertical).LineStyle = xlContinuous
        .Borders(xlInsideVertical).Weight = xlThin
        .WrapText = True
        .RowHeight = 90
        .VerticalAlignment = xlBottom
    End With
    
    ' A bit of formatting to make it look nice
    .Application.ActiveWindow.WindowState = xlMaximized
    .activesheet.Rows("2:2").Select
    .Application.ActiveWindow.FreezePanes = True
    .activesheet.cells.EntireColumn.AutoFit
    
    SysCmd acSysCmdInitMeter, "Formatting...", ctrls.Count - 1
    For c = 1 To nCols
        With .activesheet.columns(c)
            If .ColumnWidth > 40 Then
                .ColumnWidth = 40
            End If
        End With
               
        SysCmd acSysCmdUpdateMeter, c
    Next
    SysCmd acSysCmdRemoveMeter
    
    .activesheet.columns.WrapText = True
    .activesheet.PageSetup.PrintGridlines = True
    
    
    ' Insert row at top and insert date stamp
    
    .activesheet.Rows("1:1").Insert Shift:=xlDown
    .activesheet.Range("A1") = "Report Date: " & Format(Date, "dd-mmm-yyyy")
    .activesheet.Range("A1").WrapText = False
    .activesheet.Rows("1:1").HorizontalAlignment = xlLeft
    .activesheet.Rows("1:1").VerticalAlignment = xlTop
    .activesheet.Rows("1:1").RowHeight = 21
    .activesheet.Rows("1:1").Font.Size = 12
    If Not IsNull(varCaption) Then
        .activesheet.Rows("1:1").Insert Shift:=xlDown
        .activesheet.Range("A1") = varCaption
        .activesheet.Range("A1").WrapText = False
        .activesheet.Rows("1:1").HorizontalAlignment = xlLeft
        .activesheet.Rows("1:1").VerticalAlignment = xlTop
        .activesheet.Rows("1:1").RowHeight = 21
        .activesheet.Rows("1:1").Font.Size = 16
    End If

    ' Name the sheet
    .activesheet.Name = Left$(frm.Caption, 31)
    
    ' Formatting as described by form tag
    intFormatCount = breakApart(frm.Tag, "~", arFormat())
    With .activesheet.PageSetup
        .Orientation = xlLandscape
        .Order = xlDownThenOver
        .LeftMargin = .Application.InchesToPoints(0.5)
        .RightMargin = .Application.InchesToPoints(0.5)
        .TopMargin = .Application.InchesToPoints(0.5)
        .BottomMargin = .Application.InchesToPoints(0.5)
        .HeaderMargin = .Application.InchesToPoints(0.5)
        .FooterMargin = .Application.InchesToPoints(0.5)

        For n = 1 To intFormatCount Step 2
            Select Case arFormat(n)
                Case "PrintTitleRows"
                    .PrintTitleRows = arFormat(n + 1)
                Case "PrintTitleColumns"
                    .PrintTitleColumns = arFormat(n + 1)
                Case "FitToPagesWide"
                    .FitToPagesWide = Val(arFormat(n + 1))
                    .FitToPagesTall = False
                    .zoom = False
                Case "FitToPagesTall"
                    .FitToPagesTall = Val(arFormat(n + 1))
                    .FitToPagesWide = False
                    .zoom = False
                Case "Orientation"
                    If arFormat(n + 1) = "L" Then
                        .Orientation = xlLandscape
                    Else
                        .Orientation = xlPortrait
                    End If

            End Select
        Next
    End With
    ' select just one cell so the whole sheet's not
    .activesheet.cells(1, 1).Select
    
    .SaveAs opfile
    
    .Application.Quit
    
End With

Set ExcelSheet = Nothing

Shell "excel.exe " & SQLstring(opfile), vbMaximizedFocus

Exit Function

ErrorHandler:
    Select Case Err
        Case 2046:  msgInfo "There is no data to export!": Exit Function
        Case 75:    msgInfo "Unable to write to file. Is the spreadsheet already open?": Exit Function
        Case Else:  msgSystemError Err, Error$, "OutputToExcel"
    End Select
    Stop
    Exit Function
    Resume
End Function