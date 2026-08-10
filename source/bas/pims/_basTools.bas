MODULE NAME: _basTools
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database   'Use database order for string comparisons
Option Explicit

Global Const gHelpFileName = ""
Const stripChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890"

Global DelayDataErrors As Integer
Global DataErrors As String
Global Const errorLogFn = "errorlog.txt"

Global waste, RetVal
Global gScheduleStartdateTime   As String
Global gScheduleMessage         As String
Global gScheduleCancelled       As Integer


Type globalVars
    sysPath                 As String
    DBname                  As String
End Type

Global g As globalVars

Declare PtrSafe Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As Any, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Long, ByVal lpFileName As String) As Long
Declare PtrSafe Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (ByVal lpApplicationName As String, ByVal lpKeyName As Any, ByVal lpString As Any, ByVal lpFileName As String) As Long

Global Const bufsize = 255

Declare PtrSafe Function GetSystemMetrics Lib "user32" (ByVal nIndex As Long) As Long

Const SM_CXSCREEN = 0
Const SM_CYSCREEN = 1

Declare PtrSafe Function GetActiveWindow Lib "user32" () As LongPtr
Declare PtrSafe Function GetDC Lib "user32" (ByVal hWnd As LongPtr) As LongPtr
Declare PtrSafe Function GetDeviceCaps Lib "gdi32" (ByVal hdc As LongPtr, ByVal nIndex As Long) As Long
Declare PtrSafe Function ReleaseDC Lib "user32" (ByVal hWnd As LongPtr, ByVal hdc As LongPtr) As Long

'Type WindowRectType
'    X1 As Long
'    Y1 As Long
'    X2 As Long
'    Y2 As Long
'End Type

Type RECT
    Left As Long
    Top As Long
    Right As Long
    Bottom As Long
End Type

Declare PtrSafe Function apiGetWindowRect Lib "user32" Alias "GetWindowRect" (ByVal hWnd As LongPtr, lpRect As RECT) As Long

Const HORZRES = 8        '  Horizontal width in pixels
Const HORZSIZE = 4       '  Horizontal size in millimeters
Const VERTRES = 10       '  Vertical width in pixels
Const VERTSIZE = 6       '  Vertical size in millimeters
Const LOGPIXELSX = 88    '  Logical pixels/inch in X
Const LOGPIXELSY = 90    '  Logical pixels/inch in Y

Declare PtrSafe Function GetParent Lib "user32" (ByVal hWnd As LongPtr) As LongPtr
Declare PtrSafe Function SetWindowText Lib "user32" Alias "SetWindowTextA" (ByVal hWnd As LongPtr, ByVal lpString As String) As Long
Declare PtrSafe Function GetVersion Lib "kernel32" () As Long
Declare PtrSafe Function LocalFree Lib "kernel32" (ByVal hMem As LongPtr) As LongPtr
Declare PtrSafe Function GlobalFree Lib "kernel32" (ByVal hMem As LongPtr) As LongPtr
Declare PtrSafe Function HeapFree Lib "kernel32" (ByVal hHeap As LongPtr, ByVal dwFlags As Long, lpMem As Any) As Long
Declare PtrSafe Function GetDiskFreeSpace Lib "kernel32" Alias "GetDiskFreeSpaceA" (ByVal lpRootPathName As String, lpSectorsPerCluster As Long, lpBytesPerSector As Long, lpNumberOfFreeClusters As Long, lpTtoalNumberOfClusters As Long) As Long

Declare PtrSafe Function getUserName Lib "advapi32.dll" Alias "GetUserNameA" (ByVal lpBuffer As String, nSize As Long) As Long

'Declare Function wapi_GetUserName Lib "advapi32.dll" Alias _
          "GetUserNameA" (ByVal lpBuffer As String, nSize As Long) As Long

Declare PtrSafe Function wapi_GetComputerName Lib "kernel32" Alias _
   "GetComputerNameA" (ByVal sBuffer As String, lSize As Long) As Long
   
'for running .vbs from access = see sub RunVBS
Public Declare PtrSafe Function ShellExecute Lib "shell32.dll" _
    Alias "ShellExecuteA" (ByVal hWnd As LongPtr, _
    ByVal lpOperation As String, ByVal lpFile As String, _
    ByVal lpParameters As String, ByVal lpDirectory As String, _
    ByVal nShowCmd As Long) As LongPtr
    
   '===========================
' COMBO BOX DROP STATE STUFF
'===========================
'  retrieves the name of the class to which the specified window belongs.
Private Declare PtrSafe Function apiGetClassName Lib "user32" Alias "GetClassNameA" (ByVal hWnd As LongPtr, ByVal lpClassname As String, ByVal nMaxCount As Long) As Long
'  retrieves a handle to the specified child window's parent window.
Private Declare PtrSafe Function apiGetParent Lib "user32" Alias "GetParent" (ByVal hWnd As LongPtr) As LongPtr
'  retrieves information about the specified window. The function also
'  retrieves the 32-bit (long) value at the specified offset into the
'  extra window memory of a window.
Private Declare PtrSafe Function apiGetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hWnd As LongPtr, ByVal nIndex As Long) As Long
'  retrieves a handle to the top-level window whose class name and
'  window name match the specified strings. This function does not search
'  child windows. This function does not perform a case-sensitive search.
Private Declare PtrSafe Function apiFindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassname As String, ByVal lpWindowName As String) As LongPtr
'  retrieves a handle to a window that has the specified relationship
'  (Z order or owner) to the specified window
Private Declare PtrSafe Function apiGetWindow Lib "user32" _
    Alias "GetWindow" (ByVal hWnd As LongPtr, ByVal wCmd As Long) As LongPtr
'  The class name for an Access combo's drop down listbox window
Private Const ACC_CBX_LISTBOX_CLASS = "OGrid"
'  Class name for the Access window
Private Const ACC_MAIN_CLASS = "OMain"
'  Class name for an Access combo's drop down listbox's parent window
Private Const ACC_CBX_LISTBOX_PARENT_CLASS = "ODCombo"
'  class name for an Access form's client window
Private Const ACC_FORM_CLIENT_CLASS = "OFormSub"
'  class name for Edit controls in Access
Private Const ACC_CBX_EDIT_CLASS = "OKttbx"
'  class name for VB combo's drop down listbox's parent window (SDI)
Private Const VB_CBX_LISTBOX_PARENT_CLASS = "#32769" ' // Desktop
'  class name for VB combo's drop down listbox window
Private Const VB_CBX_LISTBOX_CLASS = "ComboLBox"
'  handle identifies the child window at the top of the Z order,
'  if the specified window is a parent window
Private Const GW_CHILD = 5
'  Retrieves the window styles.
Private Const GWL_STYLE = (-16)
'  flag denoting that a window is visible
Private Const WS_VISIBLE = &H10000000
'===========================
' END of COMBO BOX DROP STATE STUFF
'===========================

Public Function RunVBS(vbsFileName As String)
    
' Run .VBS file from access e.g. RunVBS "\\vla31\frontends$", "Install TSE Archive_new.vbs"
    
    Dim p As Integer
    Dim hWnd As LongPtr
    Dim lpOperation As String
    Dim lpParameters As String
    Dim lReturn As LongPtr
    Dim strPath As String, strFN As String

    p = Len(vbsFileName)
    Do While Mid$(vbsFileName, p, 1) <> "\" And p > 0: p = p - 1: Loop
    If p = 0 Then
        MsgBox "RunVBS: no path name specified"
        Exit Function
    End If
    
    strPath = Left$(vbsFileName, p)
    strFN = Mid$(vbsFileName, p + 1)
    
        
    hWnd = Application.hWndAccessApp
    lpOperation = "Open"
    lpParameters = vbNullString

    lReturn = ShellExecute(hWnd, lpOperation, strFN, lpParameters, strPath, vbMaximizedFocus)
End Function


Public Function UserName() As String

'returns the NT Username of the currently logged in user
    
    Dim strUserName As String * 100
    Dim pLen As Long
    Dim RetVal As Long

    pLen = Len(strUserName)
    RetVal = getUserName(strUserName, pLen)
    pLen = InStr(strUserName, Chr$(0)) - 1
    UserName = Left$(strUserName, pLen)

End Function
Sub ArraySort(anyArray() As String, AorD As String)
Dim p As Integer, b As Integer, s As Integer, l As Integer, u As Integer
Dim c As Integer
Dim temp As Variant
c = False
l = LBound(anyArray)
u = UBound(anyArray)
Select Case UCase(AorD)
  Case "A"
    Do
        c = False
        For p = l To u - 1
            If anyArray(p) > anyArray(p + 1) Then
                temp = anyArray(p)
                anyArray(p) = anyArray(p + 1)
                anyArray(p + 1) = temp
                c = True
            End If
        Next p
        If c = False Then Exit Do
    Loop

  Case "D"
    Do
        c = False
        For p = u To l + 1 Step -1
            If anyArray(p) > anyArray(p - 1) Then
                temp = anyArray(p)
                anyArray(p) = anyArray(p - 1)
                anyArray(p - 1) = temp
                c = True
            End If
        Next p
        If c = False Then Exit Do
    Loop

End Select

End Sub

Function blankCombo() As Integer
On Error Resume Next
Dim c As Control
Set c = Screen.ActiveControl
If isBlank(c.Text) Then
    c = Null
End If
On Error GoTo 0
blankCombo = True
End Function

Function breakApart(itemList, delimiterList, itemArray(), Optional blnTrim = True) As Integer

' Breaks itemList into elements of itemArray()
' using delimterList to detect separators
' Output items will be trimmed of leading and trailing spaces

Dim p As Integer, i As Integer, item As String

item = ""
i = 1
ReDim itemArray(1 To 1)

For p = 1 To Len(itemList & "")

    ' check for a delimiter
    
    If InStr(delimiterList, Mid$(itemList, p, 1)) = 0 Then
        item = item & Mid$(itemList, p, 1)
    Else
    
        ' delimiter found, so item complete
        If item <> "" Then
            itemArray(i) = IIf(blnTrim, Trim(item), item)
        Else
            itemArray(i) = Null
        End If
        i = i + 1
        ReDim Preserve itemArray(1 To i)
        item = ""

    End If

Next

If item <> "" Then
    itemArray(i) = Trim(item)
    breakApart = UBound(itemArray)
Else
    If i = 1 Then
        breakApart = 0
    Else
        ReDim Preserve itemArray(1 To i - 1)
        breakApart = UBound(itemArray)
    End If
End If

End Function

Function breakApart2(itemList, delimiterList, startPointAr(), wordLenAr()) As Integer

' Breaks itemList into elements of itemArray()
' using delimiterList to detect separators
' pointer to start of word is placed in startPointAr, length is in wordLenAr

Dim p As Long, w As Long, i As Long, item As String, l As Long, x As String

item = ""
i = 1
ReDim startPointAr(1 To 1)
ReDim wordLenAr(1 To 1)
x = itemList
l = Len(x)
p = 1: w = 0

Do While p < l

    ' look for start of word i.e. characters are in delimiterList

    Do While p <= l And InStr(delimiterList, Mid$(x, p, 1)) > 0
        p = p + 1
    Loop
    If p > l Then Exit Do

    ' start point found - store it

    w = w + 1
    ReDim Preserve startPointAr(1 To w)
    ReDim Preserve wordLenAr(1 To w)

    startPointAr(w) = p

    ' look for end of word i.e. characters are NOT in delimiterList
    
    Do While p <= l And InStr(delimiterList, Mid$(x, p, 1)) = 0
        p = p + 1
    Loop

    ' end of word found - store length

    wordLenAr(w) = p - startPointAr(w)
Loop

breakApart2 = w

End Function

'
Function CaseConvertaColumn(tableName, ColumnName, mode As String) As Integer

' mode = "u" Upper case, "l" lower case, "p" properName, "" prompt for mode

Dim SQLst As String
On Error GoTo CaseConvertaColumn_err

DoCmd.SetWarnings False

SQLst = "UPDATE [" & tableName & "] SET [" & tableName & "].[" & ColumnName & "] = properName([" & ColumnName & "]);"
DoCmd.RunSQL (SQLst)
DoCmd.SetWarnings True

CaseConvertaColumn = True

CaseConvertaColumn_exit:
    Exit Function

CaseConvertaColumn_err:
    msgSystemError Err, "SQLst", "CaseConvertaColumn"
    CaseConvertaColumn = False
    Resume CaseConvertaColumn_exit

End Function

Function ChangeAttachment(tableName, newDB) As Integer

Const connectPrefix = ";DATABASE="
    
On Error GoTo ChangeAttachment_err

Dim tbl As TableDef
Dim connectSt As String
Dim db As Database

Set db = DBEngine(0)(0)

Set tbl = db.TableDefs(tableName)
If tbl.Connect <> connectPrefix & newDB Then
    tbl.Connect = connectPrefix & newDB
    tbl.RefreshLink
End If

ChangeAttachment = True

ChangeAttachment_ok:
    Exit Function


ChangeAttachment_err:
    ChangeAttachment = False
    msgSystemError Err, Error$ & nl() & connectPrefix & newDB, "ChangeAttachment"
    Resume ChangeAttachment_ok


End Function

Function clearMe()
Screen.ActiveControl = Null
End Function

Function CopyFile(sFN, tFN, opMsg) As Long

' e.g. CopyFile("c:\pacs\pacs.ico","s:\oldpacs\pacs.ico", msg)  as long

Dim s As Integer, T As Integer, flen As Long, buf As String, bufsize As Long, bytesLeft As Long, RetVal As Variant

On Error GoTo CopyFile_err

CopyFile = 0
bufsize = 32768
buf = String$(bufsize, " ")

If Dir(sFN) = "" Then
    MsgBox "No such file as " & sFN, 48, "CopyFile"
    CopyFile = False
    Exit Function
End If

s = FreeFile
Open sFN For Binary Access Read Lock Read As #s

T = FreeFile
Open tFN For Binary Access Write As #T

flen = LOF(s)
RetVal = SysCmd(SYSCMD_INITMETER, "Copying...", flen)

Do While Loc(s) < flen
    bytesLeft = flen - Loc(s)
    If bytesLeft < bufsize Then
        bufsize = bytesLeft
        buf = String$(bufsize, " ")
    End If
    Get #s, , buf
    Put #T, , buf
    RetVal = SysCmd(SYSCMD_UPDATEMETER, flen - bytesLeft)
Loop

CopyFile = flen
opMsg = "OK"

CopyFile_end:
    On Error Resume Next
    Close #s
    Close #T
    RetVal = SysCmd(SYSCMD_REMOVEMETER)
    Exit Function

CopyFile_err:
    
    ' to handle rare 'out of string space' error
    If Err = 14 And bufsize > 4096 Then
        bufsize = bufsize / 2
        Resume
    End If

    opMsg = "Error:" & str$(Err) & " - " & Error$
    CopyFile = False
    Close
    Resume CopyFile_end

End Function

Function copyEditFile(sFN, tFN, strOld As String, strNew As String) As Long
' Copies a file replacing occurrences of strOLD with strNEW

Dim s As Integer, T As Integer, flen As Long, buf As String, bufsize As Long, bytesLeft As Long, RetVal As Variant

On Error GoTo copyEditFile_err

copyEditFile = 0
bufsize = 32768
buf = String$(bufsize, " ")

If Dir(sFN) = "" Then
    MsgBox "No such file as " & sFN, 48, "copyEditFile"
    copyEditFile = False
    Exit Function
End If

s = FreeFile
Open sFN For Binary Access Read Lock Read As #s

T = FreeFile
Open tFN For Binary Access Write As #T

flen = LOF(s)
RetVal = SysCmd(SYSCMD_INITMETER, "Copying...", flen)

