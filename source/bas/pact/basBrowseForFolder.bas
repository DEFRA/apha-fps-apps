Option Compare Database
Option Explicit


'This module contains all the declarations to use the
'Windows 95 Shell API to use the browse for folders
'dialog box. To use the browse for folders dialog box,
'please call the BrowseForFolders function using the
'syntax: stringFolderPath=BrowseForFolders(Hwnd,TitleOfDialog)
'
'For contacting information, see other module



Public Type BrowseInfo
     hwndOwner As LongPtr
     pIDLRoot As LongPtr
     pszDisplayName As Long
     lpszTitle As String
     ulFlags As Long
     lpfnCallback As LongPtr
     lParam As LongPtr
     iImage As Long
End Type

Public Const BIF_RETURNONLYFSDIRS = 1
Public Const MAX_PATH = 260

Public Declare PtrSafe Sub CoTaskMemFree Lib "ole32.dll" (ByVal hMem As LongPtr)
Public Declare PtrSafe Function lstrcat Lib "kernel32" Alias "lstrcatA" (ByVal lpString1 As String, ByVal lpString2 As String) As LongPtr
Public Declare PtrSafe Function SHBrowseForFolder Lib "shell32" (lpbi As BrowseInfo) As Long
Public Declare PtrSafe Function SHGetPathFromIDList Lib "shell32" (ByVal pidList As LongPtr, ByVal lpBuffer As String) As Long

Public Function BrowseForFolder(hwndOwner As LongPtr, sPrompt As String) As String
      
    'declare variables to be used
     Dim iNull As Integer
     Dim lpIDList As Long
     Dim lResult As Long
     Dim sPath As String
     Dim udtBI As BrowseInfo

    'initialise variables
     With udtBI
   
        .hwndOwner = hwndOwner
        .lpszTitle = lstrcat(sPrompt, "")
        .ulFlags = BIF_RETURNONLYFSDIRS
     End With

    'Call the browse for folder API
     lpIDList = SHBrowseForFolder(udtBI)
      
    'get the resulting string path
     If lpIDList Then
        sPath = String$(MAX_PATH, 0)
        lResult = SHGetPathFromIDList(lpIDList, sPath)
        Call CoTaskMemFree(lpIDList)
        iNull = InStr(sPath, vbNullChar)
        If iNull Then sPath = Left$(sPath, iNull - 1)
     End If

    'If cancel was pressed, sPath = ""
     BrowseForFolder = sPath

End Function


'Form code

'Private Sub cmdServerBrowse_Click()
' txtDatabasePath.Text = BrowseForFolder(hwnd, "Please select a Server folder.")
'End Sub