Option Compare Database   'Use database order for string comparisons
Option Explicit
'Global Const MB_OK = 0
'Global Const MB_ICONEXCLAMATION = 0
'Global Const MB_DEFBUTTON1 = 0
'Global Const MB_APPLMODAL = 0

'Constant for frmJobCodePlan-Own and frmBid setting of default value
Global strDVParentForm As String

'Sub EnumerateDocuments()
'On Error GoTo EnumerateDocuments_Err
'    Dim DefaultWorkspace As Workspace
'    Dim CurrentDatabase As Database
'    Dim MyContainer As Container, MyDocument As Document
'    Dim i As Integer, j As Integer
'
'    Set DefaultWorkspace = DBEngine.Workspaces(0)
'    Set CurrentDatabase = DefaultWorkspace.Databases(0)
'    Set MyContainer = CurrentDatabase.Containers(4)
'
'    For i = 0 To MyContainer.Documents.Count - 1
'        Set MyDocument = MyContainer.Documents(i)
'            Debug.Print MyDocument.Name
'        DoCmd.OpenReport MyDocument.Name, A_DESIGN
'    Next i
'
'EnumerateDocuments_Err:
'
'    ldoError3 Err, Error
'    Resume Next
'
'End Sub

Function IsLoaded(MyFormName)
    
    '  Determines if a form is loaded.
    
    Const FORM_DESIGN = 0
    Dim i As Integer
    
    IsLoaded = False
    For i = 0 To Forms.Count - 1
        If Forms(i).FormName = MyFormName Then
            If Forms(i).CurrentView <> FORM_DESIGN Then
                IsLoaded = True
                Exit Function  ' Quit function once form has been found.
            End If
        End If
    Next

End Function

Function isOpen(strName As String, intObjectType As Integer)

    isOpen = (SysCmd(SYSCMD_GETOBJECTSTATE, intObjectType, strName) <> 0)
    
End Function

Sub ldoArray()

    Dim strArray() As String
    Dim i As Integer

    For i = 0 To 9
        ReDim Preserve strArray(2, i)
        strArray(1, i) = CStr(i) & "JobCode"
        'Debug.Print strArray(1, i)
        strArray(2, i) = CStr(i) & "TestCode"
        'Debug.Print strArray(2, i)
    Next i

    For i = 0 To 9
        Debug.Print i; " JobCode: "; strArray(1, i); " TestCode: "; strArray(2, i)
    Next i

    
End Sub

Sub ldoChangeQuerySQL()

'WITH OWNERACCESS OPTION

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim i As Integer

    Set MyDB = CurrentDb()
    
    For i = 0 To MyDB.QueryDefs.Count - 1
          Set MyQuery = MyDB.QueryDefs(i)
          If InStr(1, MyQuery.sql, "WITH OWNERACCESS OPTION") Then
            MyQuery.sql = ldoReplaceCharacter(MyQuery.sql, "WITH OWNERACCESS OPTION", "")
          End If
    Next i

End Sub

Sub ldoContibutionReport()

    Dim i As Integer, strPC As String
    Dim MyDB As Database, rst As Recordset
    
    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset("qrytblkpProfitCentre")
    rst.MoveFirst
    Do Until rst.EOF
        If rst!ProfitCentre = "????" Then
            GoTo NextRecord
        End If
        strPC = "'" & rst!ProfitCentre & "'"
        Debug.Print strPC
        DoCmd.OpenReport "rptContribution-PC-ldo", A_NORMAL, , "BuyingPC =" & strPC
NextRecord:
        rst.MoveNext
    Loop
    rst.Close
End Sub

Function ldoDATAMDBPath(lpIniName As String, lpDbName As String) As String

    'lpIniName      The name of the ini file
    'lpDbName       Name of back-end database

    Const bufsize = 255
    Const lpDefault = ""
    Dim lpSection As String, lpEntry As String
    Dim dbCurrent As Database
    Dim GPPSReturnVal As Integer
    Dim lpReturnVAl As String * bufsize
   
    
    On Error GoTo ldoDATAMDBPath_Err

    lpSection = "Options"
    lpEntry = "SystemDB"
    
    ldoDATAMDBPath = ""

    GPPSReturnVal = GetPrivateProfileString(lpSection, ByVal lpEntry, lpDefault, lpReturnVAl, bufsize, lpIniName)
    If GPPSReturnVal <= 0 Then Exit Function

    lpDbName = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & lpDbName

    ldoDATAMDBPath = lpDbName

