Option Compare Database   'Use database order for string comparisons
Option Explicit

Global IsReal  As String
'Const CurrentJetVer = 3.51
Const CurrentJetVer = 15#


Sub closedown()
DoCmd.ShowToolbar "Form View", A_TOOLBAR_WHERE_APPROP
DoCmd.ShowToolbar "Query Datasheet", A_TOOLBAR_WHERE_APPROP
DoCmd.ShowToolbar "Print Preview", A_TOOLBAR_WHERE_APPROP
DoCmd.ShowToolbar "Database", A_TOOLBAR_WHERE_APPROP
DoCmd.ShowToolbar "FPStoolbar", A_TOOLBAR_NO
'CloseDown = True

End Sub

Function RealOrDummy()
Dim RS As Recordset
Dim db As Database
Dim ws As Workspace
Set ws = DBEngine.Workspaces(0)
Set db = ws.Databases(0)
Dim yr As Integer

Set RS = db.OpenRecordset("Select (tlkpVersion.IsLive) From tlkpVersion", DB_OPEN_SNAPSHOT)
'Debug.Print rs.islive
'rs.MoveFirst
If RS!IsLive = 0 Then
    RealOrDummy = "dUMMY"
Else
    Set RS = db.OpenRecordset("Select Db_var_Value From tblDB_Variables where db_var_name='DB_Name'", DB_OPEN_SNAPSHOT)
    yr = Right(RS!Db_var_Value, 4)
    RealOrDummy = "FPS " & yr & " - " & yr + 1
    'RealOrDummy = "FPS 2011 - 2012"
End If

End Function

Function returnisreal()
returnisreal = IsReal
End Function

Function startup() As Integer
On Error GoTo startup_err

    'LogEvent ("Startup")
     startup = False
     If Not FVerifyJetVersion() Then
        MsgBox ("Warning! Your machine is not using the correct JET version. Call the Helpdesk!")
        Application.Quit
    End If
    
    Dim varRequiredFrontEndVersion2013 As Variant
    ' Check this front-end has the correct version
    varRequiredFrontEndVersion2013 = Val(Nz(SettingsGet_BE("RequiredFrontEndVersion_FPS_2013"), 0))
    If VersionNo() < varRequiredFrontEndVersion2013 Then
        msgInfo "This program needs to be upgraded before it can be used." & vbNewLine & vbNewLine & _
                "An automatic upgrade is available." & vbNewLine & _
                "Please click OK and wait for the 'Installation Completed' message, then try again."
        RunVBS SettingsGet_BE("UpgradeFile_FPS2013")
        Application.Quit
    End If

    ' Secure the program
    
    ChangeProperty "AllowBreakIntoCode", dbBoolean, False
    
    If IsAdminUser("SeeDBWindow") Then
        ChangeProperty "AllowShortcutMenus", dbBoolean, True
        ChangeProperty "AllowSpecialKeys", dbBoolean, True
        ChangeProperty "StartUpShowDBWindow", dbBoolean, True
        ChangeProperty "AllowBuiltInToolbars", dbBoolean, True
        ChangeProperty "AllowFullMenus", dbBoolean, True
    Else
        ChangeProperty "AllowShortcutMenus", dbBoolean, False
        ChangeProperty "AllowSpecialKeys", dbBoolean, False
        ChangeProperty "StartUpShowDBWindow", dbBoolean, False
        ChangeProperty "AllowBuiltInToolbars", dbBoolean, False
        ChangeProperty "AllowFullMenus", dbBoolean, False
    End If
       
    ChangeProperty "AllowToolbarChanges", dbBoolean, False
    ChangeProperty "StartUpShowStatusBar", dbBoolean, True

    SetAccessCaption SettingsGet("SystemName")
    Application.MenuBar = "mmnuUsrFrm"
    
    DoCmd.ShowToolbar "FPStoolbar", A_TOOLBAR_YES

    DoCmd.OpenForm "frmMnuMain"
   
    SetAppProperty "AppIcon", DB_TEXT, "\\vlafiler1\frontends$\64Bit\" & DLookup("DB_Var_Value", "tblDB_Variables", "DB_Var_Name='DB_Name'") & ".bmp"

    'IsReal = RealOrDummy()
    startup = True
Exit Function

startup_err:
    ldoError Err, Error
    Application.Quit
End Function

Function FVerifyJetVersion() As Integer
If DBEngine.Version >= CurrentJetVer Then
    FVerifyJetVersion = True
Else
    FVerifyJetVersion = False
End If
End Function

Function CurrentYear() As String
    CurrentYear = Right(DLookup("DB_Var_Value", "tblDB_Variables", "DB_Var_Name='DB_Name'"), 4)
End Function

'Public Function LoadRibbons()
'
'    On Error GoTo Error1
'    Dim APP_RIBBON As String
'    APP_RIBBON = "Office_Ribbon"
'    Dim RS As dao.Recordset
'
'    Set RS = CurrentDb.OpenRecordset("SELECT * FROM USysRibbons")
'
'     Do Until RS.EOF
'
'         If RS("RibbonName").Value = APP_RIBBON Then
'              ' Ribbon found: Load it and exit
'            Application.LoadCustomUI APP_RIBBON, RS("RibbonXML").Value
'            Exit Do
'        End If
'
'         RS.MoveNext
'
'     Loop
'Error1_Exit:
'
'     On Error Resume Next
'     RS.Close
'     Set RS = Nothing
'     Exit Function
'
'Error1:
'
'     Select Case Err
'         Case 32609
'         ' Ribbon already loaded, do nothing and exit
'     Case Else
'         MsgBox "Error: " & Err.Number & vbCrLf & Err.Description, vbCritical, "Error", Err.HelpFile, Err.HelpContext
'     End Select
'
'     Resume Error1_Exit
'
' End Function