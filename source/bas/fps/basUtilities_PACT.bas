Option Compare Database   'Use database order for string comparisons
Option Explicit
'Global CONNECT_Testing As String

Global intCancel As Integer 'used for frmProcessing

' Global Variables

 '-------------------------------------------------------
 ' Global Declaration Section for CommonDialog
 '-------------------------------------------------------

   Type tagOPENFILENAME
        lStructSize As Long
        hwndOwner As Integer
        hInstance As Integer
        lpstrFilter As Long
        lpstrCustomFilter As Long
        nMaxCustFilter As Long
        nFilterIndex As Long
        lpstrFile As Long
        nMaxFile As Long
        lpstrFileTitle As Long
        nMaxFileTitle As Long
        lpstrInitialDir As Long
        lpstrTitle As Long
        flags As Long
        nFileOffset As Integer
        nFileExtension As Integer
        lpstrDefExt As Long
        lCustData As Long
        lpfnHook As Long
        lpTemplateName As Long
   End Type

   Declare PtrSafe Function GetOpenFileName% Lib "COMMDLG.DLL" (OPENFILENAME As tagOPENFILENAME)
   Declare PtrSafe Function GetSaveFileName% Lib "COMMDLG.DLL" (OPENFILENAME As tagOPENFILENAME)
   Declare PtrSafe Function lstrcpy& Lib "Kernel" (ByVal lpDestString As Any, ByVal lpSourceString As Any)

   Dim OPENFILENAME As tagOPENFILENAME

   Global Const OFN_READONLY = &H2
   Global Const OFN_OVERWRITEPROMPT = &H2
   Global Const OFN_HIDEREADONLY = &H4
   Global Const OFN_NOCHANGEDIR = &H8
   Global Const OFN_SHOWHELP = &H10
   Global Const OFN_ENABLEHOOK = &H20
   Global Const OFN_ENABLETEMPLATE = &H40
   Global Const OFN_ENABLETEMPLATEHANDLE = &H80
   Global Const OFN_NOVALIDATE = &H100
   Global Const OFN_ALLOWMULTISELECT = &H200
   Global Const OFN_EXTENSIONDIFFERENT = &H400
   Global Const OFN_PATHMUSTEXIST = &H800
   Global Const OFN_FILEMUSTEXIST = &H1000
   Global Const OFN_CREATEPROMPT = &H2000
   Global Const OFN_SHAREAWARE = &H4000
   Global Const OFN_NOREADONLYRETURN = &H8000
   Global Const OFN_NOTESTFILECREATE = &H10000

   Global Const OFN_SHAREFALLTHROUGH = 2
   Global Const OFN_SHARENOWARN = 1
   Global Const OFN_SHAREWARN = 0

Sub ldoAttachODBCTables()
    'CONNECT_Testing = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"

    Dim MyDB As Database
    Dim MyTable As TableDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.TableDefs.Count - 1
        Set MyTable = MyDB.TableDefs(i)
        If MyTable.Attributes And DB_ATTACHEDODBC Then
            If MyTable.Connect Like "*DATABASE=FPS*" Then
    
                MyTable.Connect = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"
                MyTable.RefreshLink
            ElseIf MyTable.Connect Like "*DATABASE=MAB_Archive*" Then
                MyTable.Connect = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("MAB_Archive_SQLServer") & ";DATABASE=" & SettingsGet("MAB_Archive_SQLDatabase") & ";"
                MyTable.RefreshLink
            End If

        Else
            Debug.Print "Ignored: "; MyTable.Name; " "; MyTable.SourceTableName
        End If
        
    Next i
    

End Sub

Sub attach_Passthrough_query()
   ' CONNECT_Testing = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"
    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.QueryDefs.Count - 1
        Set MyQuery = MyDB.QueryDefs(i)
        If MyQuery.Type = dbQSQLPassThrough Or MyQuery.Type = dbQSPTBulk Then
            'MyTable.Connect = " "
            MyQuery.Connect = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";"
            'MyQuery.RefreshLink
            'MyDB.TableDefs.Refresh
            'DoEvents
            Debug.Print MyQuery.Name;
        Else
            'Debug.Print "Ignored: "; MyQuery.Name;
        End If
        
    Next i
End Sub

Sub Run_All_DD_queries()

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim i As Integer
    Dim strPCHNo As String

    Set MyDB = CurrentDb()
    For i = 0 To MyDB.QueryDefs.Count - 1
        Set MyQuery = MyDB.QueryDefs(i)
        If MyQuery.Type = dbQDDL Then
            'MyTable.Connect = " "
            MyQuery.Execute
            'MyQuery.RefreshLink
            'MyDB.TableDefs.Refresh
            'DoEvents
            Debug.Print MyQuery.Name;
        Else
            'Debug.Print "Ignored: "; MyQuery.Name;
        End If
        
    Next i
End Sub


