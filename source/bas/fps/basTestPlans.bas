Option Compare Database
Option Explicit

Function fnWGPlanLevel(WG)

If WG Like "lt*" Then
    fnWGPlanLevel = 3
ElseIf WG Like "sv*" Then
    fnWGPlanLevel = 2
Else
    fnWGPlanLevel = 1
End If

End Function

Function fnWGPlan(WG)
If WG Like "lt*" Then
    fnWGPlan = "LTM"
ElseIf WG Like "sv*" Then
    fnWGPlan = "SVXX"
Else
    fnWGPlan = WG
End If

End Function
Function SetVar(varName As String, varValue As String)
Dim sqlstr As String
DoCmd.SetWarnings False
sqlstr = "Delete from tblDB_Variables WHERE DB_Var_Name = '" & varName & "'"
DoCmd.RunSQL sqlstr

sqlstr = "Insert into tblDB_Variables (DB_Var_Name, DB_Var_Value) VALUES('" & varName & "', '" & varValue & "')"
DoCmd.RunSQL sqlstr
SetVar = True
DoCmd.SetWarnings True
End Function

Function GetVar(varName As String) As Variant
Dim blnInitialised As Boolean
Dim RS As Recordset

If Not blnInitialised Then
    blnInitialised = True
    Set RS = CurrentDb.OpenRecordset("tblDB_Variables", dbOpenDynaset)
End If

RS.FindFirst "DB_Var_Name='" & varName & "'"
If RS.NoMatch Then
    GetVar = Null
Else
    GetVar = RS!Db_var_Value
End If

End Function