Do While Loc(s) < flen
    bytesLeft = flen - Loc(s)
    If bytesLeft < bufsize Then
        bufsize = bytesLeft
        buf = String$(bufsize, " ")
    End If
    Get #s, , buf
    buf = replaceChars(buf, strOld, strNew)
    Put #T, , buf
    RetVal = SysCmd(SYSCMD_UPDATEMETER, flen - bytesLeft)
Loop

copyEditFile = flen

copyEditFile_end:
    On Error Resume Next
    Close #s
    Close #T
    RetVal = SysCmd(SYSCMD_REMOVEMETER)
    Exit Function

copyEditFile_err:
    
    ' to handle rare 'out of string space' error
    If Err = 14 And bufsize > 4096 Then
        bufsize = bufsize / 2
        Resume
    End If

    MsgBox "Error:" & str$(Err) & " - " & Error$
    copyEditFile = False
    Close
    Resume copyEditFile_end

End Function

Function deleteConfirmed()
deleteConfirmed = (MsgBox("Are you sure?", 36, "Delete") = 6)

End Function

Sub DeleteImportErrorsTable()
Dim i As Integer, LDB As Database

Set LDB = CurrentDb()

On Error Resume Next
For i = 0 To LDB.TableDefs.Count - 1
    If Left$(LDB.TableDefs(i).Name, 13) = "Import Errors" Then
        DoCmd.DeleteObject A_TABLE, LDB.TableDefs(i).Name
    End If
Next i

End Sub

Function deleteRecord()
Dim Msg As String, rc%, fm As Form, strBookmark As String, ctrl As Control
Dim isPosted As Boolean
Msg = ""
Set ctrl = Screen.ActiveControl
Set fm = ctrl.Parent.Form

deleteRecord = True
On Error Resume Next
strBookmark = fm.Bookmark
isPosted = strBookmark <> "" 'new unposted records have no bookmark
On Error Resume Next
    If fm.Dirty Then DoCmd.RunCommand acCmdUndo
    If fm.Dirty Then DoCmd.RunCommand acCmdUndo
On Error GoTo deleteRecord_err
    DoCmd.RunCommand acCmdSelectRecord
    If isPosted Then
        If StrComp(fm.Bookmark, strBookmark, 0) <> 0 Then
            MsgBox "Trying to delete wrong record"
            Exit Function
        End If
End If
    warnings False
    DoCmd.RunCommand acCmdDeleteRecord
    warnings True
    Exit Function

deleteRecord_err:
    Application.Echo True
    Select Case Err
        Case 3021: ' no record
        Case 2046: ' command not available
        Case 40036: ' no record
        Case 3200:  Msg = "This record may not be deleted - it is referred to by existing data"
        Case Else:  Msg = str$(Err) & Chr(13) & Chr(10) & Error$
    End Select
    On Error GoTo 0
    If Msg <> "" Then
        MsgBox Msg, 16, "Delete Record Error!"
        deleteRecord = False
    End If
    Exit Function
    Resume
End Function

Function DeleteRecordWithConfirm()

If deleteConfirmed() Then
    DeleteRecordWithConfirm = deleteRecord()
Else
    DeleteRecordWithConfirm = False
End If

End Function

Function DoReport(DOCNAME, Optional SelectionCriteria = "", Optional Normal_0_Preview_2 = 2) As Integer

On Error Resume Next

If Screen.ActiveForm.Dirty Then
    If Not saveRecord() Then Exit Function
End If

On Error GoTo Err_DoReport
DoCmd.OpenReport DOCNAME, Normal_0_Preview_2, , SelectionCriteria

Exit_DoReport:
    Exit Function

Err_DoReport:
    Select Case Err
        Case 2103: msgInfo "Report not yet available"
        Case 2501: msgInfo "Report Cancelled"
        Case Else:
            MsgBox str$(Err) & ": " & Error$
    End Select

    Resume Exit_DoReport
End Function

Function emptyTable(tableName) As Integer


emptyTable = runActionQueryCmd("delete * from [" & tableName & "];", "emptyTable " & tableName)


End Function

Function EraseControl()
Screen.ActiveControl = Null
End Function

Function EventsDo()
DoEvents
End Function

Function FormClose()
DoCmd.Close
End Function

Function GetAccesshWnd()
    GetAccesshWnd = Application.hWndAccessApp
    
End Function

Function getDirList(path As String, dNames() As String, dNameList As String) As Integer

' Gets a list of file and directory names found in the specified directory
' then use only the directory names, excluding "." and ".."
' The dnames() array will then contain the directory names
' The number of directories will be returned
' Note: URLs may be used instead of normal path names (e.g. \\euhor07\fred etc)

Dim fn As String
Dim c As Integer, p As Integer, d As Integer
Dim Delim As String

c = 0
ReDim FNames(1 To 1) As String


fn = Dir(path, vbDirectory)
Do While Len(fn) > 0
    c = c + 1
    If UBound(FNames) < c Then ReDim Preserve FNames(1 To c)
    FNames(c) = fn
    fn = Dir
Loop

ArraySort FNames(), "A"

ReDim dNames(1 To 1) As String
dNameList = ""
Delim = ""
d = 0
For p = 1 To c
    If FNames(p) <> "." And FNames(p) <> ".." And isDir(path & FNames(p)) Then
        d = d + 1
        If UBound(dNames) < d Then ReDim Preserve dNames(1 To d)
        dNames(d) = FNames(p)
        dNameList = dNameList & Delim & FNames(p)
        Delim = ";"
    End If
Next
getDirList = d

End Function


Function getDirListTest(path, level)

' Print a directory tree in immediate window
' Level should be 1 for initial call

Dim RetVal
Dim dNames() As String, dList As String, xpath As String, n As Integer
Dim margin As String, noDirs As Integer

xpath = path
If Right$(xpath, 1) <> "\" Then xpath = xpath & "\"

noDirs = getDirList(xpath, dNames(), dList)
margin = ""
For n = 1 To level
    margin = margin & "  |"
Next
If level = 1 Then
    Debug.Print xpath
    If noDirs = -1 Then
        Debug.Print "Directory not found!"
    End If
End If

If noDirs > 0 Then
    Debug.Print margin
    For n = 1 To noDirs
        Debug.Print margin & "--" & dNames(n)
        RetVal = getDirListTest(xpath & dNames(n), level + 1)
    Next
    Debug.Print margin
End If
End Function

Function getFileList(Filespec As String, FNames() As String, FNameList As String) As Integer

' Gets a list of filenames found in the specified directory
' The FNames() array will then contain the filenames
' The number of files will be returned

Dim fn As String
Dim c As Integer, p As Integer
Dim Delim As String

c = 0
ReDim FNames(1 To 1) As String
FNameList = ""
Delim = ""

fn = Dir(Filespec)
Do While Len(fn) > 0
    c = c + 1
    If UBound(FNames) < c Then ReDim Preserve FNames(1 To c)
    FNames(c) = fn
    fn = Dir
Loop

ArraySort FNames(), "A"

For p = 1 To c
    FNameList = FNameList & Delim & FNames(p)
    Delim = ";"
Next

getFileList = c

End Function

Function getFileNamePart(fullPath)

' Extracts the file name from a path & filename
' (i.e. all characters after the last "\" in a string)

Dim p As Integer
If IsNull(fullPath) Then
    getFileNamePart = ""
    Exit Function
End If
p = Len(fullPath)
Do While p > 0
    If Mid$(fullPath, p, 1) = "\" Then Exit Do
    p = p - 1
Loop
getFileNamePart = Mid$(fullPath, p + 1)


End Function

Function getFileNamePath(fullPath)

' Extracts the path from a path & filename
' (i.e. all characters before the last "\" in a string)

Dim p As Integer
p = Len(fullPath)
Do While p > 0
    If Mid$(fullPath, p, 1) = "\" Then Exit Do
    p = p - 1
Loop
If p = 0 Then
    getFileNamePath = ""
Else
    getFileNamePath = Mid$(fullPath, 1, p)
End If

End Function

Function getParam(itemName As String) As Variant
Dim rs As Recordset
Dim db As Database
On Error GoTo getParam_err

Set db = DBEngine(0)(0)
Set rs = db.OpenRecordset("select " & itemName & " as configItem from [tbl ParamsUser];")
If rs.EOF Then
    getParam = Null
Else
    getParam = rs!configitem
End If

rs.Close

getParam_exit:
    Exit Function

getParam_err:
    If Err = 3061 Then
        msgSystemError Err, SQLstring(itemName) & " is not a valid parameter name", "getParam"
    Else
        msgSystemError Err, Error$, "getParam"
    End If
    Resume getParam_exit

End Function

Function getPath(settingName As String, pathVar) As Integer

Dim path As Variant
getPath = False

    path = SettingsGet(settingName)
    
    If path & "" = "" Then
        path = SystemPath()
    Else
        If Right$(path, 1) <> "\" Then path = path & "\"
        If Not isDir(path) Then
            msgSystemError 0, "", "Cannot find path " & path & nl() & "Please correct setting name: " & settingName
            Exit Function
        End If
    End If
pathVar = path
getPath = True

End Function

Function getSysParam(itemName As String) As Variant

On Error GoTo getSysParam_err

Dim rs As Recordset
Dim db As Database

Set db = DBEngine(0)(0)
Set rs = db.OpenRecordset("select " & itemName & " as configItem from [tbl ParamsData];")
If rs.EOF Then
    getSysParam = Null
Else
    getSysParam = rs!configitem
End If

rs.Close
getSysParam_end:
    Exit Function

getSysParam_err:
    Resume getSysParam_end

End Function

Function getWindowsVersion() As Double
Dim v As Double, h As String
Dim winVer As Double, dosVer As Double

    v = GetVersion()
    h = Right$("0" & Hex$(v), 8)

    winVer = Val("&H" & Mid$(h, 7, 2)) + (Val("&H" & Mid$(h, 5, 2)) / 100)
    dosVer = Val("&H" & Mid$(h, 1, 2)) + (Val("&H" & Mid$(h, 3, 2)) / 100)

getWindowsVersion = winVer

End Function

Function GoFirst(fn As String)
On Error Resume Next
Err = 0
If fn & "" = "" Then
    DoCmd.GoToRecord , , A_FIRST
Else
    DoCmd.GoToRecord A_FORM, fn, A_FIRST
End If
GoFirst = (Err = 0)
End Function

Function GoLast(fn As String)
On Error Resume Next
Err = 0
If fn & "" = "" Then
    DoCmd.GoToRecord , , A_LAST
Else
    DoCmd.GoToRecord A_FORM, fn, A_LAST
End If
GoLast = (Err = 0)
End Function

Function GoNew(Optional FormName As Variant)
On Error Resume Next
Err = 0
If IsMissing(FormName) Then
    DoCmd.GoToRecord , , A_NEWREC
Else
    DoCmd.GoToRecord A_FORM, FormName, A_NEWREC
End If
If Err <> 0 Then
    msgSystemError Err, Error$, "GoNew"
    GoNew = False
Else
    GoNew = True
End If

End Function

Function GoNext(fn As String)
On Error Resume Next
Err = 0
If fn & "" = "" Then
    DoCmd.GoToRecord , , A_NEXT
Else
    DoCmd.GoToRecord A_FORM, fn, A_NEXT
End If
GoNext = (Err = 0)
End Function

Function GoNextPage(fn As String)
SendKeys "{PgDn}"

End Function

Function GoPrevious(fn As String)
On Error Resume Next
Err = 0
If fn & "" = "" Then
    DoCmd.GoToRecord , , A_PREVIOUS
Else
    DoCmd.GoToRecord A_FORM, fn, A_PREVIOUS
End If

End Function

Function GoPrevPage(fn As String)
SendKeys "{PgUp}"

End Function

Function HasField(tblDef As TableDef, fldName) As Integer

Dim fld As field

On Error GoTo hasField_err

HasField = False
Set fld = tblDef.Fields(fldName)
HasField = True

hasField_exit:
Exit Function

hasField_err:
Resume hasField_exit

End Function
Function HasRSField(rs As Recordset, fldName) As Integer

Dim fld As field

On Error GoTo HasRSField_err

HasRSField = False
Set fld = rs.Fields(fldName)
HasRSField = True

HasRSField_err:
Exit Function

End Function


Function int1(numVal) As Variant
' int numeric values, leave others

If IsNumeric(numVal) And Not IsEmpty(numVal) Then
    int1 = Int(numVal)
Else
    int1 = numVal
End If
End Function

Function isBlank(varName As Variant) As Integer
On Error GoTo IsBlank_fail

isBlank = (varName & "" = "")

IsBlank_exit:
    Exit Function

IsBlank_fail:
    isBlank = True
    Resume IsBlank_exit

End Function

Function isDir(dirName)
Dim x, d
On Error Resume Next
Err = 0
d = dirName
If Right$(d, 1) <> "\" Then d = d & "\"

x = Dir(d, vbDirectory)
isDir = (x <> "")

End Function


Function isEmptyRS(rs As Recordset)

Dim rs2 As Recordset, RetVal As Integer

On Error GoTo IsEmptyRS_err

RetVal = True

Set rs2 = rs
rs.MoveFirst
If Not (rs.EOF And rs.BOF) Then
    RetVal = False
End If

IsEmptyRS_end:
    isEmptyRS = RetVal
Exit Function

IsEmptyRS_err:
    If Err = 3021 Then
        Resume IsEmptyRS_end
    End If
    Error Err

End Function

Function isFile(FileName) As Integer

Dim fn As String

fn = ""

On Error Resume Next

fn = Dir(FileName)

isFile = (fn <> "")

End Function

Function isFormLoaded(FormName) As Integer

Dim x As Integer, numberForms As Integer

numberForms = Forms.Count
isFormLoaded = False
    
For x = 0 To numberForms - 1
    If Forms(x).Name = FormName Then
        isFormLoaded = True
        Exit Function
    End If
Next x


End Function

Function isForm(FormName) As Integer
Dim db As Database, ctr As Container, n As String
On Error GoTo isForm_end

isForm = False
Set db = CurrentDb()
Set ctr = db.Containers!Forms
n = ctr.Documents(FormName).Name
isForm = True

isForm_end:
End Function

Function loadForm(DOCNAME, Optional linkcriteria = "", Optional WindowMode = 0, Optional CommandString As Variant = "") As Integer
' WindowMode = a_normal (0), a_hidden (1), a_icon (2), a_dialog (3)
' If linkCriteria is "NewRecord" then data entry mode will be used
' Also if CommandString is "New" then data entry mode will be used
' If CommandString is "Read" then read only mode will be used
Dim formMode

On Error GoTo Err_LoadForm

Select Case CommandString
    Case "Read": formMode = acFormReadOnly
    Case "New": formMode = acFormAdd
    Case Else:  formMode = Null 'acFormEdit
End Select
    
    DoCmd.SetWarnings False
    If linkcriteria = "NewRecord" Then
        DoCmd.OpenForm DOCNAME, , , , acFormAdd, WindowMode, CommandString & ""
    Else
        If IsNull(formMode) Then
            DoCmd.OpenForm DOCNAME, , , linkcriteria, , WindowMode, CommandString & ""
        Else
            DoCmd.OpenForm DOCNAME, , , linkcriteria, formMode, WindowMode, CommandString & ""
        End If
    End If
    loadForm = True

Exit_LoadForm:
    DoCmd.SetWarnings True
    Exit Function

Err_LoadForm:

Select Case Err
    Case 2501
        loadForm = True
        Resume Exit_LoadForm   ' form closed itself!

    Case 2102
        MsgBox "Sorry, function not yet available", 48
        loadForm = False
        Resume Exit_LoadForm

    Case 3024 ' missing database
        loadForm = False
        Resume Exit_LoadForm

    Case Else
        msgSystemError Err, Error$, "loadForm"
        loadForm = False
        Resume Exit_LoadForm
        Resume
End Select

End Function

Sub msgDataError(errMsg As String)

MsgBox errMsg, 48, "Data Error"

End Sub

