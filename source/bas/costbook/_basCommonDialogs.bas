MODULE NAME: _basCommonDialogs
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

      Private Const BIFRETURNONLYFSDIRS = 1
      Private Const BIFDONTGOBELOWDOMAIN = 2
      Private Const MAXPATH = 260


      Private Declare PtrSafe Function SHBrowseForFolder Lib "shell32" (lpbi As BrowseInfo) As Long

      Private Declare PtrSafe Function SHGetPathFromIDList Lib "shell32" (ByVal pidList As LongPtr, ByVal lpBuffer As String) As Long

      Private Declare PtrSafe Function lstrcat Lib "kernel32" Alias "lstrcatA" (ByVal lpString1 As String, ByVal lpString2 As String) As LongPtr

      Private Type BrowseInfo
         hwndOwner      As LongPtr
         pIDLRoot       As LongPtr
         pszDisplayName As Long
         lpszTitle      As String
         ulFlags        As Long
         lpfnCallback   As LongPtr
         lParam         As LongPtr
         iImage         As Long
      End Type
  
Declare PtrSafe Function GetOpenFileName Lib "comdlg32.dll" Alias "GetOpenFileNameA" (pOpenfilename As OPENFILENAME) As Long
Declare PtrSafe Function GetSaveFileName Lib "comdlg32.dll" Alias "GetSaveFileNameA" (pOpenfilename As OPENFILENAME) As Long

Type MSAOPENFILENAME
    ' Filter string used for the Open dialog filters.
    ' Use MSACreateFilterString() to create this.
    ' Default =All Files, *.*
    strFilter As String
    ' Initial Filter to display.
    ' Default =1.
    lngFilterIndex As Long
    ' Initial directory for the dialog to open in.
    ' Default =Current working directory.
    strInitialDir As String
    ' Initial file name to populate the dialog with.
    ' Default ="".
    strInitialFile As String
    strDialogTitle As String
    ' Default extension to append to file if user didn't specify one.
    ' Default =System Values (Open File, Save File).
    strDefaultExtension As String
    ' Flags (see constant list) to be used.
    ' Default =no flags.
    lngFlags As Long
    ' Full path of file picked.  When the File Open dialog box is
    ' presented, if the user picks a nonexistent file,
    ' only the text in the "File Name" box is returned.
    strFullPathReturned As String
    ' File name of file picked.
    strFileNameReturned As String
    ' Offset in full path (strFullPathReturned) where the file name
    ' (strFileNameReturned) begins.
    intFileOffset As Integer
    ' Offset in full path (strFullPathReturned) where the fileextension begins.
    intFileExtension As Integer
End Type

Const ALLFILES = "All Files"

Type OPENFILENAME
    lStructSize As Long
    hwndOwner As LongPtr
    hInstance As LongPtr
    lpstrFilter As String
    lpstrCustomFilter As Long
    nMaxCustrFilter As Long
    nFilterIndex As Long
    lpstrFile As String
    nMaxFile As Long
    lpstrFileTitle As String
    nMaxFileTitle As Long
    lpstrInitialDir As String
    lpstrTitle As String
    Flags As Long
    nFileOffset As Integer
    nFileExtension As Integer
    lpstrDefExt As String
    lCustrData As LongPtr
    lpfnHook As LongPtr
    lpTemplateName As String
End Type

Const OFNALLOWMULTISELECT = &H200
Const OFNCREATEPROMPT = &H2000
Const OFNEXPLORER = &H80000
Const OFNFILEMUSTEXIST = &H1000
Const OFNHIDEREADONLY = &H4
Const OFNNOCHANGEDIR = &H8
Const OFNNODEREFERENCELINKS = &H100000
Const OFNNONETWORKBUTTON = &H20000
Const OFNNOREADONLYRETURN = &H8000
Const OFNNOVALIDATE = &H100
Const OFNOVERWRITEPROMPT = &H2
Const OFNPATHMUSTEXIST = &H800
Const OFNREADONLY = &H1
Const OFNSHOWHELP = &H10