Function ldoCreateSummaries()

    DoCmd.TransferDatabase A_EXPORT, "Microsoft Access", ldoGetBackend(), A_TABLE, "ProjectMonth2", "ProjectMonth2"
    DoCmd.TransferDatabase A_EXPORT, "Microsoft Access", ldoGetBackend(), A_TABLE, "ProjectMonth3", "ProjectMonth3"
    DoCmd.TransferDatabase A_EXPORT, "Microsoft Access", ldoGetBackend(), A_TABLE, "TimeCostCalcs", "TimeCostCalcs"

    Dim MyDB As Database
    Dim MyTable As TableDef

    Set MyDB = CurrentDb()
    Set MyTable = MyDB.TableDefs("ProjectMonth2")
    MyDB.TableDefs.Delete MyTable.Name
    Set MyTable = MyDB.TableDefs("ProjectMonth3")
    MyDB.TableDefs.Delete MyTable.Name
    Set MyTable = MyDB.TableDefs("TimeCostCalcs")
    MyDB.TableDefs.Delete MyTable.Name

    DoCmd.TransferDatabase A_ATTACH, "Microsoft Access", ldoGetBackend(), A_TABLE, "ProjectMonth2", "ProjectMonth2"
    DoCmd.TransferDatabase A_ATTACH, "Microsoft Access", ldoGetBackend(), A_TABLE, "ProjectMonth3", "ProjectMonth3"
    DoCmd.TransferDatabase A_ATTACH, "Microsoft Access", ldoGetBackend(), A_TABLE, "TimeCostCalcs", "TimeCostCalcs"

End Function

Sub ldoDeleteDupTestReqs()


    Dim MyDB As Database
    Dim rstJobCode As Recordset
    Dim rstCloneProject As Recordset
    Dim strJobCode As String
    Dim i As Integer


    Set MyDB = CurrentDb()
    Set rstJobCode = MyDB.OpenRecordset("tlkpJobCode", DB_OPEN_DYNASET)

        Do Until rstJobCode.EOF
            
            'Debug.Print Right(rstJobCode!JobCode, Len(rstJobCode!JobCode))

            If Right(rstJobCode!JobCode, 1) = ")" Then
                
                Debug.Print rstJobCode!JobCode & " " & Left(rstJobCode!JobCode, (Len(rstJobCode!JobCode) - 3))
                strJobCode = Left(rstJobCode!JobCode, (Len(rstJobCode!JobCode) - 3))
                Set rstCloneProject = rstJobCode.Clone()
                rstCloneProject.FindFirst "JobCode =" & "'" & strJobCode & "'"
                
                If rstCloneProject.NoMatch Then
                    rstJobCode.Edit
                    rstJobCode!JobCode = strJobCode
                    rstJobCode.Update
                Else
                    rstJobCode.Delete
                End If
                
                rstCloneProject.Close
            
            End If
            rstJobCode.MoveNext
        Loop

    rstJobCode.Close

End Sub

Sub ldoEnumerateForms()

    Dim MyDB As Database
    Dim MyForm As String
    Dim MyContainer As Container
    Dim MyDocument As Document
    Dim i As Integer
    
   
    Set MyDB = CurrentDb()
    Set MyContainer = MyDB.Containers("Forms")

    For i = 0 To MyContainer.Documents.Count - 1
        Set MyDocument = MyContainer.Documents(i)
        If Not Left$(MyDocument.Name, 1) = "x" Then
            Debug.Print MyDocument.Name
        End If
    Next i

End Sub

Sub ldoEnumerateQueries()

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim i As Integer
    
   
    Set MyDB = CurrentDb()

    For i = 0 To MyDB.QueryDefs.Count - 1
        Set MyQuery = MyDB.QueryDefs(i)
        If MyQuery.Type = 144 Then
            Debug.Print MyQuery.Name; " "; MyQuery.ODBCTimeout; MyQuery.Type
            'MyQuery.ODBCTimeOut = 0
        End If
    Next i
End Sub

Sub ldoEnumerateQueryProperties()

    Dim MyQuery As QueryDef
    Dim MyDB As Database
    Dim i As Integer

    Set MyDB = CurrentDb()
    'Set MyQuery = MyDB.QueryDefs("qptGetMaxStaffid")
    Set MyQuery = MyDB.QueryDefs("qpttest")

    
        Debug.Print MyQuery.Type
        Debug.Print MyQuery.Connect
        Debug.Print MyQuery.Updatable

    
    

End Sub

Sub ldoEnumerateQuerySQL(strCharacters As String, strReplacement As String)

    On Error GoTo ldoEnumerateQuerySQL_Err

    Dim MyDB As Database
    Dim MyQuery As QueryDef
    Dim i As Integer

    Set MyDB = CurrentDb()

    For i = 0 To MyDB.QueryDefs.Count - 1
        Set MyQuery = MyDB.QueryDefs(i)
        Do Until InStr(1, MyQuery.sql, strCharacters) = False
            Debug.Print MyQuery.Name
            MyQuery.sql = ldoReplaceCharacter(MyQuery.sql, strCharacters, strReplacement)
        Loop
    Next i

Exit Sub
ldoEnumerateQuerySQL_Err:

    Debug.Print Err & " " & Error & " in " & MyQuery.Name
    Resume Next

End Sub

Sub ldoEnumerateReports()

    Dim MyDB As Database
    Dim MyReport As String
    Dim MyContainer As Container
    Dim MyDocument As Document
    Dim i As Integer
    
   
    Set MyDB = CurrentDb()
    Set MyContainer = MyDB.Containers("Reports")

    For i = 0 To MyContainer.Documents.Count - 1
        Set MyDocument = MyContainer.Documents(i)
        Debug.Print MyDocument.Name

    Next i

End Sub

