Option Compare Database   'Use database order for string comparisons
Option Explicit

Global strGlobalPC As String
Global strGlobalWG As String

Sub ldoAllPCNonCSGProjectSummaries()

    Dim MyDB As Database
    Dim rst As Recordset

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("SELECT DISTINCTROW tblkpProfitCentre.ProfitCentre FROM tblkpProfitCentre WHERE ((tblkpProfitCentre.Division='R&D')) OR ((tblkpProfitCentre.Division='Ops'))ORDER BY tblkpProfitCentre.ProfitCentre;", DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        Debug.Print rst!ProfitCentre
        DoCmd.OpenReport "rptUtilityProject", , , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        rst.MoveNext
        DoEvents
    Loop

    rst.Close

End Sub

Function ldoPCSummary()

    Dim MyDB As Database
    Dim rst As Recordset

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("tblkpProfitCentre", DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        strGlobalPC = rst!ProfitCentre
        DoCmd.OpenReport "rptProfitCentreSummaryBatch", A_NORMAL, , "tblkpProfitCentre.ProfitCentre =" & Chr(34) & strGlobalPC & Chr(34)
        rst.MoveNext
    Loop
    
    rst.Close

End Function

Sub ldoPCWGSummaries()
    
    Dim MyDB As Database
    Dim rst As Recordset

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("SELECT DISTINCTROW tblkpProfitCentre.ProfitCentre FROM tblkpProfitCentre ORDER BY tblkpProfitCentre.ProfitCentre;", DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        Debug.Print rst!ProfitCentre
        strGlobalPC = rst!ProfitCentre
        DoCmd.OpenReport "rptProfitCentreSummaryBatch", A_NORMAL, , "tblkpProfitCentre.ProfitCentre =" & Chr(34) & strGlobalPC & Chr(34)
        ldoWGSummary (rst!ProfitCentre)
        rst.MoveNext
    Loop
    
    rst.Close


End Sub

Sub ldoProfitCentreSetShort()

    Dim MyDB As Database
    Dim rst As Recordset

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("SELECT DISTINCTROW tblkpProfitCentre.ProfitCentre FROM tblkpProfitCentre ORDER BY tblkpProfitCentre.ProfitCentre;", DB_OPEN_SNAPSHOT)

    Do Until rst.EOF

        DoCmd.OpenReport "rptProfitCentreSummary", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "SaleOfChargedTimeCluedo", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "rptCSGsummary-PC", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "rptPortfolioSumPC", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "rptNON-CSGsummary-PC", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "rptUtilisation", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "rptUtilisationPC", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoCmd.OpenReport "saleOfChargedTime", , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        DoEvents
        rst.MoveNext
    Loop

    rst.Close
End Sub

Sub ldoRDOpsPCWGSummaries()

    Dim MyDB As Database
    Dim rst As Recordset
    Dim strSQL As String

    strSQL = "SELECT DISTINCTROW tlkpDivision.DivName, tblkpProfitCentre.ProfitCentre "
    strSQL = strSQL & "FROM tlkpDivision INNER JOIN tblkpProfitCentre ON tlkpDivision.DivName = tblkpProfitCentre.Division "
    strSQL = strSQL & "WHERE ((tlkpDivision.DivName = 'R&D')) Or ((tlkpDivision.DivName = 'Ops')) "
    strSQL = strSQL & "ORDER BY tlkpDivision.DivName, tblkpProfitCentre.ProfitCentre;"


    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset(strSQL, DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        strGlobalPC = rst!ProfitCentre
        DoCmd.OpenReport "rptProfitCentreSummaryBatch", A_NORMAL, , "tblkpProfitCentre.ProfitCentre =" & Chr(34) & strGlobalPC & Chr(34)
        ldoWGSummary (rst!ProfitCentre)
        rst.MoveNext
    Loop
    
    rst.Close


End Sub

Sub ldoTestContracts()

    Dim MyDB As Database
    Dim rst As Recordset
    Dim i As Integer
    Dim strSQL As String

    strSQL = "SELECT DISTINCTROW tlkpProject.ParentProject FROM tlkpProject "
    strSQL = strSQL & "WHERE ((tlkpProject.PlanCat = 'TestContract')) "
    strSQL = strSQL & "ORDER BY tlkpProject.ParentProject;"
    

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset(strSQL, DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        DoCmd.OpenReport "rptAHVGcontracts", A_NORMAL, , "Buyer =" & Chr(34) & rst!ParentProject & Chr(34)
    Loop

    rst.Close

End Sub

Sub ldoVIDOutputPC()
    
    Dim MyDB As Database
    Dim rst As Recordset

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("qryVID-PC", DB_OPEN_SNAPSHOT)

    Do Until rst.EOF

        Debug.Print rst!ProfitCentre
        DoCmd.OpenReport "rptVIDOutput-PC", , , "ProfitCentre =" & Chr(34) & rst!ProfitCentre & Chr(34)
        rst.MoveNext
    Loop

    rst.Close

End Sub

Sub ldoVIDOutputWG()

    Dim MyDB As Database
    Dim rst As Recordset

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("qryVID-WG", DB_OPEN_SNAPSHOT)

    Do Until rst.EOF

        Debug.Print rst!workgroup
        DoCmd.OpenReport "rptVIDOutput-WG", , , "WorkGroup =" & Chr(34) & rst!workgroup & Chr(34)
        rst.MoveNext
        DoEvents
    Loop

    rst.Close

End Sub

Sub ldoWGSummary(strPC As String)
    
    Dim MyDB As Database
    Dim rst As Recordset
    Dim strSQL As String

    strSQL = strSQL & "SELECT DISTINCTROW WorkGroup.WorkGroup "
    strSQL = strSQL & "FROM tblkpProfitCentre INNER JOIN WorkGroup ON tblkpProfitCentre.ProfitCentre = WorkGroup.ProfitCentre "
    strSQL = strSQL & "WHERE ((tblkpProfitCentre.ProfitCentre = " & Chr(34) & strPC & Chr(34) & "))"
    strSQL = strSQL & "ORDER BY WorkGroup.WorkGroup "
    strSQL = strSQL & "WITH OWNERACCESS OPTION;"

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset(strSQL, DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        Debug.Print rst!workgroup
        strGlobalWG = rst!workgroup
        DoCmd.OpenReport "rptWorkGroupSummaryBatch", A_NORMAL, , "WorkGroup =" & Chr(34) & rst!workgroup & Chr(34)
        rst.MoveNext
        DoEvents
    Loop
    
    rst.Close

End Sub

Sub ldoWGSummaryForPC()
    
    Dim MyDB As Database
    Dim rst As Recordset
    Dim strSQL As String

    strSQL = strSQL & "SELECT DISTINCTROW WorkGroup.WorkGroup "
    strSQL = strSQL & "FROM tblkpProfitCentre INNER JOIN WorkGroup ON tblkpProfitCentre.ProfitCentre = WorkGroup.ProfitCentre "
    strSQL = strSQL & "WHERE ((tblkpProfitCentre.ProfitCentre = " & Chr(34) & [Forms]![menu-division]![PCpick] & Chr(34) & "))"
    strSQL = strSQL & "ORDER BY WorkGroup.WorkGroup "
    strSQL = strSQL & "WITH OWNERACCESS OPTION;"

    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset(strSQL, DB_OPEN_SNAPSHOT)

    Do Until rst.EOF
        strGlobalWG = rst!workgroup
        Debug.Print strGlobalWG
        DoCmd.OpenReport "rptWorkGroupSummaryBatch", A_NORMAL, , "WorkGroup =" & Chr(34) & strGlobalWG & Chr(34)
        rst.MoveNext
        DoEvents
    Loop
    
    rst.Close

End Sub