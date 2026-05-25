Option Compare Database
Option Explicit
 
Public Const GHND = &H42
Public Const MAXSIZE = 4096
Public Const CF_TEXT = 1
Public Const CF_RTF = 49232
Private Const CF_ANSIONLY = &H400&
Private Const CF_APPLY = &H200&
Private Const CF_BITMAP = 2
Private Const CF_DIB = 8
Private Const CF_DIF = 5
Private Const CF_DSPBITMAP = &H82
Private Const CF_DSPENHMETAFILE = &H8E
Private Const CF_DSPMETAFILEPICT = &H83
Private Const CF_DSPTEXT = &H81
Private Const CF_EFFECTS = &H100&
Private Const CF_ENABLEHOOK = &H8&
Private Const CF_ENABLETEMPLATE = &H10&
Private Const CF_ENABLETEMPLATEHANDLE = &H20&
Private Const CF_ENHMETAFILE = 14
Private Const CF_FIXEDPITCHONLY = &H4000&
Private Const CF_FORCEFONTEXIST = &H10000
Private Const CF_GDIOBJFIRST = &H300
Private Const CF_GDIOBJLAST = &H3FF
Private Const CF_HDROP = 15
Private Const CF_INITTOLOGFONTSTRUCT = &H40&
Private Const CF_LIMITSIZE = &H2000&
Private Const CF_LOCALE = 16
Private Const CF_MAX = 17
Private Const CF_METAFILEPICT = 3
Private Const CF_NOFACESEL = &H80000
Private Const CF_NOSCRIPTSEL = &H800000
Private Const CF_NOSIMULATIONS = &H1000&
Private Const CF_NOSIZESEL = &H200000
Private Const CF_NOSTYLESEL = &H100000
Private Const CF_NOVECTORFONTS = &H800&
Private Const CF_NOOEMFONTS = CF_NOVECTORFONTS
Private Const CF_NOVERTFONTS = &H1000000
Private Const CF_OEMTEXT = 7
Private Const CF_OWNERDISPLAY = &H80
Private Const CF_PALETTE = 9
Private Const CF_PENDATA = 10
Private Const CF_PRINTERFONTS = &H2
Private Const CF_PRIVATEFIRST = &H200
Private Const CF_PRIVATELAST = &H2FF
Private Const CF_RIFF = 11
Private Const CF_SCALABLEONLY = &H20000
Private Const CF_SCREENFONTS = &H1
Private Const CF_BOTH = (CF_SCREENFONTS Or CF_PRINTERFONTS)
Private Const CF_SCRIPTSONLY = CF_ANSIONLY
Private Const CF_SELECTSCRIPT = &H400000
Private Const CF_SHOWHELP = &H4&
Private Const CF_SYLK = 4
Private Const CF_TIFF = 6
Private Const CF_TTONLY = &H40000
Private Const CF_UNICODETEXT = 13
Private Const CF_USESTYLE = &H80&
Private Const CF_WAVE = 12
Private Const CF_WYSIWYG = &H8000
 
Private Declare PtrSafe Function GlobalAlloc Lib "kernel32" (ByVal wFlags As Long, ByVal _
  dwBytes As LongPtr) As LongPtr
Private Declare PtrSafe Function GlobalLock Lib "kernel32" (ByVal hMem As LongPtr) _
  As LongPtr
Private Declare PtrSafe Function GlobalSize Lib "kernel32" (ByVal hMem As LongPtr) _
  As LongPtr
Private Declare PtrSafe Function lstrcpy Lib "kernel32" (ByVal lpString1 As Any, _
  ByVal lpString2 As Any) As LongPtr
Private Declare PtrSafe Function GlobalUnlock Lib "kernel32" (ByVal hMem As LongPtr) _
  As Long
 
Private Declare PtrSafe Function OpenClipboard Lib "user32" (ByVal hwnd As LongPtr) _
  As Long
Private Declare PtrSafe Function CloseClipboard Lib "user32" () As Long
Private Declare PtrSafe Function GetClipboardData Lib "user32" (ByVal wFormat As _
  Long) As LongPtr
Private Declare PtrSafe Function EmptyClipboard Lib "user32" () As Long
Private Declare PtrSafe Function SetClipboardData Lib "user32" (ByVal wFormat _
  As Long, ByVal hMem As LongPtr) As LongPtr
 
Private Declare PtrSafe Function RegisterClipboardFormat Lib "user32" Alias _
    "RegisterClipboardFormatA" (ByVal lpString As String) As Long
 
Private Declare PtrSafe Sub CopyMemory Lib "kernel32" Alias "RtlMoveMemory" ( _
    ByVal Destination As Long, Source As Any, ByVal Length As LongPtr)
    
Private Declare PtrSafe Function GlobalFree Lib "kernel32" ( _
     ByVal hMem As LongPtr) As LongPtr
    
Private Const GMEM_DDESHARE = &H2000
Private Const GMEM_MOVEABLE = &H2
 
Public Function copyRTFToClipBoard(sRTF As String)

