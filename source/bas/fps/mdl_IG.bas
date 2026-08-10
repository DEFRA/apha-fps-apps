Option Compare Database   'Use database order for string comparisons

         Global Const VERB_OPEN = -2
         Global Const OLE_ACTIVATE = 7
         Global Const OLE_EMBEDDED = 1
         Global Const OLE_LINKED = 0
         Global Const OLE_CREATE_EMBED = 0
         Global Const OLE_CREATE_LINK = 1

            Global Const OLE_INSERT_OBJ_DLG = 14

Function convertnulldouble(x As Variant) As Double
convertnulldouble = IIf(IsNull(x), 0, x)

End Function

Function IsSurv(WG As String) As String
       If (Left(WG, 2) = "SV") Or WG = "pa5" Then
           IsSurv = "Surv"
       Else
           IsSurv = "Non-Surv"
       End If

End Function

Function WGOptBanner(Opt As Variant) As String
Select Case Opt
    Case 1
        WGOptBanner = "Total "
    Case 2
        WGOptBanner = "Assured  "
    Case 3
        WGOptBanner = "Not Assured "
     End Select


End Function

Function ZT_Split(Project As String) As String
Select Case Project
    Case "ZTLeave"
        ZT_Split = "ZZTLeave"
    Case "ZTWork"
        ZT_Split = "ZTWork"
    Case Else
        ZT_Split = "Chargeable"
    End Select

End Function