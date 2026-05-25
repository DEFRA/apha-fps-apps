MODULE NAME: basMilestoneNumbers
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function fnLatestMilestoneNumber(p, y) As String
    Dim yr2d As String
    yr2d = Right(CStr(y), 2)
    fnLatestMilestoneNumber = Nz(DMax("[Number]", "tblMilestone", "Project='" & p & "' And left([number],2)=" & yr2d))
End Function

Function fnLatestTempMilestoneNumber(p, y) As String
    Dim yr2d As String
    yr2d = Right(CStr(y), 2)
    fnLatestTempMilestoneNumber = Nz(DMax("[Number]", "tempMilestone", " left([number],2)=" & yr2d))
End Function

Function fnNextMilestoneNumber(p, y)
    Dim yr2d As String
    Dim LMN, LTMN
    LTMN = fnLatestTempMilestoneNumber(p, y)
    LMN = fnLatestMilestoneNumber(p, y)
    yr2d = Right(CStr(y), 2)
    
  
        If LMN = "" And LTMN = "" Then
            fnNextMilestoneNumber = yr2d & "/01"
        ElseIf LMN >= LTMN Then
            fnNextMilestoneNumber = yr2d & "/" & Format(CInt(Right(LMN, 2)) + 1, "00")
        Else
            fnNextMilestoneNumber = yr2d & "/" & Format(CInt(Right(LTMN, 2)) + 1, "00")
        End If

End Function



Public Function fnMilestoneSearch()
On Error GoTo fnMilestoneSearch_err

    If IsNull(Forms![frmLOGMilestone]![txtNo1]) And IsNull(Forms![frmLOGMilestone]!txtNo2) Then
        fnMilestoneSearch = "*"
    Else
        fnMilestoneSearch = Nz(Forms![frmLOGMilestone]!txtNo1, "*") & "/" & Nz(Forms![frmLOGMilestone]!txtNo2, "*")
    End If
Exit Function
fnMilestoneSearch_err:
    fnMilestoneSearch = "*"
End Function
