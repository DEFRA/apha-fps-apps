MODULE NAME: basUserName
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Declare PtrSafe Function GetUserName Lib "advapi32.dll" Alias "GetUserNameA" (ByVal lpBuffer As String, nSize As Long) As Long

Function fnGetUserName() As String

Dim zName As String * 128
Dim zLength As Long
Dim tmp As Long

zLength = 128

tmp = GetUserName(zName, zLength)

fnGetUserName = Left(zName, zLength - 1)

End Function

Sub subDisplayUserName()

MsgBox Trim(fnGetUserName())
MsgBox Len(Trim(fnGetUserName()))

End Sub

Function fnUserDisplayName(n)
    Dim ln
    If IsNull(n) Then
        fnUserDisplayName = Null
    Else
        ln = DLookup("Name", "tblCapsStaff", "mnumber='" & n & "'")
        If IsNull(ln) Then
            fnUserDisplayName = "Name not found"
        Else
            fnUserDisplayName = ln
        End If
    End If
    
End Function
