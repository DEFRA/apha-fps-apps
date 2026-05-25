Option Compare Database
Option Explicit

Function fnAnimalDesc(d) As String
    Dim p As Integer
    p = InStr(d, " x ")
    If p > 0 Then
        fnAnimalDesc = Left(d, p - 1)
    Else
        fnAnimalDesc = d
    End If
End Function

Function fnAnimalDays(d)
    Dim p, q As Integer
    p = InStr(d, " x ")
    q = InStr(d, "@")
    If p > 0 And q > 0 Then
        fnAnimalDays = Mid(d, p + 3, q - p - 4)
    Else
        fnAnimalDays = Null
    End If
    

End Function

Function fnDeptIncomeMonthFrom()

    If IsNull(Forms![frmDeptIncome]![pickMonthFrom]) Then
        fnDeptIncomeMonthFrom = 1
    Else
        fnDeptIncomeMonthFrom = Forms![frmDeptIncome]![pickMonthFrom]
    End If

End Function


Function fnDeptIncomeMonthTo()

    If Not IsNull(Forms![frmDeptIncome]![pickMonthTo]) Then
        fnDeptIncomeMonthTo = Forms![frmDeptIncome]![pickMonthTo]
    ElseIf Not IsNull(Forms![frmDeptIncome]![pickMonthFrom]) Then
        fnDeptIncomeMonthTo = (Forms![frmDeptIncome]![pickMonthFrom])
    Else
        fnDeptIncomeMonthTo = 12
    End If

End Function


Function fnDeptIncomeProject()
    fnDeptIncomeProject = [Forms]![frmDeptIncome]![PickProject]
End Function



Function fnAnimalRate(d)
    Dim p As Integer
    p = InStr(d, " @ ")
    If p > 0 Then
        fnAnimalRate = Mid(d, p + 3)
    Else
        fnAnimalRate = Null
    End If
End Function