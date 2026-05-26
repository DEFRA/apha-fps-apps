Option Compare Database   'Use database order for string comparisons
Option Explicit

'Declare Constants
Const bufsize = 255
Const lpDefault = ""
'Object declarations
Dim MyDB As Database, MyWorkSpace As Workspace, rst As Recordset
'String decalarations
Dim strEmailAddress As String, strIniFile As String, strMDAFile As String, strFullRun As String
Dim FormNames As String, ReportNames As String, strMessage As String, AccessVer As String
Dim strActiveObject As String, strDATAMDB As String, strOnOff As String, lpReturnVAl As String * bufsize
Dim strRstCriteria As String, lpFileName As String, strFPSVer As String
'Integer declarations
Dim x As Integer, strRunTime As Integer, numberForms As Integer, NumberReports As Integer
Dim GPPSReturnVal As Integer

Sub IGChangeControl(ctr As Control)

ctr.SetFocus
If (ctr.Text = "#Error") Then
     ctr.ControlSource = "0"
     
End If

End Sub

'Function ldoBuildErrorMessage(ErrNumber As Integer, ErrorMessage As String) As String
'
'    On Error GoTo ldoBuildErrorMessage_Err
'
'    'This section generates information for the email error message
'    numberForms = Forms.Count
'    FormNames = "There are " & numberForms & " open forms:" & Chr(10)
'    For x = 0 To numberForms - 1
'        FormNames = FormNames & Forms(x).Name & Chr(10)
'    Next x
'
'    NumberReports = Reports.Count
'    ReportNames = "There are " & NumberReports & " open reports:" & Chr(10)
'    For x = 0 To NumberReports - 1
'        ReportNames = ReportNames & Reports(x).Name & Chr(10)
'    Next x
'
'    strActiveObject = Application.CurrentObjectName
'    AccessVer = SysCmd(SYSCMD_ACCESSVER)
'    strIniFile = SysCmd(SYSCMD_INIFILE)
'    strMDAFile = ldoGetMDAFile("Options", "SystemDB")
'    strFPSVer = ldoGetFPSVer()
'    strRunTime = SysCmd(SYSCMD_RUNTIME)
'    If strRunTime = 0 Then strFullRun = "Full" Else strFullRun = "Runtime"
'
'    'Building the email message
'    strMessage = "Current FPS User is: " & CurrentUser() & Chr(10)
'    strMessage = strMessage & Chr(10) & "Date & time error occurred: " & Format$(Now, "dd/mm/yy hh:nn") & Chr(10)
'    strMessage = strMessage & Chr(10) & "Error number and message is: " & ErrNumber & " - " & ErrorMessage & Chr(10)
'    strMessage = strMessage & Chr(10) & "The active object was: " & strActiveObject & Chr(10)
'    strMessage = strMessage & Chr(10) & FormNames & ReportNames
'    strMessage = strMessage & Chr(10) & "The version of FPS is: " & strFPSVer
'    strMessage = strMessage & Chr(10) & "Version of Access they are using: " & AccessVer
'    strMessage = strMessage & Chr(10) & "Type of Access running the application: " & strFullRun
'    strMessage = strMessage & Chr(10) & ".ini file in use: " & strIniFile
'    strMessage = strMessage & Chr(10) & ".mda file in use: " & strMDAFile
'
'    ldoBuildErrorMessage = strMessage
'
'ldoBuildErrorMessage_Done:
'Exit Function
'
'ldoBuildErrorMessage_Err:
'
'    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoBuildErrorMessage', please inform the IT Helpdesk!"
'    GoTo ldoBuildErrorMessage_Done
'
'End Function

'Function ldoCheckErrorForwardingOnOff() As Integer
'
'    On Error GoTo ldoCheckErrorForwardingOnOff_Err
'
'    'Open tblUtility to see if Error Forwarding is on/off
'    lpFileName = SysCmd(SYSCMD_INIFILE)
'    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, lpFileName)
'    If GPPSReturnVal <= 0 Then Exit Function
'    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
'
'    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
'    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
'    Set rst = MyDB.OpenRecordset("qryUtility", DB_OPEN_SNAPSHOT)
'
'    strRstCriteria = "Utility = 'ldoError'"
'    rst.FindFirst strRstCriteria
'    strOnOff = rst.Value
'    If strOnOff = "Off" Then ldoCheckErrorForwardingOnOff = False Else ldoCheckErrorForwardingOnOff = True
'
'
'ldoCheckErrorForwardingOnOff_Done:
'rst.Close
'Exit Function
'
'ldoCheckErrorForwardingOnOff_Err:
'    MsgBox (Error & " " & Err)
'    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoCheckErrorForwardingOnOff', please inform the IT Helpdesk!"
'    GoTo ldoCheckErrorForwardingOnOff_Done
'
'End Function

Sub ldoEmailErrorMessage(strMessage As String)
    
    On Error GoTo ldoEmailErrorMessage_Err

    'Send the email
    strEmailAddress = "FPS Administrators"
    DoCmd.SendObject , , A_FORMATTXT, strEmailAddress, , , "FPS Error Message", strMessage, 0


ldoEmailErrorMessage_Done:
Exit Sub

ldoEmailErrorMessage_Err:

    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoEmailErrorMessage', please inform the IT Helpdesk!"
    GoTo ldoEmailErrorMessage_Done