Sub msgFormError(Msg As String)

    MsgBox Msg, 48, Screen.ActiveForm.caption

End Sub

Sub msgInfo(Msg)

    MsgBox Msg, 64, "Information"

End Sub

Sub msgSystemError(Optional ErrCode = 0, Optional errorMSG = "", Optional idText = "")

Dim msgTxt, logTxt As String
Dim errorItem As Error
Dim errMsg() As String
Dim errNo() As Long
Dim errSource() As Variant
Dim e As Long, n As Long

On Error Resume Next

ReDim errMsg(1 To DBEngine.Errors.Count)
ReDim errNo(1 To DBEngine.Errors.Count)
ReDim errSource(1 To DBEngine.Errors.Count)
e = 0
For Each errorItem In DBEngine.Errors
     e = e + 1
     With errorItem
       errNo(e) = .Number
       errMsg(e) = .Description
       errSource(e) = .Source
     End With
Next errorItem


msgTxt = "An error has occurred in the system - please call the help desk" _
        & vbNewLine & vbNewLine _
        & errorMSG _
        & vbNewLine _
        & IIf(ErrCode = 0 Or errorMSG <> "", "", vbNewLine & "Error: " & str$(ErrCode) & " = " & Error$(ErrCode))
        
logTxt = "-----------------------------------------------------------------------"
logTxt = logTxt & vbNewLine & "Date         : " & Now
logTxt = logTxt & vbNewLine & "Application  : " & CurrentDb.Properties("AppTitle")
logTxt = logTxt & vbNewLine & "MDB          : " & CurrentDb.Name
logTxt = logTxt & vbNewLine & "Login Id     : " & GetUserLogonId()
logTxt = logTxt & vbNewLine & "Computer Name: " & GetComputerName()
logTxt = logTxt & vbNewLine & "Form         : " & Screen.ActiveForm.Name
logTxt = logTxt & vbNewLine & "Object       : " & Application.CurrentObjectName
logTxt = logTxt & vbNewLine & "Control      : " & Screen.ActiveControl.Name
logTxt = logTxt & vbNewLine & "Temp space   : " & DiskSpaceMB(Environ$("temp")) & " mbytes"
logTxt = logTxt & vbNewLine & "Process id   : " & idText
logTxt = logTxt & vbNewLine & vbNewLine _
                & errorMSG _
                & vbNewLine & vbNewLine _
                & IIf(ErrCode = 0 Or errorMSG <> "", "", vbNewLine & "Error: " & str$(ErrCode) & " = " & Error$(ErrCode))


For e = 1 To UBound(errMsg)
    logTxt = logTxt & vbNewLine & "Error" & str$(errNo(e)) & " " & errMsg(e) & IIf(errSource(e) <> "0", " (From " & errSource(e) & ")", "")
Next

logTxt = logTxt & vbNewLine
writeTxt logTxt, "errorlog.txt", True
MsgBox msgTxt, 16, "System Error"



End Sub

Function navKey(KeyCode As Integer, Shift As Integer) As Integer

' call this function from an OnKeyDown event to convert
' up and down cursor key presses to previous and next record
' e.g. KeyCode = NavKey(KeyCode, Shift)
Dim ctrl As Control

On Error GoTo fini
Set ctrl = Screen.ActiveControl

If TypeOf ctrl Is ComboBox Then
    If IsComboDropped() Then
        navKey = KeyCode
        Exit Function
    End If
End If

If Shift And acCtrlMask Then
        
End If

If Shift <> 0 Then
    navKey = KeyCode
    Exit Function
End If

On Error Resume Next

navKey = 0
Select Case KeyCode
    Case KEY_DOWN:  DoCmd.GoToRecord , , A_NEXT
    Case KEY_UP:  DoCmd.GoToRecord , , A_PREVIOUS
    Case Else:  navKey = KeyCode
End Select
On Error GoTo 0
Exit Function
fini:
    navKey = KeyCode
End Function

Function dittoKey(KeyCode As Integer, Shift As Integer) As Integer

' call this function from an OnKeyDown event to convert
' Ctrl + D to copy the data from the cell above
' e.g. KeyCode = dittoKey(KeyCode, Shift)
Dim rs As Recordset, frm As Form, d As Variant, ctrl As Control

On Error GoTo fini

If Not (Shift And acCtrlMask) Then GoTo fini
If KeyCode <> vbKeyD Then GoTo fini
Set ctrl = Screen.ActiveControl
Set frm = ctrl.Parent
Set rs = frm.RecordsetClone
rs.Bookmark = frm.Bookmark
rs.MovePrevious
If Not rs.BOF Then
    d = rs(ctrl.ControlSource)
    ctrl = d
    dittoKey = 0
    DoCmd.GoToRecord , , acNext
End If
rs.Close
Set rs = Nothing
Exit Function
fini:
    dittoKey = KeyCode
End Function

Function ditto(Optional varMoveDown = True)
Dim rs As Recordset, frm As Form, d As Variant, ctrl As Control

On Error GoTo fini
' Note: Can't use the sendkeys method - it doesn't work for modal dialogue boxes
                            
Set ctrl = Screen.ActiveControl
Set frm = ctrl.Parent
Set rs = frm.RecordsetClone
If frm.NewRecord Then
    rs.MoveLast
Else
    rs.Bookmark = frm.Bookmark
    rs.MovePrevious
End If
If Not rs.BOF Then
    d = rs(ctrl.ControlSource)
    ctrl = d
    If varMoveDown = True And Not frm.NewRecord Then DoCmd.GoToRecord , , acNext
End If
rs.Close
Set rs = Nothing
Exit Function

fini:
    'Debug.Print Err, Error$
    'Stop
    'Resume

End Function

Function DittoNoMove()
SendKeys "^'"
End Function
Function nl()
nl = Chr$(13) & Chr$(10)
End Function

Function num(numVal) As Double
' treats nulls as zeroes

If isBlank(numVal) Then
    num = 0
Else
    num = numVal
End If
End Function

Function num2(numVal) As Variant
' treats nulls as zeroes
If IsNull(numVal) Or IsEmpty(numVal) Then
    num2 = Null
Else
    num2 = CDbl(numVal)
End If
End Function

Sub Capitalise(KeyAscii)
' To capitalise all letters as text is typed
' Call from  KeyPress event procedure of field to be 'capitalised'
' e.g  Capitalise KeyAscii

KeyAscii = Asc(UCase$(Chr$(KeyAscii)))


End Sub
Sub CapitaliseNoSpace(KeyAscii)
' To capitalise all letters as text is typed
' Call from  KeyPress event procedure of field to be 'capitalised'
' e.g  Capitalise KeyAscii

If Chr$(KeyAscii) = " " Then
    KeyAscii = 0
Else
    KeyAscii = Asc(UCase$(Chr$(KeyAscii)))
End If


End Sub
Sub CapitaliseFirst(KeyAscii)
' To capitalise the first letter as text is typed
' Call from  KeyPress event procedure of field to be 'capitalised'
' e.g  CapitaliseFirst KeyAscii
Dim ctrl As Control, CapChars As String
Set ctrl = Screen.ActiveControl

If ctrl.SelStart = 0 Then
    KeyAscii = Asc(UCase$(Chr$(KeyAscii)))
End If

End Sub
Sub properise(KeyAscii)

' To capitalise first letter of each word as text is typed
' Also replaces quotes with apostrophes.
' Call from  KeyPress event procedure of field to be 'properised'
' e.g  Properise KeyAscii

Dim ctrl As Control, CapChars As String

CapChars = " ,-.()'/\" & nl()

Set ctrl = Screen.ActiveControl


If ctrl.SelStart = 0 Then 'IsNull(ctrl.text) Then
    KeyAscii = Asc(UCase$(Chr$(KeyAscii)))
    Exit Sub
End If

If ctrl.SelStart > Len(ctrl.Text) Then
    KeyAscii = Asc(UCase$(Chr$(KeyAscii)))
    Exit Sub
End If

If Len(ctrl.Text) > 1 Then
    If InStr(CapChars, Right$(ctrl.Text, 1)) > 0 Then
        KeyAscii = Asc(UCase$(Chr$(KeyAscii)))
        Exit Sub
    End If
End If

' convert quotes to apostrophes

