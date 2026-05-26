MODULE NAME: basStartup
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

Function StartUp()

    Dim varRequiredFrontEndVersion As Variant
    ' Check this front-end has the correct version
    'LogEvent ("Startup")
    'varRequiredFrontEndVersion = Val(Nz(SettingsGet_BE("RequiredFrontEndVersion_Costbook2013"), 0))
    'If VersionNo() < varRequiredFrontEndVersion Then
    '    msgInfo "This program needs to be upgraded before it can be used." & vbNewLine & vbNewLine & _
    '            "An automatic upgrade is available." & vbNewLine & _
    '            "Please click OK and wait for the 'Installation Completed' message, then try again."
    '    RunVBS SettingsGet_BE("UpgradeFile_Costbook2013")
    '    Application.Quit
    'End If
    ChangeProperty "AllowSpecialKeys", dbBoolean, False
    ChangeProperty "AllowBreakIntoCode", dbBoolean, False
    ChangeProperty "AllowShortcutMenus", dbBoolean, False
    ChangeProperty "StartUpShowDBWindow", dbBoolean, False
    ChangeProperty "AllowBuiltInToolbars", dbBoolean, False
    ChangeProperty "AllowToolbarChanges", dbBoolean, False
    ChangeProperty "AllowFullMenus", dbBoolean, False
    ChangeProperty "StartUpShowStatusBar", dbBoolean, True
    
    DoCmd.OpenForm "frmWelcome"
    DoEvents
    ldoAttachFPS_ODBCTables
    DoEvents
    DoCmd.ShowToolbar "FPStoolbar", A_TOOLBAR_YES
    DoCmd.OpenForm "frmProject"
    
End Function
