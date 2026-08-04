Option Compare Database
Option Explicit

Function fnFindQuery(qtext As String)

    Dim MyDB As Database
    Dim qd As QueryDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.QueryDefs.Count - 1
        Set qd = MyDB.QueryDefs(i)
        If qd.sql Like "*" & qtext & "*" Then
           
            Debug.Print qd.Name; "  "; Chr(13) & Chr(10)
        Else
          'Debug.Print ""
        End If
        
    Next i
    

End Function

Function fnReplaceTxt(qtext As String, oldT As String, NewT As String) As String

Dim oldTLen As Integer
Dim newTlen As Integer
Dim i As Integer

oldTLen = Len(oldT)
newTlen = Len(NewT)
i = InStr(1, qtext, oldT, vbTextCompare)
If i > 0 Then
    fnReplaceTxt = Left(qtext, i - 1) & NewT & fnReplaceTxt(Mid(qtext, oldTLen + i), oldT, NewT)
Else
    fnReplaceTxt = qtext
End If

End Function
Function fnFindAndReplaceQuery(oldT As String, NewT As String)
On Error GoTo F_Err

    Dim MyDB As Database
    Dim qd As QueryDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.QueryDefs.Count - 1
        Set qd = MyDB.QueryDefs(i)
        If qd.sql Like "*" & oldT & "*" Then
           qd.sql = fnReplaceTxt(qd.sql, oldT, NewT)
            Debug.Print qd.Name; "  "; Chr(13) & Chr(10)
        Else
          'Debug.Print ""
        End If
        
    Next i
    
F_Err:
If Err.Number <> 0 Then
    Debug.Print "Error with : "
    Resume Next
End If

End Function