Sub ldoEnumerateTableDefs()
    
    Dim MyDB As Database
    Dim MyTable As TableDef
    Dim i As Integer
    
   
    Set MyDB = CurrentDb()

    For i = 0 To MyDB.TableDefs.Count - 1
        MyDB.TableDefs.Refresh
        Set MyTable = MyDB.TableDefs(i)
        If MyTable.Attributes = 536870912 Then     'And Left(MyTable.Name, 3) = "dbo"
            'MyTable.Connect = ""
            'MyTable.Connect = "ODBC;DSN=PACT 16;APP=Microsoft Access;WSID=PCH00338;DATABASE=PACT"
            'MyTable.Connect = "ODBC;DSN=PACT 16;APP=Microsoft Access;WSID=;DATABASE=PACT"
            'MyTable.Name = "tblpt" & Mid$(MyTable.Name, 4, Len(MyTable.Name) - 3)
            'Debug.Print "tblpt" & Mid$(MyTable.Name, 4, Len(MyTable.Name) - 3)
            'DoEvents
            Debug.Print MyTable.Name; " "; MyTable.SourceTableName   'MyTable.Attributes; " "; Left(MyTable.Name, 3); " ";
            Debug.Print MyTable.Connect
        End If
    Next i

End Sub

Sub ldoFilltblReports()

    Dim MyDB As Database
    Dim MyReport As String
    Dim MyContainer As Container
    Dim MyDocument As Document
    Dim i As Integer
    Dim rst As Recordset
    
   
    Set MyDB = CurrentDb()
    Set MyContainer = MyDB.Containers("Reports")
    Set rst = MyDB.OpenRecordset("tblReports")

    For i = 0 To MyContainer.Documents.Count - 1
        Set MyDocument = MyContainer.Documents(i)
        Debug.Print MyDocument.Name
        rst.AddNew
        rst!Name = MyDocument.Name
        rst!SystemName = MyDocument.Name
        rst.Update

    Next i
    
    rst.Close

End Sub

Function ldoGetBackend()

    Dim mdbFrontEnd As Database
    Dim mdbBackEnd As Database
    Dim tblAny As TableDef
    Dim strConnect As String

    Set mdbFrontEnd = CurrentDb()

    Set tblAny = mdbFrontEnd.TableDefs("MonthlyTime")
    
    ldoGetBackend = Mid$(tblAny.Connect, 11, Len(tblAny.Connect) - 10)
    
End Function

Sub ldoImportNormalTS(strWorkGroup As String, strMonth As String, strFileName As String)
    
    On Error GoTo ldoImportNormalTS_Err

    BeginTrans

    Dim MyDB As Database
    Dim rstOrigin As Recordset
    Dim rstDestination As Recordset
    Dim intColumns As Integer, i As Integer, lngCounter As Long, lngRecords As Long, lngCurrentRecord As Long
    
    Set MyDB = CurrentDb()
    Set rstOrigin = MyDB.OpenRecordset(strFileName, DB_OPEN_TABLE)
    Set rstDestination = MyDB.OpenRecordset("tbltmpMonthlyTime", DB_OPEN_TABLE)

    rstOrigin.MoveLast
    rstOrigin.MoveFirst
    lngRecords = rstOrigin.RecordCount - 1
    rstOrigin.MoveNext  'Ignore column headings
    Do Until rstOrigin.EOF
            If intCancel = -1 Then
                Rollback
                GoTo ldoImportNormalTS_Tidy_Up
            End If
            lngCurrentRecord = lngCurrentRecord + 1
            Forms![frmProcessing]![Time] = "Creating record: " & lngCurrentRecord & " of " & lngRecords
            DoCmd.RepaintObject A_FORM, "frmProcessing"
            rstDestination.AddNew
            rstDestination!WorkGroup = strWorkGroup
            rstDestination!PACTStaffID = ldoReplaceCharacter(rstOrigin.Fields(1), "_", ".")
            'rstDestination!PACTid = rstOrigin.Fields(0)
            rstDestination!TimeCode = rstOrigin.Fields(2)
            rstDestination!ParentProject = rstOrigin(3)
            rstDestination!Month = strMonth
            rstDestination!Hours = rstOrigin.Fields(5)
            rstDestination.Update
        DoEvents
        rstOrigin.MoveNext
    Loop
            
    CommitTrans

ldoImportNormalTS_Tidy_Up:

    rstOrigin.Close
    rstDestination.Close
    
    DoCmd.DeleteObject A_TABLE, strFileName
    DoCmd.Close A_FORM, "frmProcessing"
    DoCmd.Close A_FORM, "frmImportDialog"

Exit Sub
ldoImportNormalTS_Err:
    
    MsgBox Err & " " & Error
    Rollback
    GoTo ldoImportNormalTS_Tidy_Up

End Sub

