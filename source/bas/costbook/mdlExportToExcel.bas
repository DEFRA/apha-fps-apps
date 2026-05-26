MODULE NAME: mdlExportToExcel
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Public Function ExportCurrentReportToExcel()
On Error GoTo Problem

If Application.CurrentObjectType = acReport Then
    DoCmd.OutputTo acOutputReport, Screen.ActiveReport.Name, _
        acFormatXLSX, Screen.ActiveReport.Name & ".xlsx", True

ElseIf Application.CurrentObjectType = acForm Then
    DoCmd.OutputTo acOutputForm, Screen.ActiveForm.Name, _
        acFormatXLSX, Screen.ActiveForm.Name & ".xlsx", True
End If

Exit Function

Problem:

    Dim errorDesc, errNo
    errorDesc = Err.Description
    errNo = Err.Number

    MsgBox "Error encountered while exporting." & vbCrLf & vbCrLf & _
        "Error No: " & errNo & vbCrLf & vbCrLf & _
        "  Error Desc: " & errorDesc, , "There Has Been An ERROR!"
End Function
