MODULE NAME: basMilestoneFormReceived
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit
Function fnIsReceived(Project As String) As Boolean
Dim IsR As Variant
Dim M As String
Dim y As Integer
M = Forms![frmMenuReports]![cbMonth].Column(1)
y = CInt(Forms![frmMenuReports]![txtYear])
IsR = DLookup(M, "MY_MilestoneFormDates", "Year=" & y & " And ParentProject='" & Project & "'")

fnIsReceived = Not IsNull(IsR)
End Function
