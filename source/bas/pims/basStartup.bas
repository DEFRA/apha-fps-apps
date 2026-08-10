MODULE NAME: basStartup
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Public Function Startup()
    Dim caption As String
    Dim rs As Recordset, varRequiredFrontEndVersion As Variant
    Dim varUserID As Variant

    'LogEvent ("Startup")
    ' Check this front-end has the correct version
    varRequiredFrontEndVersion = Val(Nz(SettingsGet_BE("RequiredFrontEndVersion_PIMS2013"), 0))
    If VersionNo() < varRequiredFrontEndVersion Then
        msgInfo "This program needs to be upgraded before it can be used." & vbNewLine & vbNewLine & _
                "An automatic upgrade is available." & vbNewLine & _
                "Please click OK and wait for the 'Installation Completed' message, then try again."
        RunVBS SettingsGet_BE("UpgradeFile_PIMS2013")
        Application.Quit
    End If
    caption = SettingsGet("SystemName") & " - Version " & SettingsGet("MajorVersionNo") & "." & CStr(DMax("Version", "tbl Versions"))
    'caption = "PIMS" & " - Version " & "2" & "." & CStr(DMax("Version", "tbl Versions"))
    
    If SettingsGet("RunMode") = "Test" Then caption = caption & " TEST System"
    SetAccessCaption caption
    
    
    ' Secure the program
    
'    If SettingsGet("RunMode") = "Live" Then
'        Dim cb As Object
'        For Each cb In CommandBars
'                DoCmd.ShowToolbar cb.Name, acToolbarNo
'        Next
'    End If
    
    ChangeProperty "AllowSpecialKeys", dbBoolean, False
    ChangeProperty "AllowBreakIntoCode", dbBoolean, False
    ChangeProperty "AllowShortcutMenus", dbBoolean, False
    ChangeProperty "StartUpShowDBWindow", dbBoolean, False
    ChangeProperty "AllowBuiltInToolbars", dbBoolean, False
    ChangeProperty "AllowToolbarChanges", dbBoolean, False
    
    ChangeProperty "AllowFullMenus", dbBoolean, False
    ChangeProperty "StartUpShowStatusBar", dbBoolean, True

    SetAppProperty "AppIcon", DB_TEXT, "\\vlafiler1\frontends$\Pims_P.ico"

End Function
