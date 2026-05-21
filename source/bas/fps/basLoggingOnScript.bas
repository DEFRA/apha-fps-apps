Option Compare Database   'Use database order for string comparisons
Option Explicit

Const bufsize = 255
Const lpDefault = ""

Dim MyDB As Database, MyOBJDB As Database, MyWorkSpace As Workspace, rst As Recordset, rstOBJ As Recordset, rstDATA As Recordset
Dim rstDATAErrSugg As Recordset, rstOBJDATAErrSugg As Recordset

Dim lpReturnVAl As String * bufsize, strDATAMDB As String, strRstCriteria As String
Dim lpFileName As String

Dim varOBJDATAErrSugg, varDATAErrSugg

Dim GPPSReturnVal As Integer, intOnOff As Integer, intRstCriteria As Integer

Function ldoChkErrorMsgTbl() As Integer

    'NB - NOT USED AS CANT IMPORT A TABLE THROUGH DAO, BRILLIANT...NOT!
    On Error GoTo ldoChkErrorMsgTbl_Err
    
    Set MyOBJDB = CurrentDb()

    'Open tblUtility to see if Error Forwarding is on/off
    lpFileName = SysCmd(SYSCMD_INIFILE)
    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, lpFileName)
    If GPPSReturnVal <= 0 Then Exit Function
    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
    
    'Get the LastUpdated property for the FPS-DATA.MDB tblErrMsgSuggestions.
    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
    Set rstDATAErrSugg = MyDB.OpenRecordset("tblErrMsgSuggestions", DB_OPEN_TABLE)
    varDATAErrSugg = rstDATAErrSugg.LastUpdated
    Debug.Print varDATAErrSugg
    
    'Get the LastUpdated property for the FPS-OBJ.MDB tblErrMsgSuggestions.
    Set rstOBJDATAErrSugg = MyOBJDB.OpenRecordset("tblErrMsgSuggestions", DB_OPEN_TABLE)
    varOBJDATAErrSugg = rstOBJDATAErrSugg.LastUpdated
    Debug.Print varOBJDATAErrSugg

    'Compare the two
    If varDATAErrSugg = varOBJDATAErrSugg Then
        ldoChkErrorMsgTbl = True
        GoTo ldoChkErrorMsgTbl_Done
    Else
        ldoChkErrorMsgTbl = False
    End If

ldoChkErrorMsgTbl_Done:
Exit Function

ldoChkErrorMsgTbl_Err:

    ldoError Err, Error
    ldoChkErrorMsgTbl = True
    GoTo ldoChkErrorMsgTbl_Done

End Function

Function ldoChkWarningOnOff(strCurrentUser As String) As String

On Error GoTo ldoChkWarningOnOff_Err

    'Open tblUtility to see if Error Forwarding is on/off
    lpFileName = SysCmd(SYSCMD_INIFILE)
    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, lpFileName)
    If GPPSReturnVal <= 0 Then Exit Function
    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
    
    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
    
    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
    Set rst = MyDB.OpenRecordset("qrytblUsers", DB_OPEN_SNAPSHOT)
    strRstCriteria = "UserName = " & "'" & strCurrentUser & "'" & ""
    rst.FindFirst strRstCriteria
    intOnOff = rst!frmWarning
    Debug.Print intOnOff
    If intOnOff = -1 Then ldoChkWarningOnOff = "On" Else ldoChkWarningOnOff = "Off"
    rst.Close

ldoChkWarningOnOff_Done:
Exit Function

ldoChkWarningOnOff_Err:

    ldoError Err, Error
    GoTo ldoChkWarningOnOff_Done

