Option Compare Database   'Use database order for string comparisons
Option Explicit

'This module demonstrates a technique for determining whether a user has the version
'of the Jet engine that corresponds with the Microsoft Access Service Pack.  The function
'FVerifyJetVersion() retrieves the file version information from MSAJT200.DLL via direct
'Windows API calls and compares the build version of the file with the JET_MINORVERSION
'constant below.  The build version that is being checked should not be confused with the
'information returned from the Version property of the DBEngine object.
'The Init() function should be called by an AUTOEXEC macro to ensure that all users only
'open the database with JET 2.5.  If a user is still using a version of JET prior to 2.5,
'a message will be displayed and Access will quit.  The informational message and shut down
'of the application can be changed or commented out to meet individual needs.
'See the READSRV.TXT file installed with the Service Pack for more details.

'These are necessary to define the JET engine file name and the minimum JET drop number.
Const JET_FILENAME = "msajt200.dll"
'This is not the same as the version property from the DBEngine object.  This number
'should not be modified.
Const JET_MINORVERSION = 1100
    
' Type returned by VER.DLL GetFileVersionInfo
Type VS_FIXEDFILEINFO
    wTolLen As Integer
    wValLen As Integer
    szSig As String * 16
    dwSignature As Long             '/* e.g. 0xfeef04bd */
    dwStrucVersion As Long          '/* e.g. 0x00000042 = "0.42" */
    dwFileVersionMS As Long         '/* e.g. 0x00030075 = "3.75" */
    dwFileVersionLS As Long         '/* e.g. 0x00000031 = "0.31" */
    dwProductVersionMS As Long      '/* e.g. 0x00030010 = "3.10" */
    dwProductVersionLS As Long      '/* e.g. 0x00000031 = "0.31" */
    dwFileFlagsMask As Long         '/* = 0x3F for version "0.42" */
    dwFileFlags As Long             '/* e.g. VFF_DEBUG | VFF_PRERELEASE */
    dwFileOS As Long                '/* e.g. VOS_DOS_WINDOWS16 */
    dwFileType As Long              '/* e.g. VFT_DRIVER */
    dwFileSubtype As Long           '/* e.g. VFT2_DRV_KEYBOARD */
    dwFileDateMS As Long            '/* e.g. 0 */
    dwFileDateLS As Long            '/* e.g. 0 */
End Type

' User defined type so we can copy into the type above using LSet
Type fBuffer
    item As String * 1024
End Type

Declare PtrSafe Function GetFileVersionInfoSize Lib "ver.dll" (ByVal stFileName As String, ByVal stTmp As String) As Long
Declare PtrSafe Function GetFileVersionInfo Lib "ver.dll" (ByVal stFileName As String, ByVal hVersionInfo As Long, ByVal lSize As Long, ByVal stBuf As String) As Integer

Function FVerifyJetVersion()

    Dim Buffer As fBuffer
    Dim vInfo As VS_FIXEDFILEINFO
    Dim stBuf As String
    Dim lSize As Long
    Dim stUnused As String * 4
    Dim ErrCode As Long
    Dim stMajor As String
    Dim stMinor As String
    Dim VerNum As String

    lSize = GetFileVersionInfoSize(JET_FILENAME, stUnused)
    stBuf = String$(lSize + 1, 0)
    ErrCode = GetFileVersionInfo(JET_FILENAME, 0&, lSize, stBuf)

    If ErrCode <> 0 Then
        Buffer.item = stBuf
        LSet vInfo = Buffer
        FVerifyJetVersion = ((vInfo.dwProductVersionLS And &HFFFF&) > JET_MINORVERSION)
    Else
        FVerifyJetVersion = False
    End If

End Function

Function Init() As String
On Error GoTo Init_Err

' Verify correct version of jet
    If FVerifyJetVersion() Then
'       This can be uncommented to verify that you are running JET 2.5
        'MsgBox "You are running the correct version of JET"
        Init = "2.5"
    Else
'       Message to inform user that they are running JET 2.0
        'MsgBox "You must be running JET 2.5 to access the database.  Copy the 2.5 version of MSAJT200.DLL to your {windows}\system directory and restart."
        Init = "2.0"
'       This closes the Access application
        'Application.Quit
        'GoTo Init_Exit
    End If


Init_Exit:
On Error Resume Next
    Exit Function

Init_Err:
    MsgBox Error$
    Resume Init_Exit
End Function