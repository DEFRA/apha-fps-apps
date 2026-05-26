MODULE NAME: _nameSplit
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function fnNoChar(str, char) As Integer
    Dim i As Integer
    Dim p As Integer

    i = 0
    For p = 1 To Len(str)
        If Mid(str, p, 1) = char Then i = i + 1
    Next
    fnNoChar = i
End Function

    

Function fnFirstName(str)
    Dim p As Integer
    If IsNull(str) Then
        fnFirstName = Null
    Else
        p = InStr(1, str, ",")
        If p > 0 Then
            fnFirstName = Trim(Mid(str, p + 1))
        ElseIf fnNoChar(str, " ") = 1 Then
            p = InStr(1, str, " ")
            fnFirstName = Trim(Left(str, p - 1))
        End If
    End If
End Function


Function fnLastName(str)
    Dim p As Integer
    If IsNull(str) Then
        fnLastName = Null
    Else
        p = InStr(1, str, ",")
        If p > 0 Then
            fnLastName = Trim(Left(str, p - 1))
        ElseIf fnNoChar(str, " ") = 1 Then
            p = InStr(1, str, " ")
            fnLastName = Mid(str, p + 1)
        Else
            fnLastName = str
        End If
    End If
End Function