End Function
'
'Function ldoInstallYesNo() As Integer
'
'    On Error GoTo ldoInstallYesNo_Err
'
'    Dim strOBJVersion As String, strDATAOBJVersion As String
'    Dim Response As Integer
'
'    'Initialise variables
'    strRstCriteria = ""
'    strDATAOBJVersion = ""
'    strOBJVersion = ""
'    strDATAMDB = ""
'
'    'Open tblUtility to see if has the latest FPS-OBJ.MDB
'    lpFileName = SysCmd(SYSCMD_INIFILE)
'    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, lpFileName)
'    If GPPSReturnVal <= 0 Then Exit Function
'    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
'
'    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
'
'    'Get FPS-OBJ.MDB version number held in FPS-DATA.MDB
'    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
'    Set rstDATA = MyDB.OpenRecordset("qryUtility", DB_OPEN_DYNASET)
'    strRstCriteria = "Utility = " & "'" & "OBJVersion" & "'" & ""
'    rstDATA.FindFirst strRstCriteria
'    strDATAOBJVersion = rstDATA!Value
'    rstDATA.Close
'
'    'Get FPS-OBJ.MDB version number held in FPS-OBJ.MDB
'    Set MyOBJDB = MyWorkSpace.OpenDatabase("C:\FPS\FPS-OBJ.MDB")
'    Set rstOBJ = MyOBJDB.OpenRecordset("tblVersion", DB_OPEN_DYNASET)
'    rstOBJ.MoveFirst
'    strOBJVersion = rstOBJ.VersionNumber
'    rstOBJ.Close
'
'    'Compare the two.
'    If strOBJVersion = strDATAOBJVersion Then
'        ldoInstallYesNo = False
'        GoTo ldoInstallYesNo_Done
'    Else
'        Response = MsgBox("The version of FPS you are using is not up to date. Would you like to install the latest version now ? ", MB_YESNO + MB_ICONQUESTION, "Application Information")
'        Debug.Print Response
'        If Response = 6 Then
'            ldoInstallYesNo = True
'        End If
'    End If
'
'ldoInstallYesNo_Done:
'Exit Function
'
'ldoInstallYesNo_Err:
'
'    ldoError Err, Error
'    Resume ldoInstallYesNo_Done
'
'End Function

Function ldoIsMember(ByVal pstrUser As String, ByVal pstrGroup As String) As Integer

On Error GoTo ldoIsMemberErr

    Dim MyDB As Database
    Dim wrk As Workspace
    Dim usr As User
    Dim gru As Group
    Dim strMsg As String
    Dim intErrHndlrFlag As Integer
    Dim varGroupName As Variant
    Dim strProcName As String
    
    Const MB_OK = 0

    Set MyDB = CurrentDb()
    
    strProcName = "ldoIsMember"

    'Intialize return value
    ldoIsMember = False

    Set wrk = DBEngine.Workspaces(0)

    'Refresh users and groups collections
    wrk.Users.Refresh
    wrk.Groups.Refresh

    Set usr = wrk.Users(pstrUser)
    Set gru = wrk.Groups(pstrGroup)
    varGroupName = usr.Groups(pstrGroup).Name
    
    If Not IsEmpty(varGroupName) Then
        ldoIsMember = True
        GoTo ldoIsMemberDone
        Else: GoTo ldoisnotmember
    End If

ldoIsMemberDone:
    
    'If glrUserWithBlankPW(CurrentUser()) = True Then
    '    DoCmd OpenForm "frmPassword"
    'End If

    'ldoUserLogging "Developer Logon to FPS"

Exit Function

ldoisnotmember:
    
    DoCmd.Hourglass True
    Application.MenuBar = "mmnuDatabase"
    DoCmd.DoMenuItem 1, 4, 3
    DoCmd.Hourglass False

    ''Check to see if using latest FPS-OBJ.MDB
    'If ldoInstallYesNo() = True Then
    '    MsgBox "Click OK to exit FPS then run the 'setup.exe' file in 'g:\fpslive\instal31'", MB_OK + MB_ICONEXCLAMATION, "Application Information"
    '    Application.Quit
    'End If

    DoCmd.Hourglass True
    'DoCmd OpenForm "frmSplash"
    Application.SetOption "Built-In Toolbars Available", False
    DoCmd.OpenForm "frmmnuMain"
    
    ''Check to see if still want to display "live" warning screen
    'If ldoChkWarningOnOff(CurrentUser()) = "On" Then
    '    DoCmd OpenForm "frmWarning"
    'End If
    
    DoCmd.Hourglass False
    
    ''Check to see if user has a blank password and force them to create one.
    'If glrUserWithBlankPW(CurrentUser()) = True Then
    '    DoCmd OpenForm "frmPassword"
    '    Forms!frmSplash.visible = False
    '    Forms!frmmnuMain.visible = False
    '    Dim intfrmWarning As Integer
    '    intfrmWarning = SysCmd(SYSCMD_GETOBJECTSTATE, A_FORM, "frmWarning")
    '    If intfrmWarning <> 0 Then
    '        Forms!frmWarning.visible = False
    '    End If
    'End If
    
    'DoCmd Hourglass True
    
    ''Write log record for successfull logon.
    'ldoUserLogging "User Logon to FPS"
    'DoCmd Hourglass False

Exit Function
ldoIsMemberErr:
    
    Select Case Err
    Case 3265
        
        GoTo ldoisnotmember
    
    Case Else
    MsgBox "The application encountered unexpected error #" & Err & " with message string '" & Error & " in procedure Sub Form_Open'", MB_OK, "Please inform ITU - from ldoChkUsrIsDvlpr!"
    End Select
    Application.Quit
    
End Function