Sub ldoImportOutputSheet(strFileName As String, intOption As Integer)
    
    On Error GoTo ldoImportOutPutSheet_Err
    
    BeginTrans
    
    Dim MyDB As Database
    Dim rstOrigin As Recordset
    Dim rstDestination As Recordset
    Dim intColumns As Integer, i As Integer, lngCounter As Long, lngRecords As Long, lngCurrentRecord As Long
    
    Set MyDB = CurrentDb()
    Set rstOrigin = MyDB.OpenRecordset(strFileName, DB_OPEN_SNAPSHOT)
    Set rstDestination = MyDB.OpenRecordset("tbltmpMonthlyOutPut", DB_OPEN_TABLE)
    

    rstOrigin.MoveLast
    rstOrigin.MoveFirst
    lngRecords = rstOrigin.RecordCount - 1
    rstOrigin.MoveNext  'Ignore column headings

    If intOption = 1 Then    'PACT file
        Do Until rstOrigin.EOF
                If intCancel = -1 Then
                    Rollback
                    GoTo ldoImportOutputSheet_Tidy_Up
                End If
                rstDestination.AddNew
                lngCurrentRecord = lngCurrentRecord + 1
                'FORMS!FRMPROCESSING.VISIBLE = False
                Forms![frmProcessing]![Time] = "Creating record: " & lngCurrentRecord & " of " & lngRecords
                DoCmd.RepaintObject A_FORM, "frmProcessing"
                rstDestination!TestCode = rstOrigin.Fields(1)
                rstDestination!Buyer = rstOrigin.Fields(3)
                rstDestination!Month = rstOrigin.Fields(4)
                rstDestination!WorkGroup = rstOrigin.Fields(0)
                If IsNull(rstOrigin.Fields(5)) Or rstOrigin.Fields(5) = "" Then
                    rstDestination!Volume = "0"
                Else
                    rstDestination!Volume = rstOrigin.Fields(5)
                End If
                rstDestination.Update
            DoEvents
            rstOrigin.MoveNext
        Loop
    ElseIf intOption = 2 Then    'FarmFile file
        Do Until rstOrigin.EOF
                If intCancel = -1 Then
                    Rollback
                    GoTo ldoImportOutputSheet_Tidy_Up
                End If
                rstDestination.AddNew
                lngCurrentRecord = lngCurrentRecord + 1
                Forms![frmProcessing]![Time] = "Creating record: " & lngCurrentRecord & " of " & lngRecords
                DoCmd.RepaintObject A_FORM, "frmProcessing"
                rstDestination!TestCode = rstOrigin.Fields(1)
                rstDestination!Buyer = rstOrigin.Fields(2)
                rstDestination!Month = rstOrigin.Fields(3)
                rstDestination!WorkGroup = rstOrigin.Fields(0)
                If IsNull(rstOrigin.Fields(4)) Or rstOrigin.Fields(4) = "" Then
                    rstDestination!Volume = "0"
                Else
                    rstDestination!Volume = rstOrigin.Fields(4)
                End If
                rstDestination.Update
            DoEvents
            rstOrigin.MoveNext
        Loop
    End If
    CommitTrans

ldoImportOutputSheet_Tidy_Up:

    rstOrigin.Close
    rstDestination.Close
    
    DoCmd.DeleteObject A_TABLE, strFileName
    DoCmd.Close A_FORM, "frmProcessing"
    DoCmd.Close A_FORM, "frmImportDialog"

Exit Sub
ldoImportOutPutSheet_Err:
    
    MsgBox Err & " " & Error & " for " & rstOrigin.Fields(1) & " " & rstOrigin.Fields(2) & " " & rstOrigin.Fields(3) & " " & rstOrigin.Fields(0) & " " & rstOrigin.Fields(4)
    Rollback
    GoTo ldoImportOutputSheet_Tidy_Up

End Sub

Sub ldoImportOutputSS(ByVal intOption As Integer)
    
    On Error GoTo ldoImportOutputSS_Err
    Dim strTableName As String
    Dim strFileDetails As String
    Dim intPosition As Integer
    Dim intLength As Integer
    Dim i As Integer
    Dim strCharacter As String
    Dim strWG As String, strMonth As String
    Dim tdfOrigin As TableDef
    Dim tdfOriginFinal As TableDef
    Dim fldOriginFinal As Field
    Dim fldOrigin As Field
    Dim MyDB As Database
    Dim strSQL As String
    Dim strUpdateSQL As String

    'Get file for importing
    If intOption = 1 Then
        strFileDetails = OpenCommDlg("Excel")
    ElseIf intOption = 2 Then
        strFileDetails = OpenCommDlg("Text")
    End If

    If strFileDetails = "" Or IsNull(strFileDetails) Then
        DoCmd.Close A_FORM, "frmImportOutputsDialog"
        Exit Sub
    End If

    'Strip out the file name from the path details for the temporary table name
    intLength = Len(strFileDetails)
    intPosition = intLength - 4
    For i = 1 To intLength
        If Mid$(strFileDetails, intPosition, 1) = "\" Then
            strTableName = Mid$(strFileDetails, intPosition + 1, (intLength - 4) - intPosition)
            GoTo Import_OutPuts_File
        End If
            intPosition = intPosition - 1
    Next i
    

