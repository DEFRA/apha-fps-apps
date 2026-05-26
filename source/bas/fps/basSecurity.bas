Option Compare Database   'Use database order for string comparisons
Option Explicit

' MsgBox parameters
Global Const MB_OK = 0                 ' OK button only
Global Const MB_OKCANCEL = 1           ' OK and Cancel buttons
Global Const MB_ABORTRETRYIGNORE = 2   ' Abort, Retry, and Ignore buttons
Global Const MB_YESNOCANCEL = 3        ' Yes, No, and Cancel buttons
Global Const MB_YESNO = 4              ' Yes and No buttons
Global Const MB_RETRYCANCEL = 5        ' Retry and Cancel buttons

Global Const MB_ICONSTOP = 16          ' Critical message
Global Const MB_ICONQUESTION = 32      ' Warning query
Global Const MB_ICONEXCLAMATION = 48   ' Warning message
Global Const MB_ICONINFORMATION = 64   ' Information message

Global Const MB_APPLMODAL = 0          ' Application Modal Message Box
Global Const MB_DEFBUTTON1 = 0         ' First button is default
Global Const MB_DEFBUTTON2 = 256       ' Second button is default
Global Const MB_DEFBUTTON3 = 512       ' Third button is default
Global Const MB_SYSTEMMODAL = 4096      'System Modal

' MsgBox return values
Global Const IDOK = 1                  ' OK button pressed
Global Const IDCANCEL = 2              ' Cancel button pressed
Global Const IDABORT = 3               ' Abort button pressed
Global Const IDRETRY = 4               ' Retry button pressed
Global Const IDIGNORE = 5              ' Ignore button pressed
Global Const IDYES = 6                 ' Yes button pressed
Global Const IDNO = 7                  ' No button pressed

'Application Security constants
Const ERR_NAME_NOT_IN_COLLCTN = 3265
Const ERR_ACCNT_ALREADY_EXISTS = 3390
Const ERR_BAD_PID = 3304
Const ERR_CANT_PERFORM_OPP = 3032
Const ERR_NO_PERMISSION = 3033
Const ERR_BAD_ACCNT_NAME = 3030
Const ERR_BAD_ACCNT_OR_PW = 3029

'Account type constants
Global Const ACCNT_USER = 1
Global Const ACCNT_GROUP = 2
Global Const ACCNT_NONE = 0

'Object type constants
Global Const OBJ_DATABASE = 0
Global Const OBJ_FORM = 1
Global Const OBJ_MODULE = 2
Global Const OBJ_REPORT = 3
Global Const OBJ_SCRIPT = 5
Global Const OBJ_MACRO = 5
Global Const OBJ_TABLE = 7

'API Constants
Const MAX_SIZE = 256

'API Declarations
'Get a string from a private INI file.  Returns the number of bytes
'copied into strReturned, not including the trailing null.
Declare PtrSafe Function glr_apiSecGetPrivateProfileString Lib "Kernel" Alias "GetPrivateProfileString" (ByVal strAppName As String, ByVal strKeyName As String, ByVal strDefault As String, ByVal strReturned As String, ByVal intSize As Integer, ByVal strFileName As String) As Integer

Function glrChangePW(ByVal pstrUser As String, ByVal pstrOldPW As String, ByVal pstrNewPW As String) As Integer

    ' Change the password for a user account.
    '
    ' (From The Microsoft Access Developer's Handbook
    ' by Ken Getz, Paul Litwin, and Greg Reddick, Sybex 1994)
    '
    ' In:
    '    pstrUser: name of user account
    '    pstrOldPW: old password
    '    pstrNewPW: new password
    ' Out:
    '     Return value: True (success) or False (failed).
    ' Example:
    '     intReturn = glrChangePW("Nikki", "Lucky7", "NoSuchLuck")

    On Error GoTo glrChangePWErr

    Dim wrk As Workspace
    Dim usr As User
    Dim strMsg As String
    Dim strProcName As String

    strProcName = "glrChangePW"
    
    'Intialize return value
    glrChangePW = False

    Set wrk = DBEngine.Workspaces(0)

    'Point to user object
    Set usr = wrk.Users(pstrUser)

    'Change password. If no permission or
    'bad PW, the error handler will kick in.
    usr.NewPassword pstrOldPW, pstrNewPW

    'Success
    glrChangePW = True

glrChangePWDone:
    On Error GoTo 0
    Exit Function

glrChangePWErr:
    Select Case Err
    Case ERR_NAME_NOT_IN_COLLCTN
        strMsg = "The user account '" & pstrUser & "' doesn't exist."
    Case ERR_NO_PERMISSION
        strMsg = "You don't have permission to perform this operation or you have entered the wrong old password."
    Case Else
        strMsg = "Error#" & Err & "--" & Error$(Err)
    End Select
        MsgBox strMsg, MB_ICONSTOP + MB_OK, "Procedure " & strProcName
    Resume glrChangePWDone

End Function

Function glrUserWithBlankPW(strCurrentUser As String) As Integer

    ' In:
    '    None
    ' Out:
    '     Return value: True (at least one blank password)
    '       or False (no blank passwords).
    ' Example:
    '     intReturn = glrUsersWithBlankPW()

On Error GoTo glrUserWithBlankPWErr

beginning:

    Dim wrkDefault As Workspace
    Dim wrkNew As Workspace
    Dim strUser As String
    Dim fNonBlankPW As Integer
    Dim fAnyBlankPWs As Integer
    Dim intI As Integer
    Dim strAccessDir As String
    Dim strMsg As String
    Dim strCR As String
    Dim strProcName As String

    'Intialize flag to track if any blank PWs occur
    fNonBlankPW = True

    Set wrkDefault = DBEngine.Workspaces(0)
    
    'Create a new workspace and attempt to log on as user with blank PW
    Set wrkNew = DBEngine.CreateWorkspace("NewWorkspace", strCurrentUser, "")

    
glrUserWithBlankPWDone:
    
    glrUserWithBlankPW = fNonBlankPW
    Exit Function

glrUserWithBlankPWErr:
    
    Select Case Err
    Case ERR_BAD_ACCNT_OR_PW
        'Could not log on user account with blank PW
        fNonBlankPW = False
        GoTo glrUserWithBlankPWDone
    Case Else
        ldoError Err, Error
    End Select
        
    Resume glrUserWithBlankPWDone

End Function