If KeyAscii = Asc("""") Then KeyAscii = Asc("'")


End Sub

Function properName(xx) As Variant
' Upper case first letters of words in X, lowercase other letters.
' If the current 'word' contains numbers, then don't lower case 2nd and subsequent characters (Allows for post codes and house numbers with letters eg.g. 19a)
    
Dim p%, x$, l As Integer, CapChars As String, p2%, hasNumbers As Boolean

CapChars = " ,-.()'/\" & nl()

If IsNull(xx) Then
    properName = Null
    Exit Function
End If

x$ = xx & ""
l = Len(x$)

Mid$(x$, 1, 1) = UCase$(Mid$(x$, 1, 1))

For p% = 2 To l

    ' is this the start of a word? i.e. the previous letter is a word break letter (but this letter is not the last letter in the string)?
    If InStr(CapChars, Mid$(x$, p% - 1, 1)) > 0 And p% < l Then
        
        hasNumbers = False
        For p2% = p% To l
            If InStr(CapChars, Mid$(x$ & " ", p2%, 1)) > 0 Then
                Exit For
            End If
            If IsNumeric(Mid$(x$, p2%, 1)) Then
                hasNumbers = True
                Exit For
            End If
        Next
        
        ' yes it is, so ucase it UNLESS it follows an apostrophe and it's the end of a word)
        If Mid$(x$, p% - 1, 1) = "'" And InStr(CapChars, Mid$(x$ & " ", p% + 1, 1)) > 0 Then
            Mid$(x$, p%, 1) = LCase$(Mid$(x$, p%, 1))
        Else
            Mid$(x$, p%, 1) = UCase$(Mid$(x$, p%, 1))
        End If
    Else
    
    ' no it isn't, so lowercase it unless the word contains numbers
        If Not hasNumbers Then Mid$(x$, p%, 1) = LCase$(Mid$(x$, p%, 1))
    End If
Next

properName = x$

End Function

Function properNameCtrl()
Dim c As Control
Set c = Screen.ActiveControl
c = properName(c)

End Function

Function replaceChars(StringToBeProcessed, StringToBeReplaced$, StringToReplaceWith$) As String
' replace all occurrences of o$ with n$ in xx
    
Dim p%, x$, y$, o$, n$
On Error GoTo ErrorHandler

If IsNull(StringToBeProcessed) Then
    replaceChars = ""
    Exit Function
End If
x$ = StringToBeProcessed
y$ = ""
o$ = StringToBeReplaced$
n$ = StringToReplaceWith$

p% = InStr(x$, o$)
Do While p% > 0
    y$ = y$ & Left$(x$, p% - 1) & n$
    x$ = Mid$(x$, p% + Len(o$))
    p% = InStr(x$, o$)
Loop

replaceChars = y$ & x$
Exit Function

ErrorHandler:
    If Err = 2427 Then
        replaceChars = ""
        Exit Function
    End If
    msgSystemError Err, Error$, "replaceChars"
    

End Function

Function replaceString(x, os, ns) As Variant

' Replace all occurrences of os with ns

Dim p As Integer, xx As String
On Error GoTo ReplaceString_err

If IsNull(x) Then
    replaceString = Null
    Exit Function
End If

xx = x
p = 1
Do
    p = InStr(p, xx, os)
    If p = 0 Then Exit Do

    xx = Left$(xx, p - 1) & ns & Mid$(xx, p + Len(os))
    p = p + Len(ns)
Loop


replaceString = xx

replaceString_ok:
    Exit Function

ReplaceString_err:
    Resume replaceString_ok

End Function

Function round(n, dPlaces) As Variant

If IsNull(n) Then
    round = Null
    Exit Function
End If

Dim dFact As Long

dFact = 10 ^ dPlaces

round = Int((n * dFact) + 0.501) / dFact

End Function

Function RsHasField(rs As Recordset, fldName) As Integer

Dim fld As field

On Error GoTo RsHasField_err

RsHasField = False
Set fld = rs.Fields(fldName)
RsHasField = True

RsHasField_exit:
Exit Function

RsHasField_err:
Resume RsHasField_exit

End Function

Function runActionQuery(SQLst As String, ID As String) As Integer

Dim ws As Workspace, db As Database
Dim sysErr, sysErrMsg

On Error GoTo runActionQuery_err

runActionQuery = False

Set ws = DBEngine(0)
Set db = ws(0)

ws.BeginTrans
db.Execute SQLst, dbSeeChanges
ws.CommitTrans

runActionQuery = True

runActionQuery_ok:

    Exit Function

runActionQuery_err:
    sysErr = Err: sysErrMsg = Error$
    writeTxt "Error Code: " & str$(sysErr) & " " & sysErrMsg & nl() & nl() & "SQL as follows..." & nl() & nl() & SQLst, "SQLDUMP.TXT", False
    msgSystemError sysErr, "SQL failed - see SQLDUMP.TXT", IIf(ID = "", "runActionQuery", ID)
    ws.Rollback
    runActionQuery = False

    Resume runActionQuery_ok

End Function

Function runActionQueryCmd(SQLst As String, ID As String) As Integer

Dim sysErr, sysErrMsg

On Error GoTo runActionQueryCmd_err

runActionQueryCmd = False

DoCmd.SetWarnings False
DoCmd.RunSQL SQLst
DoCmd.SetWarnings True

runActionQueryCmd = True

runActionQueryCmd_ok:

    Exit Function

runActionQueryCmd_err:
    sysErr = Err: sysErrMsg = Error$
    writeTxt "Error Code: " & str$(sysErr) & " " & sysErrMsg & nl() & nl() & "SQL as follows..." & nl() & nl() & SQLst, "SQLDUMP.TXT", False
    msgSystemError sysErr, "SQL failed - see SQLDUMP.TXT", IIf(ID = "", "runActionQueryCmd", ID)
    runActionQueryCmd = False

    Resume runActionQueryCmd_ok
    Resume
End Function

Function RunReport(DOCNAME, linkcriteria, mode) As Integer

' mode = A_NORMAL (0) or A_PREVIEW (2)

    On Error GoTo Err_RunReport

    RunReport = True

    DoCmd.OpenReport DOCNAME, mode, , linkcriteria

Exit_RunReport:
    Exit Function

Err_RunReport:
    If Err <> 2501 Then
        RunReport = False
        MsgBox str$(Err) & " " & Error$
    End If
    Resume Exit_RunReport
    

End Function

Function saveRecord() As Integer
On Error GoTo SaveRecord_err

DoCmd.DoMenuItem A_FORMBAR, A_FILE, A_SAVERECORD, , A_MENU_VER20
saveRecord = True

SaveRecord_exit:
    Exit Function

SaveRecord_err:
    'msgSystemError Err, "Unable to save record: " & Error$, "SaveRecord"
    saveRecord = False
    Resume SaveRecord_exit
End Function

Function scheduledStart(startDateTime, message) As Integer

' Generic scheduler
' Displays a modal dialog showing message that remains displayed
' until startDateTime is reached or cancel is clicked.
' Returns false if cancel clicked, else true.

' If startDateTime contains only a time then the  date
' will be calculated as today if time < now, tomorrow if time >= now

' Requires global variables
'   gScheduleStartdateTime as variant
'   gScheduleMessage as string
'   gScheduleCancelled as integer

Dim startAT As Variant

If Not IsDate(startDateTime) Then
    MsgBox startDateTime & " is not a valid date/time", 48, "scheduleStart"
    scheduledStart = False
    Exit Function
End If

startAT = CVDate(startDateTime)

If startAT < 1 Then
    If startAT > Time Then
        startAT = Date + startAT
    Else
        startAT = Date + 1 + startAT
    End If
End If


gScheduleStartdateTime = startAT
gScheduleMessage = message

DoCmd.OpenForm "dlgScheduler", , , , , A_DIALOG
scheduledStart = Not gScheduleCancelled

End Function

Function ScreenHeightInPixels()
ScreenHeightInPixels = GetSystemMetrics(SM_CYSCREEN)

End Function

Function ScreenHeightTwips() As Single

Dim hWnd As LongPtr
Dim hdc As LongPtr
Dim PixelsPerInchX As Long
Dim ScreenPixelsX As Long
Dim ScreenInchesX As Long

Dim rc%

'twip ...
'Unit of measurement used by Microsoft Access and implemented as 1/20 of a point, or 1/1440 of an inch.  There
'are 567 twips to a centimeter

hWnd = GetAccesshWnd()
hdc = GetDC(hWnd)
ScreenPixelsX = GetDeviceCaps(hdc, VERTRES)
PixelsPerInchX = GetDeviceCaps(hdc, LOGPIXELSX)
ScreenInchesX = ScreenPixelsX / PixelsPerInchX

ScreenHeightTwips = ScreenInchesX * 1440
rc% = ReleaseDC(hWnd, hdc)

End Function

Function ScreenWidthInPixels()

ScreenWidthInPixels = GetSystemMetrics(SM_CXSCREEN)

End Function

Function ScreenWidthTwips() As Single

Dim hWnd As LongPtr
Dim hdc As LongPtr
Dim PixelsPerInchX As Long
Dim ScreenPixelsX As Long
Dim ScreenInchesX As Long

Dim rc As Long

'twip ...
'Unit of measurement used by Microsoft Access and implemented as 1/20 of a point, or 1/1440 of an inch.  There
'are 567 twips to a centimeter

hWnd = GetAccesshWnd()
hdc = GetDC(hWnd)
ScreenPixelsX = GetDeviceCaps(hdc, HORZRES)
PixelsPerInchX = GetDeviceCaps(hdc, LOGPIXELSX)
ScreenInchesX = ScreenPixelsX / PixelsPerInchX

ScreenWidthTwips = ScreenInchesX * 1440
rc = ReleaseDC(hWnd, hdc)

End Function

Sub setFormHeight(fm As Form, HeightInches As Double)

DoCmd.MoveSize , , , 1440 * HeightInches

End Sub

Function setLimitToList(ctrl As Control, mode)

ctrl.LimitToList = mode

End Function

Function setParam(itemName As String, itemValue As Variant) As Variant
Dim iv As Variant

iv = itemValue
If varType(iv) = 8 Then 'string type
    If Left$(iv, 1) <> """" Then
        iv = SQLstring(iv)
    End If
End If

setParam = runActionQuery("update [tbl ParamsUser] set [" & itemName & "] = " & iv & ";", "setParams")

End Function

Function setProperName()

' use this in the after_update event

Dim c As Control

Set c = Screen.ActiveControl

c = properName(c.Value)

End Function

Function setSysParam(itemName As String, itemValue As Variant) As Variant
Dim iv As Variant

iv = itemValue
If varType(iv) = 8 Then 'string type
    If Left$(iv, 1) <> """" Then
        iv = SQLstring(iv)
    End If
End If

setSysParam = runActionQuery("update [tbl ParamsData] set [" & itemName & "] = " & iv & ";", "setSysParam")

End Function

Function setSysPath() As Integer

' get the database name and path, store in global vars

Dim p As Integer
Dim db As Database

setSysPath = False

Set db = DBEngine(0)(0)
g.sysPath = db.Name

p = Len(g.sysPath)
Do While p > 0 And Mid$(g.sysPath, p, 1) <> "\"
    p = p - 1
Loop
If p = 0 Then
    MsgBox "SetSysPath", , "System Error"
    Exit Function
End If

g.DBname = Mid$(g.sysPath, p + 1)
g.sysPath = Left$(g.sysPath, p)
setSysPath = True

End Function

Function ShowHelp()

Select Case Application.CurrentObjectType
    Case A_MODULE: GoTo showHelp_access
    Case A_MACRO: GoTo showHelp_access
    Case A_QUERY: GoTo showHelp_access
    Case A_TABLE: GoTo showHelp_access
    Case A_FORM: If Screen.ActiveForm.CurrentView = 0 Then GoTo showHelp_access
    Case A_REPORT: If Screen.ActiveReport.CurrentView = 0 Then GoTo showHelp_access
End Select


RetVal = Shell("winhelp " & SystemPath() & gHelpFileName, 1)

Exit Function

showHelp_access:
    DoCmd.DoMenuItem 1, 5, 0, , A_MENU_VER20
    Exit Function

End Function

Function SQLdate(dateVar As Variant) As String
' Takes a date in local format, returns "#mm/dd/yyyy#" format

Dim ddmmyy As Variant



If varType(dateVar) = V_DATE Then
    ddmmyy = dateVar
Else
    ddmmyy = DateValue(dateVar)
End If

If Not IsDate(ddmmyy) Then
    msgSystemError 0, Error$, "SQLdate: " & ddmmyy & " is not a date"
    SQLdate = ""
    Exit Function
End If

' convert date to mm/dd/yyyy

SQLdate = "#" & Format(ddmmyy, "mm\/dd\/yyyy") & "#"


End Function

Function SQLtime(dateVar As Variant) As String
' Takes a date in local format, returns "#mm/dd/yyyy#" format

Dim ddmmyy As Variant



If varType(dateVar) = V_DATE Then
    ddmmyy = dateVar
Else
    ddmmyy = TimeValue(dateVar)
End If

If Not IsDate(ddmmyy) Then
    msgSystemError 0, Error$, "SQLtime: " & ddmmyy & " is not a date"
    SQLtime = ""
    Exit Function
End If

' convert date to mm/dd/yyyy

SQLtime = "#" & Format(ddmmyy, "hh:nn:ss") & "#"


End Function

Function SQLdateTime(dateVar As Variant) As String
' Takes a date in local format, returns "#mm/dd/yyyy hh:nn:ss#" format

Dim ddmmyy As Variant



If varType(dateVar) = V_DATE Then
    ddmmyy = dateVar
Else
    ddmmyy = DateValue(dateVar) + TimeValue(dateVar)
End If

If Not IsDate(ddmmyy) Then
    msgSystemError 0, Error$, "SQLdateTime: " & ddmmyy & " is not a date"
    SQLdateTime = ""
    Exit Function
End If

' convert date to mm/dd/yyyy

SQLdateTime = "#" & Format(ddmmyy, "mm\/dd\/yyyy hh:nn:ss") & "#"


End Function

Function SQLstring(xx, Optional blnAllowNull = False, Optional strDelimiter As String = """") As String
'strDelimiter must be single or double quotes

Dim x As String

On Error Resume Next

x = ""
x = xx

If IsNull(xx) Then
    If blnAllowNull Then
        SQLstring = "NULL"
        Exit Function
    Else
        SQLstring = strDelimiter & strDelimiter
    End If
    Exit Function
End If

' replace any delimeters in string with pairs of delimiters
x = replaceChars(xx, strDelimiter, strDelimiter & strDelimiter)

SQLstring = strDelimiter & x & "" & strDelimiter

End Function
Function sq(sqtype As String, data, Optional MAXLEN As Variant) As String
' generates strings for sql
' sqtype = "s" string, "d" date, "n" number
Dim x As String

If IsNull(data) Then
    x = "NULL"
Else
    Select Case sqtype
        Case "s":
            If Not IsMissing(MAXLEN) Then
                x = SQLstring(Left$(data, MAXLEN))
            Else
                x = SQLstring(data)
            End If
        Case "d":   x = SQLdate(data)
        Case "n":
            If IsNumeric(data) Then
                x = Format(data)
                If x = "" Then
                    x = "NULL"
                End If
            Else
                If Not IsMissing(MAXLEN) Then
                    x = SQLstring(Left$(data, MAXLEN))
                Else
                    x = SQLstring(data)
                End If
            End If
    End Select

End If

sq = x

End Function

Function sq2(sqtype As String, data, Optional MAXLEN As Variant) As String
' as sq, but adds a "," to end

If IsMissing(MAXLEN) Then
    sq2 = sq(sqtype, data) & ", "
Else
    sq2 = sq(sqtype, data, MAXLEN) & ", "
End If

End Function

Function sq3(fName As String, sqtype As String, data, Optional MAXLEN As Variant) As String
' generates a '[fieldname] = value, ' string for making SQL update statements

sq3 = "[" & fName & "] = " & sq2(sqtype, data, MAXLEN)

End Function

Sub StatusReset()
On Error Resume Next

RetVal = SysCmd(SYSCMD_CLEARSTATUS)

End Sub

Static Function stopWatch(watchNo, mode As String) As Double

' You can have up to 10 stopwatches running simultaneously.

' mode = "start" to start a watch, returns 0
' mode = "stop" to stop a watch, returns elapsed time and stops watch
' mode = "read" to read a watch, returns elapsed time so far, watch stays running
' mode = "lap" to read a watch, returns elapsed time since start or last lap, watch stays running


Dim timerStart(1 To 10) As Double
Dim timerStore(1 To 10) As Double
Dim timerRunning(1 To 10) As Integer
Dim laptime(1 To 10) As Double

Select Case mode
    Case "start":
        timerStart(watchNo) = Timer
        timerStore(watchNo) = 0
        laptime(watchNo) = timerStart(watchNo)
        timerRunning(watchNo) = True
        stopWatch = 0

    Case "stop":
        If timerRunning(watchNo) Then
            timerStore(watchNo) = timerStore(watchNo) + Timer - timerStart(watchNo)
            timerRunning(watchNo) = False
        End If
        stopWatch = timerStore(watchNo)
    
    Case "read":
        If timerRunning(watchNo) Then
            stopWatch = timerStore(watchNo) + Timer - timerStart(watchNo)
        Else
            stopWatch = timerStore(watchNo)
        End If

    Case "lap":
        If timerRunning(watchNo) Then
            stopWatch = Timer - laptime(watchNo)
            laptime(watchNo) = Timer
        Else
            stopWatch = timerStore(watchNo) - (laptime(watchNo) - timerStart(watchNo))
        End If

End Select

End Function

Function strip(ipdata) As Variant
Dim xIn As Variant, xOut As Variant, x As String
Dim p As Integer

If IsNull(ipdata) Then
    strip = Null
    Exit Function
End If

xIn = ipdata
For p = 1 To Len(xIn)
    x = Mid$(xIn, p, 1)
    If InStr(stripChars, x) > 0 Then
        xOut = xOut & x
    End If
Next p

strip = xOut

End Function

Function SystemPath()

' Returns the full path name of the .MDB of the default database

SystemPath = ""

If g.sysPath & "" = "" Then
    If Not setSysPath() Then
        Exit Function
    End If
End If
SystemPath = g.sysPath

End Function

Function toolbars(mode)

Application.SetOption "Built-in Toolbars Available", mode

End Function

Sub SetAccessCaption(caption As String)
  
SetAppProperty "AppTitle", dbText, caption
RefreshTitleBar

End Sub


Function SetAppProperty(strName As String, varType As Variant, varValue As Variant) As Integer
    Dim dbs As Database, prp As Property
    Const conPropNotFoundError = 3270

    Set dbs = CurrentDb
    On Error GoTo SetAppProperty_Err
    dbs.Properties(strName) = varValue

SetAppProperty = True

SetAppProperty_Bye:
    Exit Function

SetAppProperty_Err:
    If Err = conPropNotFoundError Then
        Set prp = dbs.CreateProperty(strName, varType, varValue)
        dbs.Properties.Append prp
        Resume
    Else
        SetAppProperty = False
        Resume SetAppProperty_Bye
    End If
End Function

Function undorecord()

' undo changes to active control
On Error Resume Next
Err = 0
DoCmd.RunCommand acCmdUndo
'MsgBox Error$
undorecord = (Err = 0)


End Function

Function ControlLabel(ctrl) As Variant
Dim c As Control

ControlLabel = Null

For Each c In ctrl.Controls
    If TypeOf c Is Label Then
        ControlLabel = c.caption
        Exit For
    End If
Next


End Function


Function validate(ctrl As Control, validation As String, Optional ipCtrlName = Null, Optional depCtrl As Control, Optional depCtrlName) As Integer

' validates data as required,
' if validation fails, displays message, sets focus and returns false
'
' validation string can contain any of...
'
'  r    required
'  n    numeric
'  p    numeric and positive
'  d    date
'  b    blank
'  ix   'include' cross-check - ctrl required if depCtrl not null
'  ex   'exclude' cross-check - ctrl not allowed if depCtrl null


Dim eMsg As String, lf As String, ctrlName As Variant

validate = True

ctrlName = Nz(ipCtrlName, ControlLabel(ctrl))

If InStr(validation, "r") Then
    If isBlank(ctrl) Then
        eMsg = eMsg & lf & ctrlName & " required"
        lf = nl()
    End If
End If

If Not isBlank(ctrl) Then
    If InStr(validation, "b") Then
        If Not isBlank(ctrl) Then
            eMsg = eMsg & lf & ctrlName & " must be blank"
            lf = nl()
        End If
    End If
    
    If InStr(validation, "n") Then
        If Not isBlank(ctrl) Then
            If Not IsNumeric(ctrl) Then
                eMsg = eMsg & lf & ctrlName & " must be numeric"
                lf = nl()
            End If
        End If
    End If
    
    If InStr(validation, "p") Then
        If IsNumeric(ctrl) Then
            If ctrl < 0 Then
                eMsg = eMsg & lf & ctrlName & " must be numeric and positive"
                lf = nl()
            End If
        End If
    End If
    
    If InStr(validation, "d") Then
        If Not IsDate(ctrl) Then
            eMsg = eMsg & lf & ctrlName & " must be a valid date"
            lf = nl()
        Else
            If ctrl < #1/1/1900# Or ctrl > #12/31/2999# Then
                eMsg = eMsg & lf & ctrlName & " must be a date between 1 Jan 1900 and 31 Dec 2999"
                lf = nl()
            End If
        End If
    End If
    
End If

' cross checks
If InStr(validation, "ix") Then
    If IsNull(ctrl) And Not IsNull(depCtrl) Then
        eMsg = eMsg & lf & ctrlName & " required if " & depCtrlName & " is blank"
        lf = nl()
    End If
End If

If InStr(validation, "ex") Then
    If Not IsNull(ctrl) And IsNull(depCtrl) Then
        eMsg = eMsg & lf & ctrlName & " not allowed if " & depCtrlName & " is not blank"
        lf = nl()
    End If
End If

If eMsg <> "" Then
    validate = False
    If ctrl.Enabled And ctrl.Visible Then
        On Error Resume Next
        ctrl.SetFocus
    End If
    msgDataError eMsg
End If

End Function

Sub waitDialog(Msg)

If isBlank(Msg) Then
    DoCmd.Close A_FORM, "dlgMessage"
Else
    DoCmd.OpenForm "dlgMessage", , , , , , Msg
End If

End Sub

Sub warnings(TrueOrFalse As Integer)
DoCmd.SetWarnings TrueOrFalse

End Sub

Sub writeTxt(txt, pFn As String, appendTxt As Integer)

' writes the contents of variable txt to the named file
' if appendTXT then append if file exists

Dim f As Integer, fileExists As Integer, fn

If InStr(pFn, "\") = 0 Then
    fn = SystemPath() & pFn
Else
    fn = pFn
End If

fileExists = (Dir(fn) <> "")

f = FreeFile

If fileExists And appendTxt Then
    Open fn For Append As f
Else
    Open fn For Output As f
End If

Print #f, txt

Close #f

End Sub

Function readTxt(TextFileName As String) As Variant

' reads the contents of the named file and returns it

Dim f As Integer, txt As String, rec As String, fn

fn = TextFileName

If Not isFile(fn) Then
    readTxt = "File " & fn & " not found!"
    Exit Function
End If

txt = ""

f = FreeFile

Open fn For Input As f

Do While Not EOF(f)
    Line Input #f, rec
    txt = txt & rec & nl()
Loop

Close #f
readTxt = txt

End Function

Function zoom(maxLength As Long, Title)
    DoCmd.OpenForm "Zoom", , , , , A_DIALOG, str$(maxLength) & Chr$(255) & Title

End Function

Function zoomBox()
' opens a standard Access zoom box for viewing/entering data = Shift F2
On Error Resume Next
    DoCmd.RunCommand acCmdZoomBox
End Function

Function getObjectNames()

Dim n As Integer, p As Integer, ctrl As Control, objType As String, objName  As String, x As Variant
Dim frm As Form, rpt As Report
Dim prefix As String, suffix As String


If Application.CurrentObjectType <> A_MODULE Then
    MsgBox "No active form module - active object is " & Application.CurrentObjectName
    Exit Function
End If

x = InputBox$("Enter prefix", , "")
'x = replaceString(x, "[", "{[}")
'x = replaceString(x, "]", "{]}")
'x = replaceString(x, "+", "{+}")
'x = replaceString(x, "^", "{^}")
'x = replaceString(x, "%", "{%}")
'x = replaceString(x, "~", "{~}")
'x = replaceString(x, "(", "{(}")
'x = replaceString(x, "(", "{)}")

prefix = x

x = InputBox$("Enter suffix", , "")
'x = replaceString(x, "[", "{[}")
'x = replaceString(x, "]", "{]}")
'x = replaceString(x, "+", "{+}")
'x = replaceString(x, "^", "{^}")
'x = replaceString(x, "%", "{%}")
'x = replaceString(x, "~", "{~}")
'x = replaceString(x, "(", "{(}")
'x = replaceString(x, "(", "{)}")

suffix = x

objName = Application.CurrentObjectName
p = InStr(objName, "_")
objType = Left$(objName, p - 1)
objName = Mid$(objName, p + 1)
Select Case objType
    Case "Form":
        Set frm = Forms(objName)
        n = frm.Count
        On Error GoTo getFormObjectNames_badControl
        
        For p = 0 To n - 1
            Set ctrl = frm(p)
            x = ctrl.ControlSource
            Debug.Print prefix & ctrl.Name & suffix
getFormObjectNames_next:
        Next
        Exit Function
    
    Case "Report":
        Set rpt = Reports(objName)
        n = rpt.Count
        On Error GoTo getFormObjectNames_badControl
        
        For p = 0 To n - 1
            Set ctrl = rpt(p)
            x = ctrl.ControlSource
            Debug.Print prefix & ctrl.Name & suffix
getReportObjectNames_next:
        Next
        Exit Function
    
    Case Else:
        MsgBox "No active form or report"
        Exit Function
End Select

getFormObjectNames_badControl:
    If objType = "Form" Then
        Resume getFormObjectNames_next
    Else
        Resume getReportObjectNames_next
    End If

End Function


Public Function setCursorPos()
' places the cursor for the current control at pos 1

On Error GoTo setCursorPos_exit

Dim ctrl As Control
Set ctrl = Screen.ActiveControl
ctrl.SelStart = 1


setCursorPos_exit:

End Function

Public Function ResolveReferences(arg As Variant) As String
' Arg may contain references to system 'settings' (held in the Settings table)
' and these must be replaced by the specified setting's value.
' References to settings will be held in the form "[settingName]"

Dim ps As Integer, pe As Integer, xIn As String, xOut As String, settingName As String, settingValue As Variant

On Error GoTo ResolveReferences_err

If IsNull(arg) Then
    ResolveReferences = ""
    Exit Function
End If

xIn = arg
xOut = ""

Do While xIn <> ""
    ps = InStr(xIn, "[")
    If ps = 0 Then
        xOut = xOut & xIn
        Exit Do
    End If
    pe = InStr(xIn, "]")
    If pe = 0 Then
        xOut = xOut & xIn
        Exit Do
    End If
    
    settingName = Mid$(xIn, ps + 1, pe - ps - 1)
    settingValue = SettingsGet(settingName)
    
    If IsNull(settingValue) And settingName = "temp" Then
        settingValue = Environ("temp")
        If Right$(settingValue, 1) = "\" Then
            settingValue = Left$(settingValue, Len(settingValue) - 1)
        End If
    End If
    
    If IsNull(settingValue) Then
        
        xOut = xOut & Left$(xIn, pe)
    Else
        xOut = xOut & Left$(xIn, ps - 1)
        xOut = xOut & settingValue
    End If
    
    xIn = Mid$(xIn, pe + 1)

Loop

ResolveReferences = xOut
Exit Function

ResolveReferences_err:
    msgSystemError Err, Error$, "ResolveReferences"
    ResolveReferences = ""
    
End Function


Public Function mnuPrint()
On Error Resume Next

DoCmd.RunCommand acCmdPrint
End Function


Public Function MemFree() As Variant
Dim mem As LongPtr, r As LongPtr

r = GlobalFree(mem)
If r Then
    MemFree = mem
End If
End Function

Public Function getCommandLine()
getCommandLine = Command$
End Function

Public Function DiskSpaceMB(fullPath) As Double
Dim lpRootPathName As String, lpSectorsPerCluster As Long, lpBytesPerSector As Long, lpNumberOfFreeClusters As Long, lpTotalNumberOfClusters As Long
Dim n As Double
Dim RootPathName As String

' If fullPath is like x:\pppp.... then x:\ is needed
' If fullpath is unc like \\computername\pppp... then \\computername\x$ is needed
' Won't bother about that just now!!!

RootPathName = Left$(fullPath, 2) & "\"

If GetDiskFreeSpace(RootPathName, lpSectorsPerCluster, lpBytesPerSector, lpNumberOfFreeClusters, lpTotalNumberOfClusters) Then
    n = lpNumberOfFreeClusters
    n = n * lpSectorsPerCluster
    n = n * lpBytesPerSector
    n = n / 1024#
    n = n / 1024#
    DiskSpaceMB = round(n, 2)
End If
End Function

Public Function getFileNames(targetTable, SearchPath, FileSpecification, Optional IncludePathName As Integer = False) As Long
' Populates a table with filenames found in the
' specified searchpath with the specified file specification.
' the table MUST have a field named "FileName" and it will be emptied first.

Dim i As Long, rs As Recordset, db As Database, p As Integer, fn As String
getFileNames = 0

If Dir(SearchPath, vbDirectory) = "" Then
    Exit Function
End If

With Application.FileSearch
    .NewSearch
    .LookIn = SearchPath
    .FileName = FileSpecification
    .SearchSubFolders = False
    .MatchTextExactly = False

    If .Execute() > 0 Then
        getFileNames = .FoundFiles.Count
        Set db = CurrentDb()
        db.Execute "delete * from [" & targetTable & "];"
        Set rs = db.OpenRecordset(targetTable)
        For i = 1 To .FoundFiles.Count
            rs.AddNew
            fn = .FoundFiles(i)
            If Not IncludePathName Then
                p = Len(fn)
                Do While p > 0 And Mid$(fn, p, 1) <> "\" And Mid$(fn, p) <> ":"
                    p = p - 1
                Loop
                fn = Mid$(fn, p + 1)
            End If
            rs!FileName = fn
            rs.Update
        Next i
        rs.Close
        Set rs = Nothing
        Set db = Nothing
    End If
    
End With


End Function

Public Function GetComputerName() As String
'Returns computer name if successful, empty string otherwise.

Dim strComputerName As String, lngNameSize As Long, lngRtn As Long

On Error GoTo Err_GetComputerName

GetComputerName = ""

strComputerName = Space$(16)

lngNameSize = Len(strComputerName)

lngRtn = wapi_GetComputerName(strComputerName, lngNameSize)

If lngRtn <> 0 Then
   GetComputerName = Left$(strComputerName, lngNameSize)
Else
   GetComputerName = ""
End If

Err_GetComputerName:
Exit Function

End Function
Public Function GetUserID() As String
'Aim: Get currently logged on NT userid
'Returns NT userid if successful, empty string otherwise.

Dim strNBuffer As String
Dim lngBuffsize As Long
Dim lngWok As Long
Dim strwaste As String
   
On Error GoTo Err_GetUserID

GetUserID = ""

lngBuffsize = 256
strNBuffer = Space$(lngBuffsize)

lngWok = getUserName(strNBuffer, lngBuffsize)
strwaste = Trim$(strNBuffer)

' Remove end character
GetUserID = Left(strwaste, Len(strwaste) - 1)
    
Exit_GetUserID:
   Exit Function

Err_GetUserID:
   msgSystemError Err.Number, Error$, "GetUserID"
   GetUserID = ""
   GoTo Exit_GetUserID
End Function


Function getNextFNseq(path, pattern) As String

' Pattern must be like "x??????.TXT"
' The number of "?" in the pattern will determine the seq no. size
' If no files exist with that pattern, then seq 1 will be returned,
' e.g. "000001"

Dim seq As Long, fSeq As Long, seqSize As Integer, dPath As String, fn As String
Dim prefix As String, suffix As String

Dim p1 As Integer, p2 As Integer

If Not isDir(path) Then
    getNextFNseq = "Error: Path '" & path & "' not found"
    Exit Function
End If

p1 = InStr(pattern, "?")
If p1 = 0 Then
    getNextFNseq = "Error: No '?' in pattern"
    Exit Function
End If

prefix = Left$(pattern, p1 - 1)

p2 = InStr(pattern, ".")
If p2 = 0 Then
    getNextFNseq = "Error: No '.' in pattern"
    Exit Function
End If

seqSize = p2 - p1

suffix = Mid$(pattern, p2)


If Mid$(pattern, p1, seqSize) <> String$(seqSize, "?") Then
    getNextFNseq = "Error: No '" & String$(seqSize, "?") & "' in pattern"
    Exit Function
End If

dPath = path & pattern
seq = 0

fn = Dir(dPath)
Do While fn <> ""
    fSeq = Val(Mid$(fn, p1, seqSize))
    If fSeq > seq Then
        seq = fSeq
    End If
    fn = Dir
Loop

seq = seq + 1
If Len(Format(seq)) > seqSize Then
    seq = 1
End If

getNextFNseq = Format(seq, String$(p2 - p1, "0"))

End Function
Public Function pathName(p As String) As String
' ensures that p is a valid pathname

Dim x
If p = "" Then pathName = "": Exit Function
x = p
If Right$(x, 1) <> "\" Then
    x = x & "\"
End If
pathName = x

End Function

Function textControl(k As Integer, l As Integer) As Integer

' control text entry for the active control by limiting length, capitalising first letter
' and converting quotes to apostrophes
'
' Use in KeyPress action e.g.
'
'   keyAscii = textControl(keyAscii, 30)
'

Dim KeyAscii As Integer, ctrl As Control

Set ctrl = Screen.ActiveControl

KeyAscii = k

If Len(ctrl.Text & "") > l - 1 And KeyAscii <> 8 And KeyAscii <> 127 Then
    textControl = 0
    Beep
    Exit Function
End If

' Capitalise first letter

If ctrl.Text & "" = "" Then
    KeyAscii = Asc(UCase$(Chr$(KeyAscii)))
End If

' convert quotes to apostrophes

If KeyAscii = Asc("""") Then KeyAscii = Asc("'")

textControl = KeyAscii

End Function


Public Function maxLength(maximumLength As Long)

' To inhibit typing if data in current control exceeds stated maximum length
' set as event property for onChange event procedure of control
' e.g  =maxLength(255)

If maximumLength < 1 Then
    Exit Function
End If

Dim ctrl As Control, l As Long

Set ctrl = Screen.ActiveControl
If Len(ctrl.Text) > maximumLength Then
    'MsgBox "You have attempted to exceed the maximum length of " & CStr(maximumLength) & " characters.", vbOKOnly, "Data error"
    Beep
    Do While Len(ctrl.Text) > maximumLength
        l = Len(ctrl.Text)
        SendKeys "{Backspace}"
        If Len(ctrl.Text) = l Then
            Exit Do
        End If
    Loop
End If

End Function

Public Function getFieldList(strTableName As String) As String

' returns a comma-delimited list of fields in a table definition

Dim db As Database, tdef As TableDef, fld As field
Dim strList As String

On Error GoTo ErrorHandler

Set db = CurrentDb()
Set tdef = db.TableDefs(strTableName)
For Each fld In tdef.Fields
    strList = strList & IIf(strList = "", "", ",") & fld.Name
Next

getFieldList = strList

Set db = Nothing
Exit Function

ErrorHandler:
    Select Case Err
        Case 3265
            MsgBox "No such table as " & strTableName, vbCritical, "getFieldList"
            
        Case Else
            MsgBox "Error " & str$(Err) & ": " & Error$, vbCritical, "getFieldList"
            
    End Select
    Exit Function
End Function

Static Function SettingsGet(itemName As String, Optional Default As Variant) As Variant
' If RunMode = "Test" in tbl Settings, this function
' will return the values from the TestSetting column if present (or the Setting column if TestSetting is null),

Dim Setting As Variant, blnInitialised As Variant
Dim rs As Recordset
Dim ps As Integer, pe As Integer, bracketCount As Integer, x As String, subSetting As Variant
Dim strParentPath As String, blnIsTesting As Boolean

If Not blnInitialised Then
    blnInitialised = True
    Set rs = CurrentDb.OpenRecordset("tbl Settings", dbOpenDynaset)
End If
rs.Requery
rs.FindFirst "Id = ""RunMode"""
If rs.NoMatch Then
    blnIsTesting = False
Else
    blnIsTesting = (rs!Setting = "Test")
End If

rs.FindFirst "Id = " & SQLstring(itemName)

If rs.NoMatch Then
    If IsMissing(Default) Then
        Setting = Null
    Else
        Setting = Default
    End If
Else
    If blnIsTesting And Not IsNull(rs!TestSetting) Then
        Setting = expandedText(rs!TestSetting)
    Else
        Setting = expandedText(rs!Setting)
    End If
End If



' check for recursive references to another setting (i.e. the setting contains a "[xxxx]" reference
Do While InStr(Setting, "[") > 0
    ps = InStr(Setting, "[")
    If ps = 0 Then
        SettingsGet = Setting
        Exit Function
    End If
    
    ' found a reference - look for the matching "]"
    
    pe = ps + 1
    bracketCount = 1
    Do While pe <= Len(Setting)
        x = Mid$(Setting, pe, 1)
        Select Case x
            Case "]":   bracketCount = bracketCount - 1
            Case "[":   bracketCount = bracketCount + 1
        End Select
        If bracketCount = 0 Then
            subSetting = SettingsGet(Mid$(Setting, ps + 1, pe - (ps + 1))) & ""
            Setting = Left$(Setting, ps - 1) & subSetting & Mid$(Setting, (pe + 1))
            Exit Do
        End If
        pe = pe + 1
    Loop
Loop

strParentPath = getParentPath(SystemPath())
If Right$(strParentPath, 1) = "\" Then strParentPath = Left$(strParentPath, Len(strParentPath) - 1)
Setting = replaceString(Setting, "ParentPath$", strParentPath)
Setting = replaceString(Setting, "AppPath$", SystemPath())

SettingsGet = Setting

End Function

Function SettingsSet(itemName As String, itemValue As Variant)
Dim iv As Variant

iv = itemValue
If varType(iv) = 8 Then 'string type
    If Left$(iv, 1) <> """" Then
        iv = SQLstring(iv)
    End If
End If

If DCount("*", "tbl settings", "id = " & SQLstring(itemName)) = 0 Then
    runActionQuery "INSERT INTO [tbl Settings] ( id ) values (" & SQLstring(itemName) & ");", "SettingsSet"
End If
If runActionQuery("UPDATE [tbl Settings] SET setting = " & iv & " where Id = " & SQLstring(itemName) & ";", "SettingsSet") Then
End If

End Function


Function expandedText(xxx) As String
' xxx may contain "AppPath$" which must be replaced with the
' path of the executing database,

On Error GoTo expandedText_err

Dim n$, cLine$, parm$
Dim p%, ap%, winTempPath As String

Dim sysDir As String, pacsDir As String

sysDir = SystemPath()
If Right$(sysDir, 1) = "\" Then
    sysDir = Left$(sysDir, Len(sysDir) - 1)
End If

cLine$ = xxx & ""

' replace occurrences of appPath$

parm$ = "AppPath$"
ap% = InStr(cLine$, parm$)
Do While ap% > 0
    cLine$ = replaceString(cLine$, parm$, sysDir)
    ap% = InStr(cLine$, parm$)
Loop

' replace occurrences of appath$

parm$ = "Appath$"
ap% = InStr(cLine$, parm$)
Do While ap% > 0
    cLine$ = replaceString(cLine$, parm$, sysDir)
    ap% = InStr(cLine$, parm$)
Loop

' replace occurrences of wintemp$

parm$ = "wintemp$"
ap% = InStr(cLine$, parm$)
winTempPath = Environ("temp")
If Right$(winTempPath, 1) = "\" Then
    winTempPath = Left$(winTempPath, Len(winTempPath) - 1)
End If

Do While ap% > 0
    cLine$ = replaceString(cLine$, parm$, winTempPath)
    ap% = InStr(cLine$, parm$)
Loop


expandedText = cLine$

expandedText_ok:
    Exit Function


expandedText_err:
    MsgBox str$(Err) & ": " & Error$
    Resume expandedText_ok

End Function


Function closeAllForms()
Dim db As Database, p As Integer
Dim fm As Form

Do While Forms.Count > 0
    DoCmd.Close A_FORM, Forms(0).Name
Loop

End Function



Function closeAllReports()
Dim db As Database, p As Integer
Dim fm As Form

Do While Reports.Count > 0
    DoCmd.Close A_REPORT, Reports(0).Name
Loop

End Function

Function compileAllForms()
    Dim DefaultWorkspace As Workspace
    Dim CurrentDatabase As Database
    Dim MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String

    Set DefaultWorkspace = DBEngine.Workspaces(0)
    Set CurrentDatabase = DefaultWorkspace.Databases(0)

    For j = 0 To CurrentDatabase.Containers.Count - 1
        Set MyContainer = CurrentDatabase.Containers(j)
        If MyContainer.Name = "Forms" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                If Left$(fm, 1) <> "z" Then
                    DoCmd.OpenForm fm, A_DESIGN, , , , A_NORMAL 'A_HIDDEN?
                    DoCmd.RunMacro "compile"
                    DoCmd.Close A_FORM, fm
                End If
            Next i
        End If
    Next j
End Function

Function loadAllForms()

    Dim DefaultWorkspace As Workspace
    Dim CurrentDatabase As Database
    Dim MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String, searchText As String

    searchText = InputBox("Enter prefix of form names to be loaded" & nl() & "(leave blank for all, enter 'Q' to quit)", "Search All")
    If searchText = "Q" Then Exit Function
    
    Set DefaultWorkspace = DBEngine.Workspaces(0)
    Set CurrentDatabase = DefaultWorkspace.Databases(0)

    For j = 0 To CurrentDatabase.Containers.Count - 1
        Set MyContainer = CurrentDatabase.Containers(j)
        If MyContainer.Name = "Forms" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                If searchText = "" Or Left$(fm, Len(searchText)) = searchText Then
                    If Left$(fm, 1) <> "z" Then
                        DoCmd.OpenForm fm, A_DESIGN, , , , A_ICON
                    End If
                End If
            Next i
        End If
    Next j
End Function

Function loadAllReports()

    Dim DefaultWorkspace As Workspace
    Dim CurrentDatabase As Database
    Dim MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String, searchText As String

    searchText = InputBox("Enter prefix of report names to be loaded" & nl() & "(leave blank for all, enter 'Q' to quit)", "Search All")
    If searchText = "Q" Then Exit Function
    
    Set DefaultWorkspace = DBEngine.Workspaces(0)
    Set CurrentDatabase = DefaultWorkspace.Databases(0)

    For j = 0 To CurrentDatabase.Containers.Count - 1
        Set MyContainer = CurrentDatabase.Containers(j)
        If MyContainer.Name = "Reports" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                If searchText = "" Or Left$(fm, Len(searchText)) = searchText Then
                    If Left$(fm, 1) <> "z" Then
                        DoCmd.OpenReport fm, A_DESIGN
                    End If
                End If
            Next i
        End If
    Next j
End Function

Function searchAllForms()

    Dim DefaultWorkspace As Workspace
    Dim CurrentDatabase As Database
    Dim MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String, searchText As String

    searchText = InputBox("Enter search text" & nl() & "(leave blank to terminate)", "Search All")
    If searchText = "" Then Exit Function

    Set DefaultWorkspace = DBEngine.Workspaces(0)
    Set CurrentDatabase = DefaultWorkspace.Databases(0)

    For j = 0 To CurrentDatabase.Containers.Count - 1
        Set MyContainer = CurrentDatabase.Containers(j)
        If MyContainer.Name = "Forms" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                If Left$(fm, 1) <> "z" Then
                    DoCmd.OpenForm fm, A_DESIGN, , , , A_NORMAL 'A_HIDDEN?
                    ''' HOW TO SEARCH FOR TEXT STRING!!!
                    DoCmd.RunMacro "ViewCode"
                    SendKeys searchText
                    DoCmd.RunMacro "Find"
                    DoCmd.Close A_FORM, fm
                End If
            Next i
        End If
    Next j
End Function

Function searchAllQueries(searchText)

    Dim DefaultWorkspace As Workspace
    Dim db As Database
    Dim MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String
    Dim qdef As QueryDef, rs As Recordset

    If searchText & "" = "" Then
        searchText = InputBox("Query Search: Enter search text" & nl() & "(leave blank to terminate)", "Search All")
    End If
    If searchText = "" Then Exit Function

    Set db = CurrentDb()
    emptyTable "SearchResults"
    Set rs = db.OpenRecordset("SearchResults")
    With rs
        For Each qdef In db.QueryDefs
            If InStr(qdef.sql, searchText) > 1 Then
                .AddNew
                !ObjectName = qdef.Name
                !ObjectType = "qry"
                !Text = qdef.sql
                .Update
            End If
        Next
        .Close
    End With
    
    
End Function

Public Function FormErrorHandler(DataErr As Integer, Response As Integer)
Dim varErr As Error, strMsg As String
Select Case DataErr
    Case 3146: Response = acDataErrContinue
    Case 3022: msgFormError "The item already exists - it may not be added again" & vbNewLine & "Press the Esc key to undo": Response = acDataErrContinue
    Case 3200: msgFormError "This data item cannot be deleted as it is referred to by other data": Response = acDataErrContinue
    Case Else
End Select

End Function

Public Sub ClearMultiSelect(ctrl As Control)
Dim varItem As Variant
For Each varItem In ctrl.ItemsSelected
    ctrl.Selected(varItem) = False
Next
End Sub

Public Function RequeryAllLists(strFormname As String)
' Finds all conbos and lists in the specified form
' and requeries them

Dim frm As Form, ctrl As Control

Set frm = Forms(strFormname)
If frm.CurrentView = 0 Then Exit Function ' form is in design view
For Each ctrl In frm.Controls
    If TypeOf ctrl Is ComboBox _
    Or TypeOf ctrl Is ListBox Then
        ctrl.Requery
    End If
Next

End Function

Public Function FormPositionSet()
' stores x,y of active form in tblSettings
Dim strFormname As String, frm As Form
Set frm = Screen.ActiveForm
strFormname = frm.Name
SettingsSet strFormname & "_XPOS", getFormLeftInTwips(frm)
SettingsSet strFormname & "_YPOS", getFormTopInTwips(frm)

End Function

Public Function FormPositionGet(Optional varFormName As Variant)
Dim strFormname As String, dblXposCM As Double, dblYposCM As Double

If IsMissing(varFormName) Then
    strFormname = Screen.ActiveForm.Name
Else
    strFormname = varFormName
End If

dblXposCM = SettingsGet(strFormname & "_XPOS") & ""
dblYposCM = SettingsGet(strFormname & "_YPOS") & ""
DoCmd.MoveSize dblXposCM, dblYposCM
End Function
Function ChangeProperty(strPropName As String, varPropType As Variant, varPropValue As Variant) As Integer
    Dim dbs As Database, prp As Property
    Const conPropNotFoundError = 3270

    Set dbs = CurrentDb
    On Error GoTo Change_Err
    dbs.Properties(strPropName) = varPropValue
    ChangeProperty = True

Change_Bye:
    Exit Function

Change_Err:
    If Err = conPropNotFoundError Then  ' Property not found.
        Set prp = dbs.CreateProperty(strPropName, varPropType, varPropValue)
        dbs.Properties.Append prp
        Resume Next
    Else
        ' Unknown error.
        ChangeProperty = False
        Resume Change_Bye
    End If
End Function

Public Function AutoDropCombo(Optional varKeyAscii As Variant)
' call this function from an OnKeyPress event to drop a combo when a key is pressed
' e.g. AutoDropCombo KeyAscii
On Error GoTo exitFunction
If TypeOf Screen.ActiveControl Is ComboBox Then
    If IsMissing(varKeyAscii) Then
           Screen.ActiveControl.Dropdown
    Else
        Select Case varKeyAscii
            Case KEY_RETURN, KEY_ESCAPE, KEY_TAB
            Case Else
                Screen.ActiveControl.Dropdown
        End Select
    End If
End If

exitFunction:

End Function

Function GetUserLogonId() As String

Dim strName As String * 128
Dim lngLength As Long
Dim tmp As Long

lngLength = 128

getUserName strName, lngLength

GetUserLogonId = Left(strName, lngLength - 1)

End Function

Public Function VersionNo()
VersionNo = DMax("Version", "tbl Versions", "")
End Function


Public Function getParentPath(CurrentPath) As String
Dim p%
For p% = Len(CurrentPath) - 1 To 1 Step -1
    If Mid$(CurrentPath, p%, 1) = "\" Then
        getParentPath = Left$(CurrentPath, p%)
        Exit For
    End If
Next
End Function


Public Function getFormLeftInTwips(frm As Form) As Long
Dim hwndParent As LongPtr
Dim rctParent As RECT, rct As RECT

hwndParent = GetParent(frm.hWnd)
apiGetWindowRect frm.hWnd, rct
If hwndParent <> Application.hWndAccessApp Then
    apiGetWindowRect hwndParent, rctParent
    With rct
        .Left = .Left - rctParent.Left
        .Top = .Top - rctParent.Top
        .Right = .Right - rctParent.Right
        .Bottom = .Bottom - rctParent.Bottom
    End With
End If
getFormLeftInTwips = PixelsToTwips(rct.Left)
End Function

Public Function getFormTopInTwips(frm As Form) As Long
Dim hwndParent As LongPtr
Dim rctParent As RECT, rct As RECT

hwndParent = GetParent(frm.hWnd)
apiGetWindowRect frm.hWnd, rct
If hwndParent <> Application.hWndAccessApp Then
    apiGetWindowRect hwndParent, rctParent
    With rct
        .Left = .Left - rctParent.Left
        .Top = .Top - rctParent.Top
        .Right = .Right - rctParent.Right
        .Bottom = .Bottom - rctParent.Bottom
    End With
End If
getFormTopInTwips = PixelsToTwips(rct.Top)
End Function


Public Function PixelsToTwips(lngPixels As Long) As Long
Dim hWnd As LongPtr
Dim hdc As LongPtr
Dim PixelsPerInchX As Long
Dim InchesX As Double

Dim rc%

'twip ...
'Unit of measurement used by Microsoft Access and implemented as 1/20 of a point, or 1/1440 of an inch.  There
'are 567 twips to a centimeter

hWnd = GetAccesshWnd()
hdc = GetDC(hWnd)
'ScreenPixelsX = GetDeviceCaps(hdc, VERTRES)
PixelsPerInchX = GetDeviceCaps(hdc, LOGPIXELSX)
InchesX = lngPixels / PixelsPerInchX

PixelsToTwips = InchesX * 1440
rc% = ReleaseDC(hWnd, hdc)


End Function

Public Function ListSelectAll(lst As ListBox, Optional varSelected As Variant)
Dim i As Integer, bln As Boolean
If IsMissing(varSelected) Then
    bln = True
Else
    bln = varSelected
End If

For i = 0 To lst.ListCount - 1
    lst.Selected(i) = bln
Next
End Function


Public Function MultiSelectSet(varList As Variant, lstMulti As ListBox) As Boolean
' Sets the 'selected' items in a multi-select listbox
' from the comma-delimited list of ID numbers in varList


Dim strList As String, i As Long

Const procId = "MultiSelectSet"
On Error GoTo ErrorHandler
MultiSelectSet = True

strList = "," & varList & ","
For i = 0 To lstMulti.ListCount - 1
    lstMulti.Selected(i) = InStr(strList, "," & CStr(lstMulti.ItemData(i)) & ",") > 0
Next
        
Exit Function
    
ErrorHandler:
    msgSystemError Err, Error$, procId
    MultiSelectSet = False

End Function

Public Function MultiSelectGet(varList As Variant, lngListCount As Long, lstMulti As ListBox, Optional varDelimiter As Variant = "", Optional varColumnNo As Variant = 0, Optional varIgnoreList As Variant = "") As Boolean
' Gets the 'selected' items in a multi-select listbox
' and sets varList to a comma-delimited list of values of the selected items
' The optional varIgnoreList can contain items not to be processed
' and should be a tilde-delimited list e.g. "[Blank]~somethingElse"

Dim strList As String, i As Variant, strComma As String

Const procId = "MultiSelectGet"
On Error GoTo ErrorHandler
MultiSelectGet = True

strList = ""
strComma = ""
lngListCount = 0

For Each i In lstMulti.ItemsSelected
    If InStr("~" & varIgnoreList & "~", "~" & CStr(lstMulti.Column(varColumnNo, i)) & "~") = 0 Then
        strList = strList & strComma & varDelimiter & CStr(lstMulti.Column(varColumnNo, i)) & varDelimiter
        strComma = ","
        lngListCount = lngListCount + 1
    End If
Next

varList = strList

Exit Function
    
ErrorHandler:
    msgSystemError Err, Error$, procId
    MultiSelectGet = False

End Function

Public Function MultiSelectGet2column(varListID As Variant, varList As Variant, lngListCount As Long, lstMulti As ListBox, Optional varDelimiter As Variant = "", Optional varColumnNoID As Variant = 0, Optional varColumnNo As Variant = 1, Optional varIgnoreListID As Variant = "") As Boolean
' Gets the 'selected' items in a multi-select listbox
' and sets varListID to a comma-delimited list of values of the selected items in column(0)
' and sets varList to a comma-delimited list of values of the selected items in column(1)
' The optional varIgnoreList can contain items not to be processed
' and should be a tilde-delimited list e.g. "[Blank]~somethingElse"

Dim strListID As String, strList As String, i As Variant, strComma As String

Const procId = "MultiSelectGet2column"
On Error GoTo ErrorHandler
MultiSelectGet2column = True

strListID = ""
strList = ""
strComma = ""
lngListCount = 0

For Each i In lstMulti.ItemsSelected
    If InStr("~" & varIgnoreListID & "~", "~" & CStr(lstMulti.Column(varColumnNoID, i)) & "~") = 0 Then
        strListID = strListID & strComma & varDelimiter & CStr(lstMulti.Column(varColumnNoID, i)) & varDelimiter
        strList = strList & strComma & varDelimiter & CStr(lstMulti.Column(varColumnNo, i)) & varDelimiter
        strComma = ","
        lngListCount = lngListCount + 1
    End If
Next

varList = strList
varListID = strListID

Exit Function
    
ErrorHandler:
    msgSystemError Err, Error$, procId
    MultiSelectGet2column = False

End Function
Public Function xMultiSelectGet(varList As Variant, lstMulti As ListBox) As Boolean
' Gets the 'selected' items in a multi-select listbox
' and sets varList to a comma-delimited list of values of the selected items


Dim strList As String, i As Variant, strComma As String

Const procId = "MultiSelectSet"
On Error GoTo ErrorHandler
xMultiSelectGet = True

strList = ""
strComma = ""

For Each i In lstMulti.ItemsSelected
    strList = strList & strComma & CStr(lstMulti.ItemData(i))
    strComma = ","
Next

varList = strList

Exit Function
    
ErrorHandler:
    msgSystemError Err, Error$, procId
    xMultiSelectGet = False

End Function

Public Function MultiSelectPopulate(lstMulti As ListBox) As Boolean
' Used in forms where a multi-select list box corresponds
' to a set of child records linked to a parent in the form.

' The third column (Column(2)) in the list box must contain the parent ID
' if a record exists in the child table, or be null if not

Dim ctrl As Control, i As Integer

Const procId = "MultiSelectPopulate"
On Error GoTo ErrorHandler

MultiSelectPopulate = True
Set ctrl = lstMulti
ctrl.Requery
For i = 0 To ctrl.ListCount - 1
    ctrl.Selected(i) = Not (ctrl.Column(2, i) = "")
Next


Exit Function
    
ErrorHandler:
    msgSystemError Err, Error$, procId
    MultiSelectPopulate = False

End Function



Public Function MultiSelectUpdate(rsChild As Recordset, lstMulti As ListBox, lngParentID As Long) As Boolean
' Used in forms where a multi-select list box corresponds
' to a set of child records (passed as a recordset of the whole child table) linked to a parent in the form.
'
' If the current item in the list is selected then
' a record must exist in the child table.
' Otherwise, one must not exist
'
' Notes
' 1. The child key value must be the first column (Column(0)) in the list box
' 2. Their must be only ONE primary key column in the parent table (a long int)
' 3. Their must be a 2-column primary key in the child table (both long int) , ParentID first

Dim ctrl As Control, lngChildID As Long, strParentIDName As String, strChildIDName As String

Const procId = "MultiSelectUpdate"
On Error GoTo ErrorHandler
MultiSelectUpdate = True

strParentIDName = rsChild.Fields(0).Name
strChildIDName = rsChild.Fields(1).Name

Set ctrl = lstMulti

lngChildID = ctrl.Column(0)
With rsChild
    .FindFirst strParentIDName & " = " & CStr(lngParentID) & " and " & strChildIDName & " = " & CStr(lngChildID)
    If ctrl.Selected(ctrl.ListIndex) Then
        If .NoMatch Then
            .AddNew
            .Fields(strParentIDName) = lngParentID
            .Fields(strChildIDName) = lngChildID
            .Update
        End If
    Else
            If Not .NoMatch Then
                .Delete
            End If
    End If
End With

Exit Function
    
ErrorHandler:
    msgSystemError Err, Error$, procId
    MultiSelectUpdate = False

End Function




Public Function SpellCheck()
RunCommand acCmdSpelling

End Function

Public Function QdefSQLSet(qryDef As Variant, strNewSQL As String) As Boolean
' sets the SQL of the specified querydef
Dim qdefs As QueryDefs, qdef As QueryDef

On Error GoTo errHandler

QdefSQLSet = False

If IsObject(qryDef) Then
    Set qdef = qryDef
Else
    Set qdefs = CurrentDb.QueryDefs
    Set qdef = qdefs(qryDef)
End If

qdef.sql = strNewSQL

QdefSQLSet = True
Exit Function

errHandler:
    writeTxt "Error Code: " & str$(Err) & " " & Error$ & nl() & nl() & "SQL as follows..." & nl() & nl() & strNewSQL, "SQLDUMP.TXT", False
    MsgBox "SQL failed - see SQLDUMP.txt"

End Function

Public Function isTableEmpty(tableName As String) As Boolean
isTableEmpty = DCount("*", tableName) = 0


End Function

Public Function getProperty(obj As Object, strPropertyName As String) As Variant

On Error GoTo ErrorHandler

getProperty = Null

getProperty = obj.Properties(strPropertyName)
Exit Function

ErrorHandler:
End Function

Public Function UpdateQuerySQL(varSQL As Variant, strParameterID As String, strNewValue As String) As String

' Updates the supplied SQL string with one selection parameter.
' E.g. if the SQL contains the text "GROUP_ID = 'ABC'" then
' this routine could be used to change it to "GROUP_ID = 'XYZ'"
' with the call UpdateQuerySQL strSQL, "GROUP_ID =", "XYZ".

Const procId = "UpdateQuerySQL"
Dim strDelim As String, ps As Integer, pe As Integer

If IsNull(varSQL) Then
    msgSystemError 0, "Supplied SQL is null", procId
    Exit Function
End If

UpdateQuerySQL = varSQL ' set to supplied SQL in case of early exit from function

' Part of the SQL will be "GROUP_ID = 'FMDC'"
' but the string in the single quotes (FMDC in this example)
' might contain anything.  That string is what we need to replace.
'
ps = InStr(varSQL, strParameterID)
If ps = 0 Then
    msgSystemError 0, "Parameter " & strParameterID & " not found", procId
    Exit Function
End If

' look for start of literal
Do While Mid$(varSQL, ps, 1) <> "'" And Mid$(varSQL, ps, 1) <> """"
    ps = ps + 1
    If ps > Len(varSQL) Then
        msgSystemError 0, "Old value for " & strParameterID & " not found", procId
        Exit Function
    End If
Loop

' store the delimiter
strDelim = Mid$(varSQL, ps, 1)

ps = ps + 1                             ' ps now points at beginning of string
pe = InStr(ps, varSQL, strDelim) - 1    ' pe now points at end of string

' now replace the old parameter value with the new one
varSQL = Left$(varSQL, ps - 1) & strNewValue & Mid$(varSQL, pe + 1)

UpdateQuerySQL = varSQL

End Function

Public Function UpdateQueryDef(strQueryDefName As String, strParameterID As String, strNewValue As String)
setQueryDefSQL strQueryDefName, UpdateQuerySQL(getQueryDefSQL(strQueryDefName), strParameterID, strNewValue)

End Function

Public Function getQueryDefSQL(strQueryDefName As String) As Variant
' returns the specified QueryDef's SQL

Dim db As Database, qdef As QueryDef

On Error GoTo ErrorHandler

getQueryDefSQL = Null

Set db = CurrentDb()
Set qdef = db.QueryDefs(strQueryDefName)
getQueryDefSQL = qdef.sql

Exit Function

ErrorHandler:
    msgSystemError Err, Error$, "getQueryDefSQL"
    getQueryDefSQL = False

End Function

Public Function setQueryDefSQL(strQueryDefName As String, varSQL As Variant) As Boolean
' overwrites the QueryDef's SQL with the supplied SQL

Dim db As Database, qdef As QueryDef

On Error GoTo ErrorHandler

Set db = CurrentDb()
Set qdef = db.QueryDefs(strQueryDefName)
qdef.sql = varSQL
db.QueryDefs.Refresh
setQueryDefSQL = True
Exit Function

ErrorHandler:
    msgSystemError Err, Error$, "setQueryDefSQL"
    setQueryDefSQL = False

End Function


Public Function Soundex(var1) As String
' Returns a 'soundex' value for the given string
' Result is always 1 to 4 bytes, 1st byte is always 1st byte of var1
' Can be used in 'fuzzy' searches

Dim s1 As String, x As String, c1 As String
Dim p As Integer, c As Integer, T As Integer
Const ALPHABET = "abcdefghijklmnopqrstuvwxyz"
Const SOUNDEX_DIGITS = " 123 12  22455 12623 1 2 2"

s1 = CStr(var1 & "")

' keep 1st character
c1 = Left$(s1, 1)

' get soundex digits for string 1
For p = 2 To Len(s1)
    c = InStr(ALPHABET, Mid$(s1, p, 1))
    If c > 0 Then
        x = Mid$(SOUNDEX_DIGITS, c, 1)
        If x <> " " Then
            c1 = c1 & x
        End If
        If Len(c1) = 4 Then p = Len(s1) + 1
    End If
Next

Soundex = Left$(c1 & "0000", 4)

End Function

Public Function FuzzyCompare(var1, var2) As Boolean

FuzzyCompare = Soundex(var1) = Soundex(var2)

End Function

Public Function GetNumberFromText(varText) As Variant
' Extracts a number from a string and returns it
Dim s As Integer, e As Integer

' look for first numeric character
s = 1
Do While s < Len(varText & " ") And Not IsNumeric(Mid$(varText & " ", s, 1)): s = s + 1: Loop

If s > Len(varText) Then
    GetNumberFromText = Null
    Exit Function
End If


' return number
GetNumberFromText = Val(Mid$(varText, s))


End Function

Public Function GetAlphaFromText(varText) As Variant
' Extracts characters up to first number from a string and returns it
Dim s As Integer, e As Integer

' look for first numeric character
s = 1
Do While s < Len(varText & " ") And Not IsNumeric(Mid$(varText & " ", s, 1)): s = s + 1: Loop

If s > Len(varText) Then
    GetAlphaFromText = varText
    Exit Function
End If


' return number
GetAlphaFromText = Left$(varText, s - 1)


End Function

Public Function GetNumberSuffix(varText) As Variant
' Extracts a number from the end of a string and returns it
Dim s As Integer
If IsNull(varText) Then
    GetNumberSuffix = Null
    Exit Function
End If

' look for first numeric character
s = Len(varText)
Do While s > 0
    If Not Mid$(varText, s, 1) Like "[0123456789]" Then Exit Do
    s = s - 1
Loop

If s = Len(varText) Then
    GetNumberSuffix = Null
    Exit Function
End If


' return number
GetNumberSuffix = Mid$(varText, s + 1)


End Function

Public Function GetAlphaPrefix(varText) As Variant
' Extracts characters up to the numeric suffix

If IsNull(varText) Then
    GetAlphaPrefix = Null
    Exit Function
End If
GetAlphaPrefix = Left$(varText, Len(varText) - Len(Nz(GetNumberSuffix(varText), "")))

End Function



Public Function addToPickList(strNewData As String, strTableName As String, strIDfield As String, strDataField As String) As Integer

On Error GoTo Err_proc

Dim Msg As String
Dim ctrl As Control, i As Integer, strListID As String, lngLookupID As Long
Dim Val As Variant
Dim rs As Recordset
Dim strRowSource As String

addToPickList = acDataErrDisplay
If MsgBox("Do you want to add '" & strNewData & "' to this Pick List?", vbYesNo, "Not in pick list!") = vbYes Then
    
    Set ctrl = Screen.ActiveControl
    
    ' get rowsource
    strRowSource = ctrl.RowSource
    
    Set rs = CurrentDb.OpenRecordset(strTableName, dbOpenDynaset, dbSeeChanges)
    With rs
        ' add a new record and save it
        .AddNew
        .Fields(strDataField) = strNewData
        .Update
        ' now open the record to get the ID, and return it
        ' note this method compatible with SQLserver autonumbers
        .MoveLast
        lngLookupID = .Fields(strIDfield)
    End With
    rs.Close
    Set rs = Nothing
    
    addToPickList = acDataErrAdded
    
Else
    addToPickList = acDataErrContinue
End If


Exit_proc:
On Error Resume Next
rs.Close
Exit Function

Err_proc:
MsgBox Err.Description, vbInformation, "addItem_ToTable Error: " & Err.Number
Resume Exit_proc

End Function


Public Static Function FinQuarter(varDate) As Integer
Dim FinancialYearStartMonth As Integer, qtr As Integer, M As Integer, y As Integer

'e.g. FinancialYearStartMonth = 4 then Months 1-3 = Q4, 4-6 = Q1, 7-9 = Q2, 10-12 = Q3
'     and if month < FinancialYearStartMonth then FinYear = Year + 1

If FinancialYearStartMonth = 0 Then
    FinancialYearStartMonth = num(SettingsGet("FinancialYearStartMonth"))
End If

M = Month(varDate)
 
M = M - FinancialYearStartMonth + 1
If M < 1 Then
    M = M + 12
    y = 1
Else
    y = 0
End If
    
FinQuarter = Int((M + 2) / 3)

End Function


Public Static Function FinYear(varDate) As Integer
Dim FinancialYearStartMonth As Integer, qtr As Integer, M As Integer, y As Integer

'e.g. FinancialYearStartMonth = 4 then Months 1-3 = Q4, 4-6 = Q1, 7-9 = Q2, 10-12 = Q3
'     and if month < FinancialYearStartMonth then FinYear = Year + 1

If FinancialYearStartMonth = 0 Then
    FinancialYearStartMonth = num(SettingsGet("FinancialYearStartMonth"))
End If

M = Month(varDate)
 
M = M - FinancialYearStartMonth + 1
If M < 1 Then
    M = M + 12
    y = -1
Else
    y = 0
End If
    
FinYear = Year(varDate) + y

End Function





Public Function FinTest()
Dim M As Integer, d As Date, n As Integer

d = #1/1/2001#

For n = 1 To 25
    Debug.Print d, FinQuarter(d), FinYear(d)
    d = DateAdd("m", 1, d)
Next

End Function



Function isQuery(strSQL) As Integer
Dim db As Database, ctr As QueryDef, n As String
On Error GoTo isQuery_end

isQuery = False
Set db = CurrentDb()
Set ctr = db.QueryDefs(strSQL)
isQuery = True

isQuery_end:
End Function

Function runPassThruActionQuery(SQLst As String, ID As String) As Integer

' creates a temporary querydef and runs it

Dim ws As Workspace, db As Database
Dim sysErr, sysErrMsg
Dim qdef As QueryDef
Dim msgText As String
Dim errorItem As Error
Dim e As Long, n As Long


On Error GoTo errHandler

runPassThruActionQuery = False

Set ws = DBEngine(0)
Set db = ws(0)

Set qdef = db.CreateQueryDef("")
ws.BeginTrans
With qdef
    .Connect = db.QueryDefs("qryPTConnect").Connect
    .sql = SQLst
    .ReturnsRecords = False
    .Execute
    
End With
''''db.Execute SQLst, dbSQLPassThrough
ws.CommitTrans

runPassThruActionQuery = True

    Exit Function

errHandler:



On Error Resume Next

    sysErr = Err: sysErrMsg = Error$
    msgText = ""

    For Each errorItem In DBEngine.Errors
         With errorItem
            msgText = msgText & IIf(msgText = "", "", vbNewLine) & .Description
         End With
    Next errorItem

    writeTxt "Error Code: " & str$(sysErr) & " " & msgText & vbNewLine & vbNewLine & "SQL as follows..." & vbNewLine & vbNewLine & getQuerySQL(SQLst), "SQLDUMP.TXT", False
    msgSystemError sysErr, "SQL failed - see SQLDUMP.TXT", IIf(ID = "", "runPassThruActionQuery", ID)
    ws.Rollback
    runPassThruActionQuery = False

End Function


Public Function getQuerySQL(strSQL As String) As String
' if strSQL contains a query name, returns the SQL from that query
' otherwise assumes strSQL is SQL and returns that

If isQuery(strSQL) Then
    getQuerySQL = getQueryDefSQL(strSQL)
Else
    getQuerySQL = strSQL
End If

End Function

Public Function LogUserDetails(strInOrOut As String, Optional varVersion As Variant = Null)
' Writes details of user to back-end database
' to make it easier to track who's using the
' system.

' syntax e.g. LogUserDetails "in", CStr(DMax("Version", "tblVersions"))

Dim db As Database, rs As Recordset
Dim strUserID As String, strPCName As String, strDBname As String
Dim blnRecordFound As Boolean

Set db = CurrentDb

strUserID = GetUserID()
strPCName = GetComputerName()
strDBname = db.Name

Set rs = db.OpenRecordset("tblUserTracking", dbOpenDynaset, dbSeeChanges)

With rs
    If .EOF And .BOF Then
        blnRecordFound = False
    Else
        .FindFirst "UserID = " & SQLstring(strUserID) & " and PCname = " & SQLstring(strPCName)
        blnRecordFound = Not .NoMatch
    End If
    If Not blnRecordFound Then
        .AddNew
        !UserID = strUserID
        !PCName = strPCName
    Else
        .Edit
    End If
    
    If strInOrOut = "in" Then
        !LoginTime = Now()
        !LogoutTime = Null
    Else
        !LogoutTime = Now()
    End If
    !FrontEnd = strDBname
    If Not IsNull(varVersion) Then !Version = varVersion
    
    .Update

End With

rs.Close
Set rs = Nothing

db.Close




End Function


Function IsComboDropped() As Boolean
'  returns true if a combo box on the form is dropped down
'  only one combo can have the focus => only one drop down
'  Note: doesn't work in modal dialogs

Static hWnd As LongPtr
Static hWndCBX_LBX As LongPtr

   hWnd = 0: hWndCBX_LBX = 0

   '  Start with finding the window with "ODCombo" class name
   hWnd = apiFindWindow(ACC_CBX_LISTBOX_PARENT_CLASS, _
                                vbNullString)
  
   '  Parent window of ODCombo is the Access window
   If apiGetParent(hWnd) = hWndAccessApp Then
         '  Child window of ODCombo window is the
         '  drop down listbox associated with a combobox
         hWndCBX_LBX = apiGetWindow(hWnd, GW_CHILD)
         '  another check to confirm that we're looking at the right window
         If fGetClassName(hWndCBX_LBX) = _
                        ACC_CBX_LISTBOX_CLASS Then
            '  Finally, if this window is visible,
            If apiGetWindowLong(hWnd, GWL_STYLE) And WS_VISIBLE Then
               '  the Combo must be open
               IsComboDropped = True
            End If
         End If
      End If
End Function


Private Function fGetClassName(hWnd As LongPtr)
Dim strBuffer As String
Dim lngLen As Long
Const MAX_LEN = 255
    strBuffer = Space$(MAX_LEN)
    lngLen = apiGetClassName(hWnd, strBuffer, MAX_LEN)
    If lngLen > 0 Then fGetClassName = Left$(strBuffer, lngLen)
End Function




Public Function UnselectText()
Screen.ActiveControl.SelStart = 0
End Function





Public Function max(varNumber1 As Variant, varNumber2 As Variant) As Double
If Nz(varNumber1) > Nz(varNumber2) Then
    max = Nz(varNumber1)
Else
    max = Nz(varNumber2)

End If
End Function


Public Function sleep(intSeconds)
Dim T As Date
T = Now

Do While Now < DateAdd("s", intSeconds, T): Loop


End Function


Public Function IsValidDate(dtField As Variant) As Boolean
IsValidDate = True
If IsNull(dtField) Then Exit Function
If Not IsDate(dtField) Then IsValidDate = False: Exit Function
If dtField < #1/1/1900# Or dtField > #12/31/2999# Then IsValidDate = False: Exit Function

End Function

Public Function IsValidTime(dtField As Variant) As Boolean
IsValidTime = True
If IsNull(dtField) Then Exit Function
If Not IsDate(dtField) Then IsValidTime = False: Exit Function
If dtField < #12:00:00 AM# Or dtField > #11:59:59 PM# Then IsValidTime = False: Exit Function

End Function

Public Function getWordUnderCursor(txtObject As TextBox) As String
' returns word currently highlighted by cursor

Dim ps As Integer, pe As Integer

ps = txtObject.SelStart
pe = txtObject.SelLength

getWordUnderCursor = txtObject.SelText

End Function

Public Function getDefault(strDefault) As Variant
' sets activecontrol to specified default value.
' The supplied default will be 'evaluated',
' so you can use things like 'now()'

On Error Resume Next
If Not IsNull(Screen.ActiveControl) Then Exit Function
Screen.ActiveControl = Eval(IIf(strDefault = "Date", "Date()", strDefault))


End Function

Public Sub EnumReferences()
Dim ref As Reference, refs As References

For Each ref In Application.References
    Debug.Print ref.Name, ref.fullPath
Next
DoCmd.RunCommand acCmdDebugWindow
End Sub


Public Function Concatenate(ParamArray varItems()) As Variant
Dim varItem As Variant, strX As String, strComma As String

strX = "": strComma = ""

For Each varItem In varItems()
    If Not isBlank(varItem) Then
        strX = strX & strComma & varItem
        strComma = ", "
    End If
Next
Concatenate = strX
End Function



Public Function ConcatenateTitle(strTitle As String, varValue As Variant, Optional varSeparator As Variant = ": ") As Variant
If IsNull(varValue) Then
    ConcatenateTitle = Null
    Exit Function
End If
ConcatenateTitle = strTitle & varSeparator & CStr(varValue)
End Function



Public Function getINlist(varTableName, varFieldName, Optional varCriteria = Null, Optional varDelimLeft = "'", Optional varDelimRight = "", Optional varSeparator = ",")
' Selects records based on tablename, fieldName and criteria
' concatenates data from specified field formatted as an SQL 'in' list

Dim rs As Recordset, strSep As String, strInList As String, strDelimLeft As String, strDelimRight As String

strDelimLeft = varDelimLeft
If varDelimRight = "" Then
    strDelimRight = varDelimLeft
Else
    strDelimRight = varDelimRight
End If
    
Set rs = CurrentDb.OpenRecordset("select distinct [" & varFieldName & "] as SelectedData from [" & varTableName & "]" & IIf(IsNull(varCriteria), "", " where " & varCriteria), dbOpenDynaset)
With rs
    If Not (.EOF And .BOF) Then
        .MoveFirst
        Do While Not .EOF
            If Not IsNull(!SelectedData) Then
                strInList = strInList & strSep & strDelimLeft & !SelectedData & strDelimRight
                strSep = varSeparator
            End If
            .MoveNext
        Loop
    End If
End With

getINlist = strInList
rs.Close
Set rs = Nothing
End Function




Public Function LookupExists(Optional blnCheckSoundsLike As Boolean = True) As Boolean
Dim rs As Recordset, blnResults As Boolean, frm As Form
Dim DescriptionFieldName, DescriptionFieldValue

Set frm = Screen.ActiveControl.Parent

Debug.Print frm.Name, Screen.ActiveControl.Name

DescriptionFieldName = Screen.ActiveControl.ControlSource
DescriptionFieldValue = Screen.ActiveControl

blnResults = False

If Screen.ActiveControl = Screen.ActiveControl.OldValue Then Exit Function

Set rs = frm.RecordsetClone
With rs
    If Not (.EOF And .BOF) Then
        .FindFirst "[" & DescriptionFieldName & "] = """ & DescriptionFieldValue & """"
        If Not .NoMatch Then
            blnResults = True
        Else
            If blnCheckSoundsLike Then
                ' check with soundex
                .FindFirst "soundex([" & DescriptionFieldName & "]) = """ & Soundex(DescriptionFieldValue) & """"
                If Not .NoMatch Then
                    If MsgBox("An item similar to this already exists (" & .Fields(DescriptionFieldName) & ")" & vbNewLine & _
                              "If you want to continue and add this new item, click OK, otherwise click Cancel then press Escape.", vbInformation + vbOKCancel, "Possible Duplicate Entry") = vbCancel Then
                              blnResults = True
                    End If
                End If
            End If
        End If
    End If
End With
rs.Close
Set rs = Nothing

If blnResults = True Then
    msgDataError "Duplicate Entry!"
End If

LookupExists = blnResults
End Function

Function updateDisplaySeq(ctrl As Control, strIDcolumn, strSeqColumn, intMovementAmount As Integer)
' changes the display sequence by swapping the DisplaySeq values of two records

Dim rs As Recordset
Dim strBookmark As String
Dim lngDS1 As Long, lngDS2 As Long
Dim frm As Form

Set frm = ctrl.Parent
If frm.Dirty Then
    ctrl.SetFocus
    saveRecord
End If

Set rs = frm.RecordsetClone
With rs
    If frm.NewRecord And Not frm.Dirty Then Exit Function
    .Bookmark = frm.Bookmark
    
    strBookmark = .Bookmark
    If IsNull(.Fields(strSeqColumn)) Then
        .Edit
            .Fields(strSeqColumn) = .Fields(strIDcolumn)
        .Update
        .Bookmark = strBookmark
    End If
    lngDS1 = .Fields(strSeqColumn)
    If intMovementAmount < 0 Then ' move up
        .MovePrevious
        If .BOF Then GoTo fini
    Else
        .MoveNext
        If .EOF Then GoTo fini
    End If
    
    lngDS2 = .Fields(strSeqColumn)
    .Edit
        .Fields(strSeqColumn) = lngDS1
    .Update
    .Bookmark = strBookmark
    .Edit
        .Fields(strSeqColumn) = lngDS2
    .Update
End With
rs.Close
frm.Requery
Set rs = frm.RecordsetClone
rs.FindFirst "[" & strSeqColumn & "] = " & CStr(lngDS2)
If Not rs.NoMatch Then
    frm.Bookmark = rs.Bookmark
End If
rs.Close

fini:

End Function


Function updateDisplaySeq2(intMovementAmount As Integer)
' changes the display sequence by swapping the DisplaySeq values of two records

Dim rs As Recordset
Dim strBookmark As String
Dim lngDS1 As Long, lngDS2 As Long
Dim frm As Form

Set frm = Screen.ActiveForm
If frm.Dirty Then
    frm.SetFocus
    saveRecord
End If

Set rs = frm.RecordsetClone
With rs
    .Bookmark = frm.Bookmark
    
    strBookmark = .Bookmark
    lngDS1 = !DisplaySeq
    If intMovementAmount < 0 Then ' move up
        .MovePrevious
        If .BOF Then GoTo fini
    Else
        .MoveNext
        If .EOF Then GoTo fini
    End If
    
    lngDS2 = !DisplaySeq
    .Edit
        !DisplaySeq = lngDS1
    .Update
    .Bookmark = strBookmark
    .Edit
        !DisplaySeq = lngDS2
    .Update
End With
rs.Close
frm.Requery
Set rs = frm.RecordsetClone
rs.FindFirst "DisplaySeq = " & CStr(lngDS2)
If Not rs.NoMatch Then
    frm.Bookmark = rs.Bookmark
End If
rs.Close

fini:

End Function


Public Function getChildData(strChildTable As String, strDataFields As String, strJoinData As String, Optional varDelim = ", ", Optional varFieldDelim = ", ") As Variant
' Some data is held in 'child' tables related to the main table by one or more key fields.
' This function gets the related records, and concatenates the data into a single string to be returned to
' the calling program.
' If the description for a field (in table design) = "lookup", then the data in the child table field is a lookup ID and the lookup value will be returned.
'
' Note that the child table can have more than one data field, all of which have to be processed
'
'e.g.
'   getChildData("tblPostureHead","Posture","[AnimalID] = " & sqlString([AnimalID]) & " and [ObservationDate] = " & sqlDate([ObservationDate]), true)
'   getChildData("tblScratchreflexes","[Reaction], [Comment]","[AnimalID] = " & sqlString([AnimalID]) & " and [ObservationDate] = " & sqlDate([ObservationDate]), true)

Dim arJF() As Variant, i As Integer, n As Integer, strSQL As String, varData As Variant
Dim rs As Recordset
Dim strDelim As String, varThisData As Variant, strAnd As String, f As Integer

varData = Null

strSQL = "select " & strDataFields & " from " & strChildTable & " " & IIf(isBlank(strJoinData), "", "where ") & strJoinData & ";"

Set rs = CurrentDb.OpenRecordset(strSQL, dbOpenDynaset, dbSeeChanges)

With rs
    If Not (.EOF And .BOF) Then
        .MoveFirst
        Do While Not .EOF
            For f = 0 To .Fields.Count - 1
                varThisData = .Fields(f)
                If getProperty(.Fields(f), "Description") & "" = "lookup" Then ' it's a lookup
                    varThisData = getLookup(varThisData)
                End If
                If Not IsNull(varThisData) Then
                    varData = varData & strDelim & varThisData
                    strDelim = varFieldDelim
                End If
            Next
            .MoveNext
            strDelim = varDelim
        Loop
    End If
    
End With

Set rs = Nothing
getChildData = varData

End Function


Public Function getLookup(varLookupID As Variant) As Variant

If IsNull(varLookupID) Then
    getLookup = Null
Else
    getLookup = DLookup("lookupValue", "tblLookups", "LookupID = " & CStr(varLookupID))
End If

End Function


Static Function SettingsGet_BE(itemName As String, Optional Default As Variant) As Variant
' If RunMode = "Test" in tbl Settings, this function
' will return the values from the TestSetting column if present (or the Setting column if TestSetting is null),

Dim Setting As Variant, blnInitialised As Variant
Dim rs As Recordset
Dim ps As Integer, pe As Integer, bracketCount As Integer, x As String, subSetting As Variant
Dim strParentPath As String, blnIsTesting As Boolean

If Not blnInitialised Then
    blnInitialised = True
    Set rs = CurrentDb.OpenRecordset("tblSettings_BE", dbOpenDynaset)
End If
rs.Requery
rs.FindFirst "Id = ""RunMode"""
If rs.NoMatch Then
    blnIsTesting = False
Else
    blnIsTesting = (rs!Setting = "Test")
End If

rs.FindFirst "Id = " & SQLstring(itemName)

If rs.NoMatch Then
    If IsMissing(Default) Then
        Setting = Null
    Else
        Setting = Default
    End If
Else
    If blnIsTesting And Not IsNull(rs!TestSetting) Then
        Setting = expandedText(rs!TestSetting)
    Else
        Setting = expandedText(rs!Setting)
    End If
End If



' check for recursive references to another setting (i.e. the setting contains a "[xxxx]" reference
Do While InStr(Setting, "[") > 0
    ps = InStr(Setting, "[")
    If ps = 0 Then
        SettingsGet_BE = Setting
        Exit Function
    End If
    
    ' found a reference - look for the matching "]"
    
    pe = ps + 1
    bracketCount = 1
    Do While pe <= Len(Setting)
        x = Mid$(Setting, pe, 1)
        Select Case x
            Case "]":   bracketCount = bracketCount - 1
            Case "[":   bracketCount = bracketCount + 1
        End Select
        If bracketCount = 0 Then
            subSetting = SettingsGet_BE(Mid$(Setting, ps + 1, pe - (ps + 1))) & ""
            Setting = Left$(Setting, ps - 1) & subSetting & Mid$(Setting, (pe + 1))
            Exit Do
        End If
        pe = pe + 1
    Loop
Loop

strParentPath = getParentPath(SystemPath())
If Right$(strParentPath, 1) = "\" Then strParentPath = Left$(strParentPath, Len(strParentPath) - 1)
Setting = replaceString(Setting, "ParentPath$", strParentPath)
Setting = replaceString(Setting, "AppPath$", SystemPath())

SettingsGet_BE = Setting

End Function

Function SettingsSet_BE(itemName As String, itemValue As Variant)
Dim iv As Variant

iv = itemValue
If varType(iv) = 8 Then 'string type
    If Left$(iv, 1) <> """" Then
        iv = SQLstring(iv)
    End If
End If

If DCount("*", "tblsettings_BE", "id = " & SQLstring(itemName)) = 0 Then
    runActionQuery "INSERT INTO [tblSettings_BE] ( id ) values (" & SQLstring(itemName) & ");", "SettingsSet_BE"
End If
If runActionQuery("UPDATE [tblSettings_BE] SET setting = " & iv & " where Id = " & SQLstring(itemName) & ";", "SettingsSet_BE") Then
End If

End Function
