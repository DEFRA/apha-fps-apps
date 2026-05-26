Option Compare Database   'Use database order for string comparisons
Option Explicit

Function glrIsMember(ByVal pstrUser As String, ByVal pstrGroup As String) As Integer

    ' Check if a user account is a member of a group account.
    '
    ' (From The Microsoft Access Developer's Handbook
    ' by Ken Getz, Paul Litwin, and Greg Reddick, Sybex 1994)
    '
    ' In:
    '    pstrName: name of user account
    '    pstrGroup: name of group account
    ' Out:
    '     Return value: True (is member) or False (not a
    '       member or either account doesn't exist).
    ' Example:
    '     intIsMember = glrIsMember("Alicia", "Librarians")

    On Error GoTo glrIsMemberErr
    
    Dim wrk As Workspace
    Dim usr As User
    Dim gru As Group
    Dim strMsg As String
    Dim intErrHndlrFlag As Integer
    Dim varGroupName As Variant
    Dim strProcName As String

    Const FLAG_SET_USER = 1
    Const FLAG_SET_GROUP = 2
    Const FLAG_CHK_MEMBER = 4
    Const FLAG_ELSE = 0
    Const MB_OK = 0

    strProcName = "glrIsMember"

    'Intialize return value
    glrIsMember = False

    'Initialize flag for determining
    'context for error handler
    intErrHndlrFlag = FLAG_ELSE
    
    Set wrk = DBEngine.Workspaces(0)

    'Refresh users and groups collections
    wrk.Users.Refresh
    wrk.Groups.Refresh

    'intErrHndlrFlag = FLAG_SET_USER
    Set usr = wrk.Users(pstrUser)

    'intErrHndlrFlag = FLAG_SET_GROUP
    Set gru = wrk.Groups(pstrGroup)

    'intErrHndlrFlag = FLAG_CHK_MEMBER
    varGroupName = usr.Groups(pstrGroup).Name

    'intErrHndlrFlag = FLAG_ELSE
    
    If Not IsEmpty(varGroupName) Then
        glrIsMember = True
    End If

glrIsMemberDone:
    'On Error GoTo glrIsMemberErr:
    Exit Function

glrIsMemberErr:
    
   'MsgBox "The application encountered unexpected error #" & Err & " with message string '" & Error & "'", MB_OK, "Please inform ITU ! - from ldoIsMemberChk"
   glrIsMember = False
   Exit Function
'Application.Quit

End Function