End Sub
'
'Sub ldoEnterErrorIntblErrorLog(ErrNumber As Integer, ErrorMessage As String)
'
'    On Error GoTo ldoEnterErrorIntblErrorLog_Err
'
'    'This section generates information for the email error message
'    numberForms = Forms.Count
'    FormNames = "There are " & numberForms & " open forms:"
'    For x = 0 To numberForms - 1
'        FormNames = FormNames & Forms(x).Name & " "
'    Next x
'
'    NumberReports = Reports.Count
'    ReportNames = "There are " & NumberReports & " open reports:"
'    For x = 0 To NumberReports - 1
'        ReportNames = ReportNames & Reports(x).Name & " "
'    Next x
'
'    strActiveObject = Application.CurrentObjectName
'    AccessVer = SysCmd(SYSCMD_ACCESSVER)
'    strIniFile = SysCmd(SYSCMD_INIFILE)
'    strMDAFile = ldoGetMDAFile("Options", "SystemDB")
'    strFPSVer = ldoGetFPSVer()
'    strRunTime = SysCmd(SYSCMD_RUNTIME)
'    If strRunTime = 0 Then strFullRun = "Full" Else strFullRun = "Runtime"
'
'    'Build the path for FPS-DATA.MDB from the FPS.MDA path in the FPS.INI File
'    lpFileName = SysCmd(SYSCMD_INIFILE)
'    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, lpFileName)
'    If GPPSReturnVal <= 0 Then Exit Sub
'    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
'
'    'Write the new record to the tblErrorLog in FPS-DATA.MDB
'    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
'    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
'    Set rst = MyDB.OpenRecordset("qryErrorLog", DB_OPEN_DYNASET)
'    rst.AddNew
'    rst.CurrentUser = CurrentUser()
'    rst.DateAndTime = Format$(Now, "dd/mm/yy hh:nn")
'    rst.ErrorNo = ErrNumber
'    rst.ErrorDescription = ErrorMessage
'    rst.ActiveObject = strActiveObject
'    rst.FormsOpen = FormNames
'    rst.ReportsOpen = ReportNames
'    rst.FPSVersion = strFPSVer
'    rst.AccessVersion = AccessVer
'    rst.AccessType = strFullRun
'    rst.iniFilePath = strIniFile
'    rst.mdaFilePath = strMDAFile
'    rst.Update
'    rst.Close
'
'ldoEnterErrorIntblErrorLog_Done:
'Exit Sub
'
'ldoEnterErrorIntblErrorLog_Err:
'
'    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoEnterErrorIntblErrorLog', please inform the IT Helpdesk!"
'    'Debug.Print Err & " - " & Error
'    GoTo ldoEnterErrorIntblErrorLog_Done
'
'End Sub

Sub ldoError(ErrNumber As Integer, ErrorMessage As String)
On Error GoTo ldoError_Err

MsgBox ("The following error has occured; " & ErrNumber & " " & ErrorMessage)


ldoError_Done:
Exit Sub

ldoError_Err:

    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoError', please inform the IT Helpdesk!"
    GoTo ldoError_Done

End Sub

Sub ldoErrorMessageForm(ErrNumber As Integer, ErrorMessage As String)

    On Error GoTo ldoErrorMessageForm_err

    DoCmd.OpenForm "frmErrorMessage", A_NORMAL
    Forms!frmErrorMessage!ErrorNumber = ErrNumber
    Forms!frmErrorMessage!ErrorMessage = ErrorMessage


ldoErrorMessageForm_Done:
Exit Sub

ldoErrorMessageForm_err:

    ldoError Err, Error
    GoTo ldoErrorMessageForm_Done

End Sub
'
'Function ldoGetFPSVer() As String
'
'    On Error GoTo ldoGetFPSVer_Err
'
'    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
'    Set MyDB = CurrentDb()
'    Set rst = MyDB.OpenRecordset("tblVersion", DB_OPEN_TABLE)
'    rst.MoveFirst
'    strFPSVer = rst.VersionNumber
'    rst.Close
'
'    ldoGetFPSVer = strFPSVer
'
'ldoGetFPSVer_Done:
'Exit Function
'
'ldoGetFPSVer_Err:
'
'    ldoError Err, Error
'    ldoGetFPSVer = "Cant report version number"
'    GoTo ldoGetFPSVer_Done
'
'End Function

Function ldoGetMDAFile(lpSection As String, lpEntry As String) As String
    
    On Error GoTo ldoGetMDAFile_Err

    Const bufsize = 255
    Dim lpDefault As String, lpFileName As String, GotInfo As String
    Dim lpReturnVAl As String * 255, intTest As Integer
    
    lpDefault = "No Value"
    lpFileName = SysCmd(SYSCMD_INIFILE)
    intTest = GetPrivateProfileString(lpSection, ByVal lpEntry, lpDefault, lpReturnVAl, bufsize, lpFileName)
    ldoGetMDAFile = lpReturnVAl

ldoGetMDAFile_Done:
Exit Function

ldoGetMDAFile_Err:

    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoGetMDAFile', please inform the IT Helpdesk!"
    ldoGetMDAFile = lpDefault
    GoTo ldoGetMDAFile_Done

End Function

Sub xldoError(ErrNumber As Integer, ErrorMessage As String)

    On Error GoTo xldoError_Err

    '1. Show the error on the user's screen no matter what.
    ldoErrorMessageForm ErrNumber, ErrorMessage

    '2. Dont do for Developer's errors.
    If glrIsMember(CurrentUser(), "Developer") = True Then Exit Sub
    
    '3. Enter the error details in tblErrorLog on the server
    'ldoEnterErrorIntblErrorLog ErrNumber, ErrorMessage
    
xldoError_Done:
Exit Sub

xldoError_Err:

    MsgBox "An error occurred in the Error Message Forwarding routine 'ldoError', please inform the IT Helpdesk!"
    GoTo xldoError_Done

End Sub