Import_OutPuts_File:
    If intOption = 1 Then   'PACT output file
        DoCmd.OpenForm "frmProcessing"
        Forms![frmProcessing]![Records] = "Importing " & strTableName & ".xls" & " into system."
        DoCmd.RepaintObject A_FORM, "frmProcessing"
        'Import the file
        DoCmd.TransferSpreadsheet A_IMPORT, 0, strTableName, strFileDetails, -1
    
    ElseIf intOption = 2 Then          'Farmfile output file
        DoCmd.OpenForm "frmProcessing"
        Forms![frmProcessing]![Records] = "Importing " & strTableName & ".txt" & " into system."
        DoCmd.RepaintObject A_FORM, "frmProcessing"
        'Import the file
        DoCmd.TransferText A_IMPORTFIXED, "ImportFarmFile", strTableName, strFileDetails, 0
        strUpdateSQL = "UPDATE DISTINCTROW " & strTableName & " SET " & strTableName & ".Buyer = 'T' & Right([Buyer],Len([Buyer])-1) "
        strUpdateSQL = strUpdateSQL & "WHERE ((" & strTableName & ".Buyer Like '9*'));"
        DoCmd.RunSQL strUpdateSQL
    End If


    'Create copy of table ensuring all fields are text type.
    Set MyDB = CurrentDb()
    Set tdfOrigin = MyDB.TableDefs(strTableName)
    Set tdfOriginFinal = MyDB.CreateTableDef("OriginFinal")

    For i = 0 To tdfOrigin.Fields.Count - 1
        Set fldOrigin = tdfOrigin.Fields(i)
        'Debug.Print tdfOrigin.Fields(i).Name
        Set fldOriginFinal = tdfOriginFinal.CreateField(tdfOrigin.Fields(i).Name)
        fldOriginFinal.Type = DB_TEXT
        tdfOriginFinal.Fields.Append fldOriginFinal
    Next i

    MyDB.TableDefs.Append tdfOriginFinal

    'Get the data from the imported table into the new one.
    strSQL = "INSERT INTO OriginFinal SELECT DISTINCTROW " & strTableName & ".* FROM " & strTableName & ";"
    DoCmd.SetWarnings False
    DoCmd.RunSQL strSQL
    DoCmd.SetWarnings True
    'Get the workgroup and month from the filename, then flatten into tbltmpMonthlyOutput
    strWG = Left(strTableName, (Len(strTableName) - 4))
    strMonth = Mid$(strTableName, (Len(strTableName) - 3), 2)
    ldoImportOutputSheet strTableName, intOption

    MyDB.TableDefs.Delete "OriginFinal"
    
    Forms!frmMonthlyOutputs![fsubtmpMonthlyOutputs].Form.Requery
    DoCmd.Close A_FORM, "frmImportOutputsDialog"

Exit Sub
ldoImportOutputSS_Err:

    Select Case Err
        Case 3176 'Someones got the .xls open.
            DoCmd.Close A_FORM, "frmProcessing"
            MsgBox "Someone has the file you chose open at the moment.", 16
            Exit Sub
        Case 2049   'Occurs if invalid characters are in the spreadsheet data
            MsgBox Err & " " & Error & Chr(10) & Chr(13) & "The following characters are invalid: ' . " & Chr(34) & Chr(10) & Chr(13) & "Tip: replace any full stops with underscores in the spreadsheet" & Chr(10) & Chr(13) & "Aborting import!", 16
            DoCmd.DeleteObject A_TABLE, strTableName
            DoCmd.Close A_FORM, "frmImportOutputsDialog"
            DoCmd.Close A_FORM, "frmProcessing"
            Exit Sub
        Case 3421   'Probably picked the wrong structure spreedsheet.
            MsgBox Err & " " & Error & Chr(10) & Chr(13) & "You have probably chosen the wrong import structure for the spreadsheet." & Chr(10) & Chr(13) & "Aborting import!", 16
            DoCmd.DeleteObject A_TABLE, strTableName
            DoCmd.Close A_FORM, "frmProcessing"
            DoCmd.Close A_FORM, "frmImportOutputsDialog"
            Exit Sub
        Case 3182
            MsgBox Err & " " & Error
            DoCmd.Close A_FORM, "frmProcessing"
            DoCmd.Close A_FORM, "frmImportOutputsDialog"
            Exit Sub
        Case Else
            MsgBox Err & " " & Error    'Anything else ?
            'DoCmd DeleteObject A_TABLE, "OriginFinal"
            Exit Sub
    End Select


End Sub

Sub ldoImportRDTimeSheet(strWorkGroup As String, strMonth As String, strFileName As String)
    
    On Error GoTo ldoImportRDTimeSheet_Err
    
    BeginTrans
    
    Dim MyDB As Database
    Dim rstOrigin As Recordset
    Dim rstDestination As Recordset
    Dim intColumns As Integer, i As Integer, lngCounter As Long, lngRecords As Long, lngCurrentRecord As Long
    
    Set MyDB = CurrentDb()
    Set rstOrigin = MyDB.OpenRecordset(strFileName, DB_OPEN_SNAPSHOT)
    Set rstDestination = MyDB.OpenRecordset("tbltmpMonthlyTime", DB_OPEN_TABLE)
    

    rstOrigin.MoveLast
    rstOrigin.MoveFirst
    lngRecords = rstOrigin.RecordCount
    
    Do Until rstOrigin.EOF
            If intCancel = -1 Then
                Rollback
                GoTo ldoImportRDTimeSheet_Tidy_Up
            End If
            rstDestination.AddNew
            lngCurrentRecord = lngCurrentRecord + 1
            Forms![frmProcessing]![Time] = "Creating record: " & lngCurrentRecord & " of " & lngRecords
            DoCmd.RepaintObject A_FORM, "frmProcessing"
            rstDestination!PACTid = rstOrigin.Fields(0)
            rstDestination!TimeCode = rstOrigin.Fields(1)
            rstDestination!Month = strMonth
            rstDestination!ParentProject = rstOrigin.Fields(2)  '??????????????
            rstDestination!WorkGroup = strWorkGroup
            rstDestination!Hours = rstOrigin.Fields(3)
            rstDestination!PACTStaffID = DLookup("Name", "tblStaff", "PACTid =" & rstDestination!PACTid)
            rstDestination.Update
        DoEvents
        rstOrigin.MoveNext
    Loop
            
    CommitTrans

ldoImportRDTimeSheet_Tidy_Up:

    rstOrigin.Close
    rstDestination.Close
    
    DoCmd.DeleteObject A_TABLE, strFileName
    DoCmd.Close A_FORM, "frmProcessing"
    DoCmd.Close A_FORM, "frmImportDialog"

