Option Compare Database
Option Explicit

Public Function fnIsErrorToZero(n) As Double
On Error Resume Next
    If Not IsNumeric(n) Then
        fnIsErrorToZero = 0
    Else
        fnIsErrorToZero = n
    End If
    

End Function