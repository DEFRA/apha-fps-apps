Option Compare Database
Option Explicit

Public Function fnSelectedProg(Prog As String) As Boolean
Dim sp1 As Boolean
Dim sp2 As Boolean

If Forms!menuPivot.ogIncludeZT = 1 Then 'Include ZT?
    sp1 = True
ElseIf Prog Like "ZT*" Then
    sp1 = False
Else
    sp1 = True
End If

If Forms!menuPivot.ogShowMe = 1 Then 'All
    sp2 = True
ElseIf Nz(Forms!menuPivot.pickProgram, "") = "" Then
    sp2 = True
ElseIf Forms!menuPivot.ogShowMe = 2 Then 'Limit to
    If Prog = Forms!menuPivot.pickProgram Then
        sp2 = True
    Else
        sp2 = False
    End If
ElseIf Forms!menuPivot.ogShowMe = 3 Then 'Exclude
    If Prog = Forms!menuPivot.pickProgram Then
        sp2 = False
    Else
        sp2 = True
    End If
End If

If sp1 And sp2 Then
    fnSelectedProg = True
Else
    fnSelectedProg = False
End If

End Function

Public Function fnSelectedProject(Project As String) As Boolean
If Nz(Forms!menuPivot.PickProject, "") = "" Then
    fnSelectedProject = True

ElseIf Forms!menuPivot.ogShowMe = 1 Then 'All
    fnSelectedProject = True
ElseIf Forms!menuPivot.ogShowMe = 2 Then 'Limit
    If Project = Forms!menuPivot.PickProject Then
        fnSelectedProject = True
    Else
        fnSelectedProject = False
    End If
    
ElseIf Forms!menuPivot.ogShowMe = 3 Then 'Exclude
    If Project = Forms!menuPivot.PickProject Then
        fnSelectedProject = False
    Else
        fnSelectedProject = True
    End If
End If

End Function