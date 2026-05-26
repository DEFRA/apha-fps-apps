MODULE NAME: basPublication
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function fnNextPublicationNumber(PN_in) As Variant
Dim PN_last
If IsNull(PN_in) Then
    fnNextPublicationNumber = Null
Else
    PN_in = UCase(PN_in)
    PN_last = DMax("Identifier", "tblPublication", "Identifier like '" & PN_in & "*'")
    If IsNull(PN_last) Then
        fnNextPublicationNumber = PN_in & "/00001"
    Else
        fnNextPublicationNumber = PN_in & "/" & Format(1 + CInt(Right(PN_last, 5)), "00000")
    End If
End If

End Function


Function fnLate(Submitted, target) As Boolean

If IsNull(target) Then
    fnLate = False
ElseIf IsNull(Submitted) Then
    If target < Date Then
        fnLate = True
    Else
        fnLate = False
    End If
Else
    If target < Submitted Then
        fnLate = True
    Else
        fnLate = False
    End If
End If

End Function

Function fnSubmittedOnTime(Submitted, target) As Boolean

If IsNull(target) Then
    fnSubmittedOnTime = False
    
ElseIf IsNull(Submitted) Then

    fnSubmittedOnTime = False

Else
    If target < Submitted Then
        fnSubmittedOnTime = False
    Else
        fnSubmittedOnTime = True
    End If
End If

End Function


Function fnLateMilestone(Submitted, target) As Boolean

If IsNull(target) Then
    fnLateMilestone = False
ElseIf IsNull(Submitted) Then
    If target < Date Then
        fnLateMilestone = True
    Else
        fnLateMilestone = False
    End If
Else
    fnLateMilestone = False

End If

End Function

Function fnNowDue(target) As Boolean
If IsNull(target) Then
    fnNowDue = False
Else
    If target <= Date Then
        fnNowDue = True
    Else
        fnNowDue = False
    End If
End If
End Function

Function fnSearchAuthor(Name As String, field As String) As Boolean
Dim seperators As String
seperators = "[ ,.-]"
If field Like Name & seperators & "*" Then
    fnSearchAuthor = True
ElseIf field Like "*" & seperators & Name & seperators & "*" Then
    fnSearchAuthor = True
Else
    fnSearchAuthor = False
End If
End Function

Function fnProgramGroup(Program)
Dim pos
Dim prefix
prefix = DLookup("PublicationPrefix", "tblRadtrackProg", "Program='" & Program & "'")

If IsNull(Program) Then
    fnProgramGroup = Null
ElseIf Not IsNull(prefix) Then
    fnProgramGroup = prefix
Else
    pos = InStr(1, Program, "_", vbTextCompare)
    If pos = 0 Then
        fnProgramGroup = Null
    Else
        fnProgramGroup = Left(Program, pos - 1)
    End If
End If
End Function


'Public Function fnPublicationProgram()
'fnPublicationProgram = [Forms]![frmPublication]![Program]
'End Function



Function fnPublicationWhere(ShowAll As Boolean) As String

Dim sqlstr As String
'sqlstr = "Select * from tblRadtrackInvoice"
sqlstr = ""
Dim seperators As String
seperators = "[ ,.:;-]"


If Not IsNull([Forms]![frmPublication]!pickProject) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    If [Forms]![frmPublication]!pickProject = "<None>" Then
         sqlstr = sqlstr & " Not Exists(select * from tblPublicationProject where tblPublicationProject.PublicationUID=UID )"
    Else
        sqlstr = sqlstr & " Exists(select * from tblPublicationProject where tblPublicationProject.PublicationUID=UID and tblPublicationProject.parentProject = '" & [Forms]![frmPublication]!pickProject & "')"
    End If
End If

If Not IsNull([Forms]![frmPublication]!pickProgramme) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " program='" & [Forms]![frmPublication]!pickProgramme & "' "
End If

If Not IsNull([Forms]![frmPublication]!pickProgramGroup) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " identifier like '" & [Forms]![frmPublication]!pickProgramGroup & "*' "
End If