' This no longer seems to be called (unless from a button on a form I've not found yet)
' so has been commented out. If used, all lines beginning "Quote-hyphen-hyphen" must be uncommented
' Without commenting out it won't compile'

'sRTF represents the rich text formatted string to paste into Word
 
'Copy the contents of the Rich Text to the clipboard
'--Dim lSuccess As Long
'--Dim lRTF As Long
'--Dim hGlobal As Long
'--Dim lpString As Long
'lSuccess = OpenClipboard(Me.hwnd)
'--lSuccess = OpenClipboard(0&)
'--lRTF = RegisterClipboardFormat("Rich Text Format")
'--lSuccess = EmptyClipboard
'--hGlobal = GlobalAlloc(GMEM_MOVEABLE Or GMEM_DDESHARE, Len(sRTF))
'--lpString = GlobalLock(hGlobal)
 
'--CopyMemory lpString, ByVal sRTF, Len(sRTF)
'--GlobalUnlock hGlobal
'--SetClipboardData lRTF, hGlobal
'--CloseClipboard
'--GlobalFree hGlobal
   
End Function
 
Function ClipBoard_SetText(strCopyString As String) As Boolean
  Dim hGlobalMemory As LongPtr
  Dim lpGlobalMemory As LongPtr
  Dim hClipMemory As LongPtr
 
  ' Allocate moveable global memory.
  '-------------------------------------------
  hGlobalMemory = GlobalAlloc(GHND, Len(strCopyString) + 1)
 
  ' Lock the block to get a far pointer
  ' to this memory.
  lpGlobalMemory = GlobalLock(hGlobalMemory)
 
  ' Copy the string to this global memory.
  lpGlobalMemory = lstrcpy(lpGlobalMemory, strCopyString)
 
  ' Unlock the memory and then copy to the clipboard
  If GlobalUnlock(hGlobalMemory) = 0 Then
    If OpenClipboard(0&) <> 0 Then
      Call EmptyClipboard
      hClipMemory = SetClipboardData(CF_TEXT, hGlobalMemory)
      ClipBoard_SetText = CBool(CloseClipboard)
    End If
  End If
End Function
 
Function getRTFFromClipBoard()
Dim lRTF As Long
lRTF = RegisterClipboardFormat("Rich Text Format")
 
getRTFFromClipBoard = ClipBoard_GetText(lRTF)
 
End Function
 

Function ClipBoard_GetText(Optional varClipboardFormat = CF_TEXT) As String
  Dim hClipMemory As LongPtr
  Dim lpClipMemory As LongPtr
  Dim strCBText As String
  Dim RetVal As LongPtr
 
  If OpenClipboard(0&) <> 0 Then
    ' Obtain the handle to the global memory
    ' block that is referencing the text.
    hClipMemory = GetClipboardData(varClipboardFormat)
    If hClipMemory <> 0 Then
      ' Lock Clipboard memory so we can reference
      ' the actual data string.
      lpClipMemory = GlobalLock(hClipMemory)
      If lpClipMemory <> 0 Then
        strCBText = Space$(MAXSIZE)
        RetVal = lstrcpy(strCBText, lpClipMemory)
        RetVal = GlobalUnlock(hClipMemory)
        ' Peel off the null terminating character.
        'writeTxt strCBText, "rtftest.txt", False
        strCBText = Left(strCBText, InStr(1, strCBText, Chr$(0), 0) - 1)
      Else
        MsgBox "Could not lock memory to copy string from."
      End If
    End If
    Call CloseClipboard
  End If
  ClipBoard_GetText = strCBText
End Function
 
Function CopyOlePiccy(Piccy As Object)
  Dim hGlobalMemory As LongPtr, lpGlobalMemory As LongPtr
  Dim hClipMemory As LongPtr, x As Long
 
  ' Allocate moveable global memory.
  '-------------------------------------------
  hGlobalMemory = GlobalAlloc(GHND, Len(Piccy) + 1)
 
  ' Lock the block to get a far pointer
  ' to this memory.
  lpGlobalMemory = GlobalLock(hGlobalMemory)
 

  'Need to copy the object to the memory here
 
  lpGlobalMemory = lstrcpy(lpGlobalMemory, Piccy)
 
  ' Unlock the memory.
  If GlobalUnlock(hGlobalMemory) <> 0 Then
    MsgBox "Could not unlock memory location. Copy aborted."
    GoTo OutOfHere2
  End If
 
  ' Open the Clipboard to copy data to.
  If OpenClipboard(0&) = 0 Then
    MsgBox "Could not open the Clipboard. Copy aborted."
    Exit Function
  End If
 
  ' Clear the Clipboard.
  x = EmptyClipboard()
 
  ' Copy the data to the Clipboard.
  hClipMemory = SetClipboardData(CF_TEXT, hGlobalMemory)
 
OutOfHere2:
  If CloseClipboard() = 0 Then
    MsgBox "Could not close Clipboard."
  End If
End Function
 

 