Function FindFile(strTitle, strSearchPath, FileSpecs As String) As String
' Displays the Open dialog box for the user to locate
' the file.
' FileSpecs format like "Program Files|*.exe;*.com|All Files|*.*"
' e.g. strFileName = findfile("Open File", "c:\", "Program Files|*.exe;*.com|All Files|*.*")

    Dim msaof As MSAOPENFILENAME

    ' Set options for the dialog box.
    msaof.strDialogTitle = strTitle
    msaof.strInitialDir = strSearchPath
    'msaof.strFilter =MSACreateFilterString(FileSpecs())
    msaof.strFilter = MSAConvertFilterString(FileSpecs)

    ' Call the Open dialog routine.
    MSAGetOpenFileName msaof

    ' Return the path and file name.
    FindFile = Trim(msaof.strFullPathReturned)

End Function

Public Function SelectDirectory(ipTitle) As String
      'Opens a Treeview control that displays the directories in a computer

         Dim lpIDList As LongPtr
         Dim sBuffer As String
         Dim szTitle As String
         Dim tBrowseInfo As BrowseInfo


        SelectDirectory = ""
     
         szTitle = ipTitle
         With tBrowseInfo
            .hwndOwner = GetAccesshWnd()
            .lpszTitle = lstrcat(szTitle, "")
            .ulFlags = BIFRETURNONLYFSDIRS + BIFDONTGOBELOWDOMAIN
         End With

         lpIDList = SHBrowseForFolder(tBrowseInfo)

         If (lpIDList) Then
            sBuffer = Space(MAXPATH)
            SHGetPathFromIDList lpIDList, sBuffer
            sBuffer = Left(sBuffer, InStr(sBuffer, vbNullChar) - 1)
            If Right$(sBuffer, 1) <> "\" Then sBuffer = sBuffer & "\"
            SelectDirectory = sBuffer
         End If

End Function



Function MSACreateFilterString(ParamArray varFilt() As Variant) As String
' Creates a filter string from the passed in arguments.
' Returns "" if no arguments are passed in.
' Expects an even number of arguments (filter name, extension), but
' if an odd number is passed in, it appends "*.*".

    Dim strFilter As String
    Dim intRet As Integer
    Dim intNum As Integer

    intNum = UBound(varFilt)
    If (intNum <> -1) Then
        For intRet = 0 To intNum
            strFilter = strFilter & varFilt(intRet) & vbNullChar
        Next
        If intNum Mod 2 = 0 Then
            strFilter = strFilter & "*.*" & vbNullChar
        End If
    
        strFilter = strFilter & vbNullChar
    Else
        strFilter = ""
    End If

    MSACreateFilterString = strFilter
End Function

Function MSAConvertFilterString(strFilterIn As String) As String
' Creates a filter string from a bar ("|") separated string.
' The string should pairs of filter|extension strings, i.e. "Access Databases|*.mdb|All Files|*.*"
' If no extensions exists for the last filter pair, *.* is added.
' This code will ignore any empty strings, i.e. "||" pairs.
' Returns "" if the strings passed in is empty.


    Dim strFilter As String
    Dim intNum As Integer, intPos As Integer, intLastPos As Integer

    strFilter = ""
    intNum = 0
    intPos = 1
    intLastPos = 1

    ' Add strings as long as we find bars.
    ' Ignore any empty strings (not allowed).
    Do
        intPos = InStr(intLastPos, strFilterIn, "|")
        If (intPos > intLastPos) Then
            strFilter = strFilter & Mid(strFilterIn, intLastPos, intPos - intLastPos) & vbNullChar
            intNum = intNum + 1
            intLastPos = intPos + 1
        ElseIf (intPos = intLastPos) Then
            intLastPos = intPos + 1
        End If
    Loop Until (intPos = 0)
    
    ' Get last string if it exists (assuming strFilterIn was not bar terminated).
    intPos = Len(strFilterIn)
    If (intPos >= intLastPos) Then
        strFilter = strFilter & Mid(strFilterIn, intLastPos, intPos - intLastPos + 1) & vbNullChar
        intNum = intNum + 1
    End If

    ' Add *.* if there's no extension for the last string.
    If intNum Mod 2 = 1 Then
        strFilter = strFilter & "*.*" & vbNullChar
    End If

    ' Add terminating NULL if we have any filter.
    If strFilter <> "" Then
        strFilter = strFilter & vbNullChar
    End If

    MSAConvertFilterString = strFilter