If Not IsNull([Forms]![frmPublication]!pickManager) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " Exists(select * from qryPublicationProjectManagers where PublicationUID=UID and Manager LIKE '*" & [Forms]![frmPublication]!pickManager & "*')"

    'sqlstr = sqlstr & " ProjectLeader = '" & [Forms]![frmPublication]!pickManager & "'"
End If

If Not IsNull([Forms]![frmPublication]!pickLeadAuthor) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " LeadAuthor LIKE '*" & [Forms]![frmPublication]!pickLeadAuthor & "*'"
End If

If Not IsNull([Forms]![frmPublication]!pickAuthor) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    If [Forms]![frmPublication]!ogAuthorSearch = 1 Then
        sqlstr = sqlstr & " ([Leadauthor] = '" & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Leadauthor] like '" & [Forms]![frmPublication]!pickAuthor & seperators & "*'"
        sqlstr = sqlstr & " OR [Leadauthor] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Leadauthor] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & seperators & "*')"
    ElseIf [Forms]![frmPublication]!ogAuthorSearch = 2 Then
        sqlstr = sqlstr & " ([Otherauthors] = '" & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Otherauthors] like '" & [Forms]![frmPublication]!pickAuthor & seperators & "*'"
        sqlstr = sqlstr & "  OR [Otherauthors] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Otherauthors] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & seperators & "*')"
    Else
        sqlstr = sqlstr & " ([Leadauthor] = '" & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Leadauthor] like '" & [Forms]![frmPublication]!pickAuthor & seperators & "*'"
        sqlstr = sqlstr & " OR [Leadauthor] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Leadauthor] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & seperators & "*'"
        sqlstr = sqlstr & " OR [Otherauthors] = '" & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Otherauthors] like '" & [Forms]![frmPublication]!pickAuthor & seperators & "*'"
        sqlstr = sqlstr & " OR [Otherauthors] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & "'"
        sqlstr = sqlstr & " OR [Otherauthors] like '*" & seperators & [Forms]![frmPublication]!pickAuthor & seperators & "*')"
    End If
End If

If ([Forms]![frmPublication]!OpIsLate) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " IsLate = true "
End If

If ([Forms]![frmPublication]!optNotSubmitted) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " submitted IS NULL "
End If


    
If Not IsNull([Forms]![frmPublication]!PickFYear) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " fnDueYear([TargetDate])=" & [Forms]![frmPublication]!PickFYear
End If

If Not IsNull([Forms]![frmPublication]!pickIdentifier) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " [identifier]='" & [Forms]![frmPublication]!pickIdentifier & "'"
End If

If sqlstr <> "" Then
    fnPublicationWhere = " WHERE " & sqlstr
ElseIf Not ShowAll Then
    fnPublicationWhere = " WHERE 1=2"
Else
    fnPublicationWhere = ""
End If
End Function


Function fnPublicationWhereDesc() As String

Dim sqlstr As String
'sqlstr = "Select * from tblRadtrackInvoice"
sqlstr = ""
Dim seperators As String
seperators = "[ ,.:;-]"


If Not IsNull([Forms]![frmPublication]!pickProject) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    If [Forms]![frmPublication]!pickProject = "<None>" Then
        sqlstr = sqlstr & " Project IS NULL"
    Else
        sqlstr = sqlstr & " Project = '" & [Forms]![frmPublication]!pickProject & "'"
    End If
End If

If Not IsNull([Forms]![frmPublication]!pickProgramme) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " program='" & [Forms]![frmPublication]!pickProgramme & "' "
End If

If Not IsNull([Forms]![frmPublication]!pickProgramGroup) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " program like'" & [Forms]![frmPublication]!pickProgramGroup & "*' "
End If


If Not IsNull([Forms]![frmPublication]!pickManager) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " ProjectLeader = '" & [Forms]![frmPublication]!pickManager & "'"
End If

