Option Compare Database
Option Explicit
Private Declare PtrSafe Function GetOpenFileName Lib "comdlg32.dll" Alias _
"GetOpenFileNameA" (pOpenfilename As OPENFILENAME) As Long

Private Type OPENFILENAME
    lStructSize As Long
    hwndOwner As LongPtr
    hInstance As LongPtr
    lpstrFilter As String
    lpstrCustomFilter As String
    nMaxCustFilter As Long
    nFilterIndex As Long
    lpstrFile As String
    nMaxFile As Long
    lpstrFileTitle As String
    nMaxFileTitle As Long
    lpstrInitialDir As String
    lpstrTitle As String
    flags As Long
    nFileOffset As Integer
    nFileExtension As Integer
    lpstrDefExt As String
    lCustData As LongPtr
    lpfnHook As LongPtr
    lpTemplateName As String
End Type



Function LaunchCD(strform As Form) As String
    Dim OpenFile As OPENFILENAME
    Dim lReturn As Long
    Dim sFilter As String
    OpenFile.lStructSize = Len(OpenFile)
    OpenFile.hwndOwner = strform.hwnd
    'sFilter = "All Files (*.*)" & Chr(0) & "*.*" & Chr(0) & _
     ' "JPEG Files (*.JPG)" & Chr(0) & "*.JPG" & Chr(0)
    sFilter = "Excel Files (*.xls)" & Chr(0) & "*.xls"
    OpenFile.lpstrFilter = sFilter
    OpenFile.nFilterIndex = 1
    OpenFile.lpstrFile = String(257, 0)
    OpenFile.nMaxFile = Len(OpenFile.lpstrFile) - 1
    OpenFile.lpstrFileTitle = OpenFile.lpstrFile
    OpenFile.nMaxFileTitle = OpenFile.nMaxFile
    OpenFile.lpstrInitialDir = "C:\"
    OpenFile.lpstrTitle = "Select a file using the Common Dialog DLL"
    OpenFile.flags = 0
    lReturn = GetOpenFileName(OpenFile)
        If lReturn = 0 Then
            MsgBox "A file was not selected!", vbInformation, _
              "Select a file using the Common Dialog DLL"
         Else
            LaunchCD = Trim(Left(OpenFile.lpstrFile, InStr(1, OpenFile.lpstrFile, vbNullChar) - 1))
         End If
End Function
   