Option Compare Database
Option Explicit

Function Startup()

    'LogEvent ("Startup")
    Dim varRequiredFrontEndVersion As Variant
    ' Check this front-end has the correct version
    varRequiredFrontEndVersion = Val(Nz(BESettingsGet("RequiredFrontEndVersion_Pact"), 0))
    If VersionNo() < varRequiredFrontEndVersion Then
        msgInfo "This program needs to be upgraded before it can be used." & vbNewLine & vbNewLine & _
                "An automatic upgrade is available." & vbNewLine & _
                "Please click OK and wait for the 'Installation Completed' message, then try again."
        RunVBS BESettingsGet("UpgradeFile_Pact")
        Application.Quit
    End If
    SetAccessCaption SettingsGet("SystemName")
    'DoCmd.DoMenuItem 1, 4, 3       'hide database container
    DoCmd.OpenForm "Menu"
End Function


Function CurrentYear() As String
    CurrentYear = Right(DLookup("DB_Var_Value", "tblDB_Variables", "DB_Var_Name='DB_Name'"), 4)
End Function