If Not IsNull([Forms]![frmPublication]!pickLeadAuthor) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " LeadAuthor = '" & [Forms]![frmPublication]!pickLeadAuthor & "'"
End If

If Not IsNull([Forms]![frmPublication]!pickAuthor) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    If [Forms]![frmPublication]!ogAuthorSearch = 1 Then
        sqlstr = sqlstr & " ([Leadauthor] contains '" & [Forms]![frmPublication]!pickAuthor & "''"

    ElseIf [Forms]![frmPublication]!ogAuthorSearch = 2 Then
        sqlstr = sqlstr & " ([Otherauthors] contains '" & [Forms]![frmPublication]!pickAuthor & "'"

    Else
        sqlstr = sqlstr & " ([Leadauthor] contains '" & [Forms]![frmPublication]!pickAuthor & "''"
        sqlstr = sqlstr & " OR ([Otherauthors] contains '" & [Forms]![frmPublication]!pickAuthor & "'"
    End If
End If

If ([Forms]![frmPublication]!OpIsLate) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " Publication is late  "
End If
    
If Not IsNull([Forms]![frmPublication]!PickFYear) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " Financial year due =" & [Forms]![frmPublication]!PickFYear & "/" & CStr([Forms]![frmPublication]!PickFYear + 1)
End If

If Not IsNull([Forms]![frmPublication]!pickIdentifier) Then
    If sqlstr <> "" Then sqlstr = sqlstr & " AND "
    sqlstr = sqlstr & " [identifier]='" & [Forms]![frmPublication]!pickIdentifier & "'"
End If
    
If sqlstr <> "" Then
    fnPublicationWhereDesc = " WHERE " & sqlstr
Else
    fnPublicationWhereDesc = ""
End If
End Function



Function fnPublicationProjectList(UID)
Dim db As Database
Dim rs As Recordset
Dim opstr As String
Dim sqlstr As String
If IsNull(UID) Then
    fnPublicationProjectList = Null
Else
    opstr = ""
    sqlstr = "Select ParentProject from tblPublicationProject where PublicationUID=" & UID
    Set db = CurrentDb
    Set rs = db.OpenRecordset(sqlstr, , dbSeeChanges)
    
    With rs
        If .BOF And .EOF Then
    
        Else
            .MoveFirst
            While Not .EOF
                If opstr <> "" Then opstr = opstr & ", "
                 opstr = opstr & .Fields("parentproject")
                 .MoveNext
            Wend
            
        End If
    End With
    fnPublicationProjectList = opstr
    
    Set rs = Nothing
    Set db = Nothing
End If
End Function

Function fnPublicationProjectLeaderList(UID)
Dim db As Database
Dim rs As Recordset
Dim opstr As String
Dim sqlstr As String
If IsNull(UID) Then
    fnPublicationProjectLeaderList = Null
Else
    opstr = ""
    sqlstr = "Select Manager from qryPublicationProjectManagers where PublicationUID=" & UID
    Set db = CurrentDb
    Set rs = db.OpenRecordset(sqlstr, , dbSeeChanges)
    
    With rs
        If .BOF And .EOF Then
    
        Else
            .MoveFirst
            While Not .EOF
                If opstr <> "" Then opstr = opstr & ", "
                 opstr = opstr & .Fields("manager")
                 .MoveNext
            Wend
            
        End If
    End With
    fnPublicationProjectLeaderList = opstr

    Set rs = Nothing
    Set db = Nothing
End If
End Function


Function fnDeletePublication(UID)
    Dim sqlstr As String
    
    If Not IsNull(UID) Then
        If MsgBox("Are you sure you want to delete this publication?", vbYesNo) = vbYes Then
            DoCmd.SetWarnings False
            sqlstr = "Delete from tblPublicationProject where PublicationUID=" & UID
            DoCmd.RunSQL (sqlstr)
            sqlstr = "Delete from tblPublication where UID=" & UID
            DoCmd.RunSQL (sqlstr)
            DoCmd.SetWarnings True
        End If
    End If
End Function
