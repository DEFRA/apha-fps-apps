MODULE NAME: Module1
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Sub IG()


Dim db As Database
Dim pt As Recordset
Dim sqlstr As String
Dim inflatedCost As Double

Set db = CurrentDb
sqlstr = "Select * from tblstaffRequ where year>= 2010 and (payrate is null or payrate =0)"
Set pt = db.OpenRecordset(sqlstr, dbOpenDynaset, dbSeeChanges)

With pt
    If Not (.EOF And .BOF) Then
        .MoveFirst
        While Not .EOF
            .Edit
            '.Fields("chargerate") = Nz(DLookup("[chargerate]", "qryWGRates", "WGGrade='" & .Fields("WGGrade") & "'"), 0) * fnInflation("InflationStaff", Project, .Fields("Year"))
            .Fields("PayRate") = DLookup("[payrate]", "qryWGRates", "WGGrade='" & .Fields("WGGrade") & "'") * fnInflation("InflationStaff", .Fields("Project"), .Fields("Year"))
            .Fields("NPR") = DLookup("[NPR]", "qryWGRates", "WGGrade='" & .Fields("WGGrade") & "'") * fnInflation("InflationStaff", .Fields("Project"), .Fields("Year"))
            .Fields("OHR") = DLookup("[OHR]", "qryWGRates", "WGGrade='" & .Fields("WGGrade") & "'") * fnInflation("InflationStaff", .Fields("Project"), .Fields("Year"))
            .Update
            .MoveNext
        Wend
        
    End If
End With
pt.Close
End Sub


Function fnNewWGGrade(wg As String) As String

    wg = Replace(wg, "Sci", "")
    wg = Replace(wg, "Vet", "")
    wg = Replace(wg, "Adm", "")
    
    wg = Replace(wg, "A_VI5", "A_VI1")
    wg = Replace(wg, "C_CD3", "C_CD1")
    wg = Replace(wg, "G_SEB5", "G_BAC4")
    
    wg = Replace(wg, "FES1", "BAC1")
    wg = Replace(wg, "SEB1", "BAC1")
    wg = Replace(wg, "FES7", "BAC2")
    wg = Replace(wg, "SEB3", "BAC3")
    wg = Replace(wg, "SEB4", "BAC4")
    wg = Replace(wg, "SEB5", "BAC5")
    
    wg = Replace(wg, "TMB1", "MPG1")
    wg = Replace(wg, "TMB2", "MPG2")
    wg = Replace(wg, "TMB4", "MPG4")
    wg = Replace(wg, "TMB5", "MPG3")
    wg = Replace(wg, "FES6", "LT6")
    wg = Replace(wg, "-", "_")
    
  
    fnNewWGGrade = wg
End Function


Function Replace(ByVal Valuein As String, ByVal WhatToReplace As _
                 String, ByVal Replacevalue As String) As String
   Dim Temp As String, p As Long
   Temp = Valuein
   p = InStr(Temp, WhatToReplace)
   Do While p > 0
      Temp = Left(Temp, p - 1) & Replacevalue & _
          Mid(Temp, p + Len(WhatToReplace))
      p = InStr(p + Len(Replacevalue), Temp, WhatToReplace, 1)
   Loop
   Replace = Temp
End Function
        
