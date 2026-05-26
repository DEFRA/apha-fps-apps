MODULE NAME: INexactmatch
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function fnInexactMatch(vfind As String, vfield As String, vsource As String) As Variant

Dim db As Database
Dim rs As Recordset
Dim sqlstr As String
Dim l As Integer
Dim bestguess As Variant
Dim exactmatch As Variant

Dim i As Integer

If IsNull(vfind) Then
    fnInexactMatch = Null
Else
    i = 0
    l = Len(vfind)
    sqlstr = "Select " & vfield & "  from " & vsource & " where left([" & vfield & "]," & l & ")='" & Left(vfind, l) & "'"
    Set db = CurrentDb
    Set rs = db.OpenRecordset(sqlstr)
    With rs
    .MoveFirst
    exactmatch = Null
    bestguess = ""
    While Not .EOF
        i = i + 1
        If vfind = .Fields(0) Then
            exactmatch = .Fields(0)
        Else
            If Len(bestguess) < Len(.Fields(0)) Then bestguess = .Fields(0)
        End If
        .MoveNext
    Wend
    If (bestguess = "") Then
        fnInexactMatch = exactmatch
    Else
        fnInexactMatch = bestguess
    End If
    If i > 2 Then Debug.Print vfind & " " & i
    End With
    
End If

End Function

Function fnProblemEntry(e As String) As String
Dim commapos As Integer
Dim spacepos As Integer

commapos = InStr(1, e, ",", vbTextCompare)
spacepos = InStr(1, e, " ", vbTextCompare)

If commapos = 0 Then commapos = spacepos
If commapos <> 0 Then commapos = commapos - 1
fnProblemEntry = Left(e, commapos)

End Function


Function fnFirstBit(s)
Dim commapos As Integer
Dim spacepos As Integer

commapos = InStr(1, s, ",", vbTextCompare)
spacepos = InStr(1, s, " ", vbTextCompare)
If commapos = 0 Then commapos = spacepos
If commapos <> 0 Then commapos = commapos - 1
fnFirstBit = Left(s, commapos)
End Function

Function fnLastBit(s)
Dim commapos As Integer
Dim spacepos As Integer

commapos = InStr(1, s, ",", vbTextCompare)
spacepos = InStr(1, s, " ", vbTextCompare)
If commapos = 0 Then commapos = spacepos
If commapos <> 0 Then commapos = commapos + 1

If commapos = 0 Then
    fnLastBit = s
Else
    fnLastBit = Mid(s, commapos)
End If
End Function