Exit Sub
ldoImportRDTimeSheet_Err:

    MsgBox Err & " " & Error
    Rollback
    GoTo ldoImportRDTimeSheet_Tidy_Up

End Sub

Sub ldoImportTimeSS(ByVal intOption As Integer)

    On Error GoTo ldoImportTimeSS_Err
    Dim strTableName As String
    Dim strFileDetails As String
    Dim intPosition As Integer
    Dim intLength As Integer
    Dim i As Integer
    Dim strCharacter As String
    Dim strWG As String, strMonth As String
    Dim tdfOrigin As TableDef
    Dim tdfOriginFinal As TableDef
    Dim fldOriginFinal As Field
    Dim fldOrigin As Field
    Dim MyDB As Database
    Dim strSQL As String

    'Get file for importing
    strFileDetails = OpenCommDlg("Excel")
    If strFileDetails = "" Or IsNull(strFileDetails) Then
        DoCmd.Close A_FORM, "frmImportDialog"
        Exit Sub
    End If
    'Strip out the file name from the path details for the temporary table name
    intLength = Len(strFileDetails)
    intPosition = intLength - 4
    For i = 1 To intLength
        If Mid$(strFileDetails, intPosition, 1) = "\" Then
            strTableName = Mid$(strFileDetails, intPosition + 1, (intLength - 4) - intPosition)
            GoTo Import_File
        End If
            intPosition = intPosition - 1
    Next i
    

Import_File:
    
    DoCmd.OpenForm "frmProcessing"
    Forms![frmProcessing]![Records] = "Importing " & strTableName & ".xls" & " into system."
    DoCmd.RepaintObject A_FORM, "frmProcessing"

    'Import the file
    'DoCmd TransferSpreadsheet A_IMPORT, 5, strTableName, strFileDetails, -1
    DoCmd.TransferSpreadsheet A_IMPORT, 0, strTableName, strFileDetails, -1

    If intOption = 3 Then
        'Create copy of table ensuring all fields are text type.
        Set MyDB = CurrentDb()
        Set tdfOrigin = MyDB.TableDefs(strTableName)
        Set tdfOriginFinal = MyDB.CreateTableDef("OriginFinal")
    
        For i = 0 To tdfOrigin.Fields.Count - 1
           
            Set fldOrigin = tdfOrigin.Fields(i)
            'Debug.Print tdfOrigin.Fields(i).Name
            Set fldOriginFinal = tdfOriginFinal.CreateField(tdfOrigin.Fields(i).Name)
            fldOriginFinal.Type = DB_TEXT
            tdfOriginFinal.Fields.Append fldOriginFinal

        Next i
    
        MyDB.TableDefs.Append tdfOriginFinal
    
        'Get the data from the imported table into the new one.
        DoCmd.SetWarnings False
        strSQL = "INSERT INTO OriginFinal SELECT DISTINCTROW " & strTableName & ".* FROM " & strTableName & ";"
        DoCmd.RunSQL strSQL
        DoCmd.SetWarnings True
    End If

    'Get the workgroup and month from the filename, then flatten into tbltmpMonthlyTime
    strWG = Left(strTableName, (Len(strTableName) - 4))
    strMonth = Mid$(strTableName, (Len(strTableName) - 3), 2)

    If intOption = 3 Then
        'Switch tables
        MyDB.TableDefs.Delete strTableName
        strTableName = "OriginFinal"
    End If

    If intOption = 3 Then
        ldoImportXtabTime strWG, strMonth, strTableName
    ElseIf intOption = 1 Then
        ldoImportRDTimeSheet strWG, strMonth, strTableName
    ElseIf intOption = 2 Then
        ldoImportNormalTS strWG, strMonth, strTableName
    Else: Exit Sub
    End If
    
    Forms!frmMonthlyTime![fsubtmpMonthlyTime].Form.Requery

 
Exit Sub
ldoImportTimeSS_Err:

    Select Case Err
        Case 3176 'Someones got the .xls open.
            DoCmd.Close A_FORM, "frmProcessing"
            MsgBox "Someone has the file you chose open at the moment.", 16
            Exit Sub
        Case 2049   'Occurs if invalid characters are in the spreadsheet data
            MsgBox Err & " " & Error & Chr(10) & Chr(13) & "The following characters are invalid: ' . " & Chr(34) & Chr(10) & Chr(13) & "Tip: replace any full stops with underscores in the spreadsheet" & Chr(10) & Chr(13) & "Aborting import!", 16
            DoCmd.DeleteObject A_TABLE, strTableName
            DoCmd.Close A_FORM, "frmImportDialog"
            DoCmd.Close A_FORM, "frmProcessing"
            Exit Sub
        Case 3421   'Probably picked the wrong structure spreedsheet.
            MsgBox Err & " " & Error & Chr(10) & Chr(13) & "You have probably chosen the wrong import structure for the spreadsheet." & Chr(10) & Chr(13) & "Aborting import!", 16
            DoCmd.DeleteObject A_TABLE, strTableName
            DoCmd.Close A_FORM, "frmProcessing"
            DoCmd.Close A_FORM, "frmImportDialog"
            Exit Sub
        Case 3182
            MsgBox Err & " " & Error
            DoCmd.Close A_FORM, "frmProcessing"
            DoCmd.Close A_FORM, "frmImportDialog"
            Exit Sub
        Case Else
            MsgBox Err & " " & Error    'Anything else ?
            DoCmd.DeleteObject A_TABLE, strTableName
            Exit Sub
    End Select