Exit Function
ldoDATAMDBPath_Err:

    MsgBox Error$
    
End Function

'Sub ldoError3(ErrNumber As Integer, ErrorMessage As String)
'
'    MsgBox "The FPS application encountered unexpected error number: " & ErrNumber & ", with message description: '" & ErrorMessage & "'", MB_OK Or MB_ICONEXCLAMATION Or MB_DEFBUTTON1 Or MB_APPLMODAL, "Please inform ITU !"
'
'    ldoSendErrorMail ErrNumber, ErrorMessage
'
'End Sub

Sub ldoGetJetVers()
Dim DBVersion As String, Version As String, Release As String
Dim DotLocation As Integer

DBVersion = DBEngine.Version
DotLocation = InStr(DBVersion, ".")
Version = Left$(DBVersion, DotLocation - 1)
Release = Right$(DBVersion, Len(DBVersion) - DotLocation)

Debug.Print Version; Release; DBVersion

End Sub

Function ldoMsgBox(Title As String, msg As String) As Integer

On Error GoTo ldoMsgBox_Err

    Const MB_OK = 0, MB_OKCANCEL = 1    ' Define buttons.
    Const MB_YESNOCANCEL = 3, MB_YESNO = 4
    Const MB_ICONSTOP = 16, MB_ICONQUESTION = 32   ' Define icons.
    Const MB_ICONEXCLAMATION = 48, MB_ICONINFORMATION = 64
    Const MB_DEFBUTTON2 = 256, IDYES = 6, IDNO = 7   ' Define other.
    
    Dim DgDef
    Dim Response

    'Title = "MsgBox Demo"
    ' Put together a sample message box with all the proper components.
    'Msg = "This is a sample of a critical-error message."
    'Msg = Msg & " Do you want to continue?"
    DgDef = MB_YESNO + MB_ICONSTOP + MB_DEFBUTTON2   ' Describe dialog box.
    Response = MsgBox(msg, DgDef, Title)    ' Get user response.
    If Response = IDYES Then    ' Evaluate response
        ldoMsgBox = True
    Else
        ldoMsgBox = False
    End If

ldoMsgBox_Done:
Exit Function

ldoMsgBox_Err:

    ldoError Err, Error
    GoTo ldoMsgBox_Done

End Function

Sub ldoQryToXL(qryName As String)
    
    Const MB_OK = 0

    On Error GoTo ldoQryToXLErr

    'Dim intTaskID As Integer
    
    DoCmd.TransferSpreadsheet A_EXPORT, 5, qryName, "c:;data;excel;Qrypivot.xls", True

    ' Start up Excel
    'intTaskID = Shell("c:;msoffice;excel;excel.exe c:;data;excel;qrypivot.xls", 1)
    
    DoCmd.RunMacro "mcrRunXL"

Exit Sub
ldoQryToXLErr:

    MsgBox "The application encountered unexpected error #" & Err & " with message string '" & Error & "'", MB_OK, "Please inform ITU !"
    
End Sub