End Function

Private Function MSAGetSaveFileName(msaof As MSAOPENFILENAME) As Integer
' Opens the file save dialog.

    Dim of As OPENFILENAME
    Dim intRet As Integer

    MSAOFtoOF msaof, of
    of.Flags = of.Flags Or OFNHIDEREADONLY
    intRet = GetSaveFileName(of)
    If intRet Then
        OFtoMSAOF of, msaof
    End If
    MSAGetSaveFileName = intRet
End Function

Function MSASimpleGetSaveFileName() As String
' Opens the file save dialog with default values.

    Dim msaof As MSAOPENFILENAME
    Dim intRet As Integer
    Dim strRet As String

    intRet = MSAGetSaveFileName(msaof)
    If intRet Then
        strRet = msaof.strFullPathReturned
    End If

    MSASimpleGetSaveFileName = strRet
End Function

Private Function MSAGetOpenFileName(msaof As MSAOPENFILENAME) As Integer
' Opens the Open dialog.

    Dim of As OPENFILENAME
    Dim intRet As Integer

    MSAOFtoOF msaof, of
    intRet = GetOpenFileName(of)
    If intRet Then
        OFtoMSAOF of, msaof
    End If
    MSAGetOpenFileName = intRet
End Function

Function MSASimpleGetOpenFileName() As String
' Opens the Open dialog with default values.

    Dim msaof As MSAOPENFILENAME
    Dim intRet As Integer
    Dim strRet As String

    intRet = MSAGetOpenFileName(msaof)
    If intRet Then
        strRet = msaof.strFullPathReturned
    End If

    MSASimpleGetOpenFileName = strRet
End Function

Private Sub OFtoMSAOF(of As OPENFILENAME, msaof As MSAOPENFILENAME)
' This sub converts from the Win32 structure to the Microsoft Access structure.

    msaof.strFullPathReturned = Left(of.lpstrFile, InStr(of.lpstrFile, vbNullChar) - 1)
    msaof.strFileNameReturned = of.lpstrFileTitle
    msaof.intFileOffset = of.nFileOffset
    msaof.intFileExtension = of.nFileExtension
End Sub

Private Sub MSAOFtoOF(msaof As MSAOPENFILENAME, of As OPENFILENAME)
' This sub converts from the Microsoft Access structure to the Win32 structure.

    Dim strFile As String * 512

    ' Initialize some parts of the structure.
    of.hwndOwner = Application.hWndAccessApp
    of.hInstance = 0
    of.lpstrCustomFilter = 0
    of.nMaxCustrFilter = 0
    of.lpfnHook = 0
    of.lpTemplateName = 0
    of.lCustrData = 0

    If msaof.strFilter = "" Then
        of.lpstrFilter = MSACreateFilterString(ALLFILES)
    Else
        of.lpstrFilter = msaof.strFilter
    End If
    of.nFilterIndex = msaof.lngFilterIndex

    of.lpstrFile = msaof.strInitialFile & String(512 - Len(msaof.strInitialFile), 0)
    of.nMaxFile = 511

    of.lpstrFileTitle = String(512, 0)
    of.nMaxFileTitle = 511

    of.lpstrTitle = msaof.strDialogTitle

    of.lpstrInitialDir = msaof.strInitialDir

    of.lpstrDefExt = msaof.strDefaultExtension

    of.Flags = msaof.lngFlags

    of.lStructSize = Len(of)
End Sub