End Sub

Sub ldoImportXtabTime(strWorkGroup As String, strMonth As String, strFileName As String)
    
    On Error GoTo ldoImportXtabTime_Err
    
    BeginTrans

    Dim MyDB As Database
    Dim rstOrigin As Recordset
    Dim rstDestination As Recordset
    Dim intColumns As Integer, i As Integer, lngCounter As Long, lngRecords As Long, lngCurrentRecord As Long
    
    Set MyDB = CurrentDb()
    Set rstOrigin = MyDB.OpenRecordset(strFileName, DB_OPEN_SNAPSHOT)
    Set rstDestination = MyDB.OpenRecordset("tbltmpMonthlyTime", DB_OPEN_TABLE)
    
    intColumns = rstOrigin.Fields.Count

    rstOrigin.MoveLast
    rstOrigin.MoveFirst
    lngRecords = rstOrigin.RecordCount * (intColumns - 3)
    
    Do Until rstOrigin.EOF
        If intCancel = -1 Then
            Rollback
            GoTo ldoImportXtabTime_Tidy_Up
        End If
        lngCounter = lngCounter + 1
        For i = 3 To intColumns - 1
            rstDestination.AddNew
            lngCurrentRecord = lngCurrentRecord + 1
            Forms![frmProcessing]![Time] = "Creating record: " & lngCurrentRecord & " of " & lngRecords
            'Forms!frmMonthlyTime.Visible = False
            DoCmd.RepaintObject A_FORM, "frmProcessing"
            'Debug.Print ldoReplaceCharacter(rstOrigin.Fields(i).Name, "_", ".")
            rstDestination!PACTStaffID = ldoReplaceCharacter(rstOrigin.Fields(i).Name, "_", ".")
            'Debug.Print rstOrigin.Fields(0)
            rstDestination!TimeCode = rstOrigin.Fields(0)
            'Debug.Print strMonth
            rstDestination!Month = strMonth
            'Debug.Print rstOrigin(2)
            rstDestination!ParentProject = rstOrigin(2)
            'Debug.Print strWorkGroup
            rstDestination!WorkGroup = strWorkGroup
            'Debug.Print rstOrigin.Fields(i)
            If IsNull(rstOrigin.Fields(i)) Or rstOrigin.Fields(i) = "" Then
                rstDestination!Hours = "0"
            Else
                rstDestination!Hours = rstOrigin.Fields(i)
            End If
            rstDestination.Update
        Next i
        DoEvents
        rstOrigin.MoveNext
    Loop

    CommitTrans

ldoImportXtabTime_Tidy_Up:
    rstOrigin.Close
    rstDestination.Close
    
    DoCmd.DeleteObject A_TABLE, strFileName
    DoCmd.Close A_FORM, "frmProcessing"
    DoCmd.Close A_FORM, "frmImportDialog"

Exit Sub
ldoImportXtabTime_Err:

    MsgBox Err & " " & Error
    Rollback
    GoTo ldoImportXtabTime_Tidy_Up

End Sub

Function ldoPrintQuerySQL()

    Dim MyDB As Database
    Dim MyQuery As QueryDef

    Set MyDB = CurrentDb()
    Set MyQuery = MyDB.QueryDefs("qryJobMonth-Single")

    Debug.Print MyQuery.sql


End Function

Sub ldoRemoveSuffix()


    Dim MyDB As Database
    Dim rst As Recordset
    Dim strTimeCode As String
    Dim i As Integer
    Dim strSQL As String

    strSQL = "SELECT DISTINCTROW tbltmpMonthlyTime.* "
    strSQL = strSQL & "FROM tbltmpMonthlyTime "
    strSQL = strSQL & "WHERE ((tbltmpMonthlyTime.TimeCode Like 'TA*'));"



    Set MyDB = CurrentDb()
    Set rst = MyDB.OpenRecordset(strSQL, DB_OPEN_DYNASET)
        
        Do Until rst.EOF
            If Right(rst!TimeCode, 1) = ")" Then
                Debug.Print rst!TimeCode & " " & Left(rst!TimeCode, (Len(rst!TimeCode) - 3))
                strTimeCode = Left(rst!TimeCode, (Len(rst!TimeCode) - 3))
                rst.Edit
                rst!TimeCode = strTimeCode
                rst.Update
            End If
            rst.MoveNext
        Loop
    
    rst.Close

End Sub

Function ldoReplaceCharacter(ByVal strField As String, strCharacter As String, strReplace As String) As String

    Dim intPosition As Integer

    intPosition = InStr(1, strField, strCharacter)

    If intPosition = 0 Then
        ldoReplaceCharacter = strField
        Exit Function
    End If

        ldoReplaceCharacter = Left$(strField, intPosition - 1) & strReplace & Right$(strField, Len(strField) - (intPosition + Len(strCharacter) - 1))

End Function

Function ldoTime(lngSeconds As Long) As Variant

Dim intHours As Single, lngMinutes As Long, lngRemainderSeconds As Long, lngRemainder As Long
Dim strInterval As String

ldoTime = "00:00:00"
If lngSeconds > 86399 Then Exit Function
If lngSeconds < 0 Then Exit Function

