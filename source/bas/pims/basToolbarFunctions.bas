MODULE NAME: basToolbarFunctions
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit
Function fnSaveSnapshot()
Dim r As Report
Set r = Screen.ActiveReport


If IsNull(r) Then
Else
    DoCmd.OutputTo acReport, r.Name, "SnapshotFormat(*.snp)"
End If
End Function

Function fnPrintDialog()
DoCmd.RunCommand acCmdPrint

End Function
