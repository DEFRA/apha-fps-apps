Option Compare Database   'Use database order for string comparisons
Option Explicit

'Declare Function GetPrivateProfileString Lib "Kernel" (ByVal lpApplicationName As String, lpKeyName As Any, ByVal lpDefault As String, ByVal lpReturnedString As String, ByVal nSize As Integer, ByVal lpFileName As String) As Integer
'Declare Function WritePrivateProfileString Lib "Kernel" (ByVal lpApplicationName As String, lpKeyName As Any, lpString As Any, ByVal lplFileName As String) As Integer

Function GetIni(lpSection As String, lpEntry As String) As Integer

    Const bufsize = 255
    Dim lpDefault As String, lpFileName As String, GotInfo As String
    Dim lpReturnVAl As String * 255
    On Error GoTo GetIni_Err

    lpDefault = "No Value"
    lpFileName = SysCmd(SYSCMD_INIFILE)
    GetIni = GetPrivateProfileString(lpSection, ByVal lpEntry, lpDefault, lpReturnVAl, bufsize, lpFileName)

    GotInfo = lpReturnVAl
    Exit Function

GetIni_Err:
    ldoError Err, Error
    GotInfo = lpDefault
    Exit Function

End Function

Function ldoGetMDA(lpSection As String, lpEntry As String) As String
    
    

    Const bufsize = 255
    Dim lpDefault As String, lpFileName As String, GotInfo As String
    Dim lpReturnVAl As String * 255, intTest As Integer
    On Error GoTo ldoGetMDA_Err

    lpDefault = "No Value"
    lpFileName = SysCmd(SYSCMD_INIFILE)
    intTest = GetPrivateProfileString(lpSection, ByVal lpEntry, lpDefault, lpReturnVAl, bufsize, lpFileName)
    ldoGetMDA = lpReturnVAl
    
Exit Function

ldoGetMDA_Err:

    ldoError Err, Error
    GotInfo = lpDefault
    Exit Function

End Function

Function SetIniFile()

On Error GoTo SetIniFile_Err

    Dim x As Integer
    x = GetIni("Options", "SystemDB")
    'Debug.Print x

Exit Function
SetIniFile_Err:

    ldoError Err, Error
    Resume Next

End Function

Function WriteIni(lpSection As String, lpEntry As String, lpString As String) As Integer

    Dim lpFileName As String
    On Error GoTo WriteIni_Err

    lpFileName = "fpstest.ini"
    WriteIni = WritePrivateProfileString(lpSection, ByVal lpEntry, ByVal lpString, lpFileName)
    Exit Function

WriteIni_Err:
    ldoError Err, Error
    Exit Function

End Function