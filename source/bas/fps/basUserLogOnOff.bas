'Option Compare Database   'Use database order for string comparisons
'Option Explicit
'
''Declare Constants
'Const bufsize = 255
'Const lpDefault = ""
''Object declarations
'Dim MyDB As Database, MyWorkSpace As Workspace, rst As Recordset
''String decalarations
'Dim strEmailAddress As String, strIniFile As String, strMDAFile As String, strFullRun As String
'Dim FormNames As String, ReportNames As String, strMessage As String, AccessVer As String
'Dim strActiveObject As String, strDATAMDB As String, strOnOff As String, lpReturnVAl As String * bufsize
'Dim strRstCriteria As String
''Integer declarations
'Dim x As Integer, strRunTime As Integer, numberForms As Integer, NumberReports As Integer
'Dim GPPSReturnVal As Integer
'Dim rstVersion As Recordset
'Dim MyDBFrontEnd As Database

'Sub ldoUserLogging(strEvent As String)
'
'On Error GoTo ldoUserLogging_Err
'
'    AccessVer = SysCmd(SYSCMD_ACCESSVER)
'    strIniFile = SysCmd(SYSCMD_INIFILE)
'    'strMDAFile = ldoGetMDAFile("Options", "SystemDB")
'    strRunTime = SysCmd(SYSCMD_RUNTIME)
'    If strRunTime = 0 Then strFullRun = "Full" Else strFullRun = "Runtime"
'
'    'Build the path for FPS-DATA.MDB from the FPS.MDA path in the FPS.INI File
'    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, strIniFile)
'    If GPPSReturnVal <= 0 Then Exit Sub
'    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
'
'    'Write the users details to the tblUserLogon/Off in FPS-DATA.MDB
'    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
'    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
'    Set MyDBFrontEnd = MyWorkSpace.OpenDatabase("C:\FPS\FPS-OBJ.MDB")
'    Set rst = MyDB.OpenRecordset("qryUserLogon/Off", DB_OPEN_DYNASET)
'    Set rstVersion = MyDBFrontEnd.OpenRecordset("tblVersion", DB_OPEN_TABLE)
'    rstVersion.MoveFirst
'    rst.AddNew
'    rst.Username = CurrentUser()
'    rst.Date = Format$(Now, "dd/mm/yy")
'    rst.Time = Format$(Now, "hh:nn")
'    'rst.MachineID = Find API call
'    rst.Event = strEvent
'    rst.FPSVersion = rstVersion.VersionNumber
'    rst.AccessVersion = AccessVer
'    rst.AccessType = strFullRun
'    rst.iniFilePath = strIniFile
'    rst.mdaFilePath = lpReturnVAl
'    rst.Update
'
'    rst.Close
'    rstVersion.Close
'
'ldoUserLogging_Done:
'
'Exit Sub
'
'ldoUserLogging_Err:
'
'    ldoError Err, Error
'    GoTo ldoUserLogging_Done
'
'End Sub