'Sub ldoSendErrorMail(ErrNumber As Integer, ErrorMessage As String)
'
'On Error GoTo ldoSendErrorMail_Err
'
'    'Dont do for Developer's errors.
'    If glrIsMember(CurrentUser(), "Developer") = True Then Exit Sub
'
'    'Declare Constants
'    Const bufsize = 255
'    Const lpDefault = ""
'    'Object declarations
'    Dim MyDB As Database, MyWorkSpace As Workspace, rst As Recordset
'    'String decalarations
'    Dim strEmailAddress As String, strIniFile As String, strMDAFile As String, strFullRun As String
'    Dim FormNames As String, ReportNames As String, strMessage As String, AccessVer As String
'    Dim strActiveObject As String, strDATAMDB As String, strOnOff As String, lpReturnVAl As String * bufsize
'    Dim strRstCriteria As String
'    'Integer declarations
'    Dim x As Integer, strRunTime As Integer, numberForms As Integer, NumberReports As Integer
'    Dim GPPSReturnVal As Integer
'
'    'Open tblUtility to see if Error Forwarding is on/off
'    GPPSReturnVal = GetPrivateProfileString("Options", ByVal "SystemDB", lpDefault, lpReturnVAl, bufsize, "FPS.INI")
'    If GPPSReturnVal <= 0 Then Exit Sub
'    strDATAMDB = Left(lpReturnVAl, GPPSReturnVal - (Len("FPS.MDA"))) & "FPS-DATA.MDB"
'    Set MyWorkSpace = DBEngine.CreateWorkspace("Special", "Microsoft", "Trimix")
'    Set MyDB = MyWorkSpace.OpenDatabase(strDATAMDB)
'    Set rst = MyDB.OpenRecordset("qryUtility", DB_OPEN_SNAPSHOT)
'    strRstCriteria = "Utility = 'ldoError'"
'    rst.FindFirst strRstCriteria
'    strOnOff = rst.Value
'    If strOnOff = "Off" Then Exit Sub
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
'    strMDAFile = ldoGetMDA("Options", "SystemDB")
'    strRunTime = SysCmd(SYSCMD_RUNTIME)
'    If strRunTime = 0 Then strFullRun = "Full" Else strFullRun = "Runtime"
'
'    'Building the email message
'    strMessage = "Current FPS User is: " & CurrentUser() & Chr(10)
'    strMessage = strMessage & Chr(10) & "Date & time error occurred: " & Format$(Now, "dd/mm/yy hh:nn") & Chr(10)
'    strMessage = strMessage & Chr(10) & "Error number and message is: " & ErrNumber & " - " & ErrorMessage & Chr(10)
'    strMessage = strMessage & Chr(10) & "The active object was: " & strActiveObject & Chr(10)
'    strMessage = strMessage & Chr(10) & FormNames & ReportNames
'    strMessage = strMessage & Chr(10) & "Version of Access they are using: " & AccessVer
'    strMessage = strMessage & Chr(10) & "Type of Access running the application: " & strFullRun
'    strMessage = strMessage & Chr(10) & ".ini file in use: " & strIniFile
'    strMessage = strMessage & Chr(10) & ".mda file in use: " & strMDAFile
'
'    'Send the email
'    strEmailAddress = "FPS Administrators"
'    DoCmd.SendObject , , A_FORMATTXT, strEmailAddress, , , "FPS Error Message", strMessage, 0
'
'Exit Sub
'ldoSendErrorMail_Done:
'Exit Sub
'
'ldoSendErrorMail_Err:
'
'    'And if it all goes horribly wrong...
'    MsgBox "An error occurred in the Error Message Forwarding routine, please inform the IT Helpdesk!"
'    Debug.Print Err & " - " & Error
'    GoTo ldoSendErrorMail_Done
'
'End Sub

Sub ldoSyncForm()

    'MsgBox "ldoSyncForm"
    'Exit Sub

    Dim strSQL As String

    'Build the SQL string to insert the records into the local table when the form is loaded.
    strSQL = "INSERT INTO tblJobTestCosts_local ( JobCode, TestCode, ItemDescription, "
    strSQL = strSQL & "NoTests, TestPrice, UnitPriceVLA, ContrVolume ) "
    strSQL = strSQL & "SELECT DISTINCTROW qryJobTestCost.JobCode, qryJobTestCost.TestCode, "
    strSQL = strSQL & "qryJobTestCost.ItemDescription , qryJobTestCost.NoTests, "
    strSQL = strSQL & "qryJobTestCost.TestPrice , qryJobTestCost.UnitPriceVLA, "
    strSQL = strSQL & "qryJobTestCost.ContrVolume FROM qryJobTestCost "
    strSQL = strSQL & "WHERE ((qryJobTestCost.JobCode=" & Chr(34) & Forms![frmProgramTestPlan]![projectpick] & Chr(34) & "));"
    

    ' Using the SQL string to copy records to local table for inserting, editing & deleting
    DoCmd.SetWarnings False
    DoCmd.RunSQL "DELETE FROM tblJobTestCosts_local;"
    DoCmd.RunSQL strSQL
    DoCmd.SetWarnings True
    
    'Forms![frmProgramTestPlan]![qryJobStaffCost].Form.Requery
    
    'Me.Requery

End Sub

Function NextpactID() As String

Dim strSQL As String
Dim rst As Recordset
Dim MyDB As Database
Dim i As Integer
Dim bGotNew As Boolean
Dim varX As Variant

i = 1000
bGotNew = False
While Not bGotNew

    varX = DLookup("[PACTid]", "tblWGEmployee", "[PACTid] = '" & i & "'")
    If IsNull(varX) Then
        NextpactID = i
        bGotNew = True
    Else
        i = i + 1
    End If
Wend

End Function

Function IsAdminUser(t As String) As Boolean
    Dim c As String

    c = "DT2Number='" & Username() & "' and [" & t & "] = 1"

    If DCount("*", "tblAdminUsers", c) = 1 Then
        IsAdminUser = True
    Else
        IsAdminUser = False
    End If
End Function