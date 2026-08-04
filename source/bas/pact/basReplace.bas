Option Compare Database
Option Explicit

'**********  Code Start   ***********
Public Function Replace(Expression As String, Find As String, ReplaceWith As String, Optional Start As Integer = 1, Optional Count As Integer = -1, Optional Compare As Integer = vbBinaryCompare)


'*******************************************
'Name:      Replace (Function)
'Purpose:   Access97 version of the A2k Replace function
'Author:    Terry Kerft
'Date:      June 01, 2000, 02:44:34
'Called by: Any
'Calls:     None
'Inputs:    Expression - String to Search
'           Find - Sub-String to be replaced
'           ReplaceWith - Sub-String to insert
'           Start - Optional start of string to return and replace
'           Count - Optional number of replacements to carry out
'           Compare - Optional compare method to use _
            (valid values are 0, 1, 2)
'Output:    1) If Expression is zero-length then a zero-length string ("")
'           2) If Expression is Null then an error
'           2) If Find is zero-length then a copy of Expression.
'           3) If ReplaceWith is zero-length then a copy of _
               Expression with all occurences of find removed.
'           4) If Start > Len(Expression) then a zero-length string.
'           5) If Count is 0 then a copy of Expression.
'*******************************************


  Dim intInstr As Integer
  Dim intCount As Integer
  Dim intFindLen As Integer
  Dim intRepLen As Integer
  Dim strRet As String
  Dim strTemp As String
  On Error GoTo Replace_err


  intFindLen = Len(Find)
  intRepLen = Len(ReplaceWith)


  If Compare < vbBinaryCompare Or Compare > vbDatabaseCompare Then
    Err.Raise 1 + vbObjectError, Description:="Bad compare method"
  ElseIf Len(Expression) = 0 Or Start > Len(Expression) Then
    strRet = ""
  ElseIf Count = 0 Or intFindLen = 0 Then
    strRet = Expression
  Else
    strTemp = Mid(Expression, Start)
    intCount = Count
    intInstr = InStr(1, strTemp, Find, Compare)
    Do While intInstr > 0
      strRet = strRet & Left(strTemp, intInstr - 1) & ReplaceWith
      strTemp = Mid(strTemp, intInstr + Len(Find))
      intInstr = InStr(1, strTemp, Find, Compare)
      intCount = intCount - 1
      If intCount = 0 Then Exit Do
    Loop
  End If
  strRet = strRet & strTemp
Replace_end:
  Replace = strRet
Exit Function
Replace_err:
  strRet = ""
  With Err
    .Raise .Number, .Source, .Description, .HelpFile, .HelpContext
  End With
End Function
'***********  Code End    ***********