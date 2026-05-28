Option Compare Database
Option Explicit






Function fnYearMonth(fy As Integer, fm As Integer) As Long

    fnYearMonth = (CLng(fy) * 100) + fm
End Function

Function fnLatestYearReleased() As Integer

    fnLatestYearReleased = DMax("[year]", "MAB_tlkpYear", "latestmonthreleased IS NOT NULL")

End Function

Function fnLatestMonthReleased() As Integer

    fnLatestMonthReleased = DLookup("[latestmonthreleased]", "MAB_tlkpYear", "[Year]=" & fnLatestYearReleased())
End Function
Function fnLatestYearMonthReleased()
    
    fnLatestYearMonthReleased = fnYearMonth(fnLatestYearReleased(), fnLatestMonthReleased())
End Function

Function fnOneYearBackFromMonthReleased()

    Dim m As Integer
    m = fnLatestMonthReleased()
    If m = 12 Then
        fnOneYearBackFromMonthReleased = fnYearMonth(fnLatestYearReleased(), 1)
    Else
        fnOneYearBackFromMonthReleased = fnYearMonth(fnLatestYearReleased() - 1, m + 1)
    End If
    
End Function


Function fnFMonthToMonth(m As Integer) As Integer

    Dim x
    x = (m + 3 + 12) Mod 12
    If x = 0 Then x = 12
    fnFMonthToMonth = x
    
End Function

Function fnDisplayTextMonth(fm As Integer)

    fnDisplayTextMonth = Format("01/" & fnFMonthToMonth(fm) & "/00", "mmmm")

End Function


Function fnTxtYearCovered() As String

    Dim m As Integer
    Dim y As Integer
    Dim sy As Integer
    Dim ey As Integer
    
    
    m = fnLatestMonthReleased()
    y = fnLatestYearReleased()
    If m = 9 Then
        sy = y
        ey = y
    ElseIf m >= 10 Then
        sy = y
        ey = y + 1
    Else
        sy = y - 1
        ey = y
    End If
    
    fnTxtYearCovered = " from " & fnDisplayTextMonth(m + 1) & " " & sy & " to " & fnDisplayTextMonth(m) & " " & ey

End Function



Function fnPickYearSelected() As String
    Dim Inlist As String
    Dim intCR As Integer
    Inlist = ""
    
    For intCR = 0 To Forms![frmProgrammeSummary]![PickYear].ListCount - 1
        If Forms![frmProgrammeSummary]![PickYear].Selected(intCR) Then
            If Inlist <> "" Then Inlist = Inlist & ", "
            Inlist = Inlist & CInt(Forms![frmProgrammeSummary]![PickYear].Column(0, intCR))
        End If
    Next
    fnPickYearSelected = Inlist
End Function