intHours = Int(lngSeconds / 3600)
lngRemainder = lngSeconds - (intHours * 3600)
lngMinutes = Int(lngRemainder / 60)
lngRemainderSeconds = (lngSeconds - ((intHours * 3600) + (lngMinutes * 60)))

strInterval = intHours & ":" & lngMinutes & ":" & lngRemainderSeconds
ldoTime = Format(TimeValue(strInterval) + Time, "hh:nn:ss")

End Function

Function ldoTime2(varTime As Variant) As Variant

Dim Elapsed As Variant, HourDiff As Variant, MinuteDiff As Variant, SecondDiff As Variant
Dim TotalMinDiff As Variant, TotalSecDiff As Variant, msg As String

Elapsed = TimeValue(varTime)
' Get differences.
HourDiff = Hour(Elapsed) - Hour(Now)
MinuteDiff = Minute(Elapsed) - Minute(Now)
SecondDiff = Second(Elapsed) - Second(Now) + 1
If SecondDiff = 60 Then
    MinuteDiff = MinuteDiff + 1 ' Add 1 to minute.
    SecondDiff = 0  ' Zero seconds.
End If
If MinuteDiff = 60 Then
    HourDiff = HourDiff + 1 ' Add 1 to hour.
    MinuteDiff = 0  ' Zero minutes.
End If
TotalMinDiff = (HourDiff * 60) + MinuteDiff   ' Get totals.
TotalSecDiff = (TotalMinDiff * 60) + SecondDiff
'Msg = "There are a total of " & Format(TotalSecDiff, "#,##0")
'Msg = Msg & " seconds until Elapsed. That translates to "
'Msg = Msg & HourDiff & " hours, " & MinuteDiff
'Msg = Msg & " minutes, and " & SecondDiff & " seconds."
'MsgBox Msg              ' Display message.

ldoTime2 = TotalSecDiff

End Function

 Function OpenCommDlg(OpeningApp As String) As String
     Dim message$, Filter$, FileName$, FileTitle$, DefExt$
     Dim Title$, szCurDir$, APIResults%, strFileName As String, strFileExt As String

    If OpeningApp = "Excel" Then
        strFileName = "Excel"
        strFileExt = ".xls"
    ElseIf OpeningApp = "Text" Then
        strFileName = "Text"
        strFileExt = ".txt"
    End If

       '*Define the filter string and allocate space in the "c" string
       'Filter$ = "Excel(*.xls)" & Chr$(0) & "*.xls" & Chr$(0)
       Filter$ = strFileName & "(*" & strFileExt & ")" & Chr$(0) & "*" & strFileExt & Chr$(0)
       
       Filter$ = Filter$ & Chr$(0)

       '* Allocate string space for the returned strings.
       FileName$ = Chr$(0) & Space$(255) & Chr$(0)
       FileTitle$ = Space$(255) & Chr$(0)

       '* Give the dialog a caption title.
       Title$ = "Choose " & strFileExt & " for import" & Chr$(0)

       '* If the user does not specify an extension, append TXT.
       DefExt$ = Right(strFileExt, 3) & Chr$(0)

       '* Set up the default directory
       szCurDir$ = CurDir$ & Chr$(0)

       '* Set up the data structure before you call the GetOpenFileName

       OPENFILENAME.lStructSize = Len(OPENFILENAME)

       'If the OpenFile Dialog box is linked to a form use this line.
          'It will pass the forms window handle.

       'OPENFILENAME.hwndOwner = Screen.ActiveForm.hWnd

       'If the OpenFile Dialog box is not linked to any form use this line.
       'It will pass a null pointer.

       OPENFILENAME.hwndOwner = 0&
       
       OPENFILENAME.lpstrFilter = lstrcpy(Filter$, Filter$)
       OPENFILENAME.nFilterIndex = 1
       OPENFILENAME.lpstrFile = lstrcpy(FileName$, FileName$)
       OPENFILENAME.nMaxFile = Len(FileName$)
       OPENFILENAME.lpstrFileTitle = lstrcpy(FileTitle$, FileTitle$)
       OPENFILENAME.nMaxFileTitle = Len(FileTitle$)
       OPENFILENAME.lpstrTitle = lstrcpy(Title$, Title$)
       OPENFILENAME.flags = OFN_FILEMUSTEXIST Or OFN_READONLY
       OPENFILENAME.lpstrDefExt = lstrcpy(DefExt$, DefExt$)
       OPENFILENAME.hInstance = 0
       OPENFILENAME.lpstrCustomFilter = 0
       OPENFILENAME.nMaxCustFilter = 0
       OPENFILENAME.lpstrInitialDir = lstrcpy(szCurDir$, szCurDir$)
       OPENFILENAME.nFileOffset = 0
       OPENFILENAME.nFileExtension = 0
       OPENFILENAME.lCustData = 0
       OPENFILENAME.lpfnHook = 0
       OPENFILENAME.lpTemplateName = 0

       '* This will pass the desired data structure to the Windows API,
       '* which in turn uses it to display the Open Dialog form.

       APIResults% = GetOpenFileName(OPENFILENAME)

       If APIResults% <> 0 Then

           '* Note that FileName$ will have an embedded Chr$(0) at the
           '* end. You may wish to strip this character from the string.

         FileName$ = Left$(FileName$, InStr(FileName$, Chr$(0)) - 1)

         OpenCommDlg = FileName$

       Else
         OpenCommDlg = ""
       End If


   End Function

Function fnResetConnections()
    ldoAttachODBCTables
    attach_Passthrough_query
    Run_All_DD_queries
End Function