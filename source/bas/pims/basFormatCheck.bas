MODULE NAME: basFormatCheck
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit
Public Function ReformatField(varDataIn As Variant, Spec As String, Optional blnQuietMode = False, Optional varReturnIfError = Null, Optional varReplace As Variant = Null) As Variant
' Calls ReformatFieldOneFormat for each specification in Spec
' Multiple specs are delimited by brackets e.g. "(00/00000)(00-0000-00)"
' Single specs have no delimiters e.g "00/00000"

' If blnQuietMode is true no error messages are returned
' If  varReturnIfError specified then that is what will be returned if data is invalid.
' If varReturnIfError contains the string "[data]" then it will be replaced with varData
'  e.g. (Check}[data] would return "{check}" followed by the contents of varData

' If varReplace specified then incoming varData can have strings replaced before processing e.g. replace "/" with "-"
' It must be formatted thus: "oldString1~newString1~oldString2~newString2" or you can use chr(0) instead of ~
' e.g. "/~-~none~N/A"   (replace all "/" with "-" and all "none" with "N/A")
' e.g. "SN/~SN~SN~SN/"  (replace all "SN/" with "SN", then all "SN//" with "SN/")
' not e that each replace command is executed on whole string, so example 2 will
' effectively ensure that all "SN" and "SN/" end up as "SN/"

Dim arSpecs(), ar As Integer, p As Integer, x As String, blnOK As Boolean, varResult
Dim arReplace(), arR As Integer
Dim varData As Variant

blnOK = True
If IsNull(varDataIn) Then
    ReformatField = Null
    Exit Function
End If

varData = varDataIn

If Not IsNull(varReplace) Then
    arR = breakApart(varReplace, "~", arReplace(), False)
    If arR = 0 Then
        arR = breakApart(varReplace, Chr(0), arReplace(), False)
    End If
End If

For p = 1 To arR Step (2)
    varData = replaceChars(varData, CStr(arReplace(p)), CStr(arReplace(p + 1)))
Next

If Left$(Spec, 1) = "(" Then
    ar = 0: p = 1
    Do While p < Len(Spec)
        If Mid$(Spec, p, 1) = "(" Then
            x = "": p = p + 1
            Do While Mid$(Spec, p, 1) <> ")" And p < Len(Spec)
                x = x & Mid$(Spec, p, 1)
                p = p + 1
            Loop
            ar = ar + 1
            ReDim Preserve arSpecs(ar)
            arSpecs(ar) = x
            If p < Len(Spec) Then p = p + 1
        Else
            msgDataError "Format incorrect!"
            ReformatField = Null
            blnOK = False
            Exit Do
        End If
    Loop
    If blnOK Then
        ' should now have all specs in array
        blnOK = False
        For p = 1 To ar
            varResult = ReformatFieldOneFormat(varData, CStr(arSpecs(p)), True)
            If Not IsNull(varResult) Then
                ReformatField = varResult
                blnOK = True
                Exit For
            End If
            ' if it didn't work with varData and replacements were made, try without replacements
            If varDataIn <> varData Then
                varResult = ReformatFieldOneFormat(varDataIn, CStr(arSpecs(p)), True)
                If Not IsNull(varResult) Then
                    ReformatField = varResult
                    blnOK = True
                    Exit For
                End If
            End If
        Next
        If Not blnOK Then
            If Not blnQuietMode Then
                msgDataError "Incorrect format"
            End If
            If IsNull(varReturnIfError) Then
                ReformatField = Null
            Else
                ReformatField = replaceChars(varReturnIfError, "[data]", CStr(varDataIn))
            End If
        End If
    End If
Else
    ReformatField = ReformatFieldOneFormat(varData, Spec, blnQuietMode)
End If
End Function


Public Function ReformatFieldOneFormat(varData As Variant, Spec As String, Optional blnQuietMode = False) As Variant
' Format is specified as a pattern
'
' nnn       any number up to this many digits
' 999       any number up to this many digits. Leading zeroes removed
' NN        fixed number of digits
' 00        fixed number of digits with leading zeroes
' xxx       any number of characters up to this many
' XXX       Any character. No. of characters required = no. of Xs
' aaa       as xxx but letters only
' AAA       as XXX but letters only
' [xx]      characters between brackets are literal and required
'           note: you can specify a tilde-separated list of alternative literals. Any is accepted but the first is always output
'                 e.g. [/~-~#] will accept "-" or "#" but always outputs "/" in that position
' yy        2 digit year
' yyyy      4 digit year
' *         Any number of any characters. If followed by literal, all characters up to that literal
'
' ¬         Truncate. Any data from here will be truncated. eg. "01-123-23" using "00-0000¬" returns "01-0123"
'
' Any other character is a literal

' >>> AND <<< just to make it difficult, multiple formats in brackets are allowed e.g. (00/00000)(00-0000-00)

' e.g. "yy\00000"   2 digit year, literal "\", 5 digits with leading 0
'      "NN/n"       2 digits, literal "/", any number of digits

Dim strSpec As String, strType As String, p As Integer, arP As Integer, strThisSpec As String
Dim i As Integer

Dim arSpec(10, 5) As String ' 10 rows, 1 for each possible section of code
                            ' each row shows 1. Type, 2. Length, 3. section size if fixed, 4 actual spec string, 5 alternative literals
Dim intLen As Integer, strDelim As String, strSection As String, strSectionContents As String
Dim s As Integer, Msg As String, msg2 As String, strNew As String
Dim arLits(), intLit As Integer

Const AR_TYPE = 1, AR_LENGTH = 2, AR_DELIM = 3, AR_CONTENTS = 4, FORMAT_CHARS = "9nN0oOxXaAyY[*¬", AR_ALTERNATIVES = 5

strType = "": p = 1: arP = 1
strSpec = Spec & Chr(0)

' Fill the spec array from the supplied spec

Do While Mid$(strSpec, p, 1) <> Chr(0)
    strType = Mid$(strSpec, p, 1)
    strThisSpec = ""
    Do While Mid$(strSpec, p, 1) = strType Or (strType = "[") And Mid$(strSpec, p, 1) <> "]"
        strThisSpec = strThisSpec & Mid$(strSpec, p, 1)
        p = p + 1
    Loop
    ' Store this section's format
    arSpec(arP, AR_TYPE) = strType
    arSpec(arP, AR_LENGTH) = Len(strThisSpec)
    arSpec(arP, AR_DELIM) = ""
    
    ' Look for a delimiter
    If strType = "[" Then
        arSpec(arP, AR_CONTENTS) = Mid$(strThisSpec, 2)
        arSpec(arP, AR_DELIM) = Mid$(strSpec, p, 1)
        p = p + 1
    Else
        arSpec(arP, AR_CONTENTS) = strThisSpec
        Do While InStr(FORMAT_CHARS & "]" & Chr(0), Mid$(strSpec, p, 1)) = 0
            ' a delimiter is next
            arSpec(arP, AR_DELIM) = arSpec(arP, AR_DELIM) & Mid$(strSpec, p, 1)
            p = p + 1
        Loop
    End If
    If arSpec(arP, AR_DELIM) = "]" Then
        arSpec(arP, AR_LENGTH) = arSpec(arP, AR_LENGTH) - 1
        arSpec(arP, AR_DELIM) = ""
    End If
    
    ' next section
    arP = arP + 1
Loop

' Display the sections for testing
'For i = 1 To arP - 1
'    Debug.Print arSpec(i, AR_TYPE), arSpec(i, AR_LENGTH), arSpec(i, AR_DELIM), arSpec(i, AR_CONTENTS)
'Next

' process the sections
s = 1 ' start point
strNew = ""

For i = 1 To arP - 1
    strType = arSpec(i, AR_TYPE)
    intLen = arSpec(i, AR_LENGTH)
    strDelim = arSpec(i, AR_DELIM)
    strSectionContents = arSpec(i, AR_CONTENTS)
    
    If strType = "[" Then
        If InStr(strSectionContents, "~") > 0 Then ' alternative literals - always output first one regardless of match
            intLit = breakApart(strSectionContents, "~", arLits())
            For p = 1 To intLit
                If Mid$(varData, s, Len(arLits(p))) = arLits(p) Then
                    strSectionContents = arLits(1)
                    Exit For
                End If
            Next
            If p > intLit Then
                Msg = "Invalid format: expecting '" & arLits(1) & "' at character " & CStr(s)
                GoTo badData
            End If
        Else
            If Mid$(varData, s, intLen) <> strSectionContents Then
                Msg = "Invalid format: expecting '" & strSectionContents & "' at character " & CStr(s)
                GoTo badData
            End If
        End If
        strSection = strSectionContents
    Else
        strSection = ReformatField_GetSection(s, strType, intLen, varData, strDelim, Msg)
    End If
    
    If Msg <> "" Then
        GoTo badData
    End If
    
    Select Case Asc(strType)
        Case Asc("¬")            ' Truncate from here
            Exit For
            
        Case Asc("*")            ' All remaining characters
            strNew = strNew & strSection & strDelim
            
        Case Asc("[")            ' Literals
            strNew = strNew & strSection & strDelim
        
        Case Asc("9")            ' any number of digits, leading zeroes removed
            If Not IsReallyNumeric(strSection) Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Val(strSection) < 0 Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Len(CStr(Val(strSection))) > intLen Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric, max " & CStr(intLen) & " digits": GoTo badData
            strNew = strNew & Trim(CStr(Val(strSection))) & strDelim
            
        Case Asc("n")            ' any number of digits
            If Not IsReallyNumeric(strSection) Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Val(strSection) < 0 Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Len(strSection) > intLen Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric, max " & CStr(intLen) & " digits": GoTo badData
            strNew = strNew & Trim(strSection) & strDelim
            
        Case Asc("N")            ' fixed number of digits
            If Not IsReallyNumeric(strSection) Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Val(strSection) < 0 Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Len(CStr(Val(strSection))) > intLen Then Msg = "Invalid format at character " & CStr(s) & ": number too big": GoTo badData
            strNew = strNew & Format(Val(strSection), String(intLen, "@")) & strDelim
            
        Case Asc("0"), Asc("o"), Asc("O") ' fixed number digits, leading zeroes
            If Not IsReallyNumeric(strSection) Then Msg = "Invalid format: should be numeric": GoTo badData
            If Val(strSection) < 0 Then Msg = "Invalid format at character " & CStr(s) & ": should be numeric": GoTo badData
            If Len(CStr(Val(strSection))) > intLen Then Msg = "Invalid format at character " & CStr(s) & ": number too big": GoTo badData
            strNew = strNew & Format(Val(strSection), String(intLen, "0")) & strDelim
        
        Case Asc("x")            ' any number of characters
            If Len(strSection) > intLen Then Msg = "Invalid format at character " & CStr(s) & ": too many characters": GoTo badData
            strNew = strNew & strSection & strDelim
        
        Case Asc("X")            ' fixed number of characters
            If Len(strSection) <> intLen Then Msg = "Invalid format at character " & CStr(s) & ": " & CStr(intLen) & " characters required": GoTo badData
            strNew = strNew & strSection & strDelim
        
        Case Asc("a")            ' any number of letters
            If Not IsNull(GetNumberFromText(strSection)) Then Msg = "Invalid format at character " & CStr(s) & ": should be letters": GoTo badData
            strNew = strNew & strSection & strDelim
        
        Case Asc("A")            ' fixed number of letters
            If Not IsNull(GetNumberFromText(strSection)) Then Msg = "Invalid format at character " & CStr(s) & ": should be letters": GoTo badData
            If Len(strSection) <> intLen Then Msg = "Invalid format at character " & CStr(s) & ": " & CStr(intLen) & "letters required": GoTo badData
            strNew = strNew & strSection & strDelim
        
        Case Asc("y"), Asc("Y")  ' 2 digit year range 00 to 99
            If intLen = 2 Then
                If Not IsReallyNumeric(strSection) Then Msg = "Invalid format at character " & CStr(s) & ": should be yy": GoTo badData
                If Len(strSection) > intLen Then Msg = "Invalid format at character " & CStr(s) & ": should be yy": GoTo badData
                strNew = Format(Val(strSection), String(intLen, "0")) & strNew & strDelim
            Else
                If Not IsReallyNumeric(strSection) Then Msg = "Invalid format at character " & CStr(s) & ": should be yy": GoTo badData
                If Len(strSection) <> 4 Then Msg = "Invalid format at character " & CStr(s) & ": should be yyyy": GoTo badData
                strNew = strNew & strSection & strDelim
            End If
        Case Else ' must be a literal
            If strType = strSection Then
                strNew = strNew & strSection & strDelim
            Else
                Msg = "Invalid format at character " & CStr(s) & ": expected " & strType: GoTo badData
            End If
        
    End Select
    s = s + Len(strSection) + Len(strDelim)

Next

If strType <> "¬" And s < Len(varData) + 1 Then
    Msg = "Invalid format at character " & CStr(s) & ": too many characters": GoTo badData
End If
ReformatFieldOneFormat = UCase$(strNew)
Exit Function

badData:
    If Not blnQuietMode Then msgDataError Msg
    ReformatFieldOneFormat = Null
    
End Function


Function ReformatField_GetSection(intStartPoint, strType, intLen As Integer, strData, strDelim, Msg As String) As Variant
Dim p As Integer
Msg = ""
If strDelim <> "" Then
    p = InStr(intStartPoint, strData, strDelim)
    If p = 0 Then
        Msg = "Missing " & strDelim
        Exit Function
    End If
    ReformatField_GetSection = Mid$(strData, intStartPoint, p - intStartPoint)
    Exit Function
Else
    If strType = "*" Then
        ReformatField_GetSection = Mid$(strData, intStartPoint)
        
    ElseIf Asc(UCase(strType)) = Asc(strType) And InStr("90", strType) = 0 Then  ' uppercase so fixed length
        ReformatField_GetSection = Mid$(strData, intStartPoint, intLen)
        
    Else
        p = intStartPoint
        If InStr("9n0y", strType) > 0 Then
            Do While (Mid$(strData & "*", p, 1) = " "): p = p + 1: Loop
            Do While IsReallyNumeric(Mid$(strData & Chr(0), p, 1)): p = p + 1: Loop
        ElseIf InStr("a", strType) > 0 Then
            Do While Not IsReallyNumeric(Mid$(strData & Chr(0), p, 1)) And p < intStartPoint + intLen: p = p + 1: Loop
        Else
            p = Len(strData) + 1
        End If
        ReformatField_GetSection = Mid$(strData, intStartPoint, p - intStartPoint)
    End If
    Exit Function
End If

End Function


Public Function StandardiseText(var, Optional IncludeDates As Boolean = False) As Variant
' Can be used to ensure certain text values are standardised in otherwise free-format fields
'
' e.g. "n/a", "n\a", "na" all become "N/A"
' Dates in text fields can also be formatted as dd mmm yyyy

Select Case var
    Case Null:  StandardiseText = Null: Exit Function
    Case IncludeDates And IsDate(var):    var = Format(var, "dd mmm yyyy")
    Case "na":  StandardiseText = "N/A"
    Case "n/a":  StandardiseText = "N/A"
    Case "n\a":  StandardiseText = "N/A"
    Case "n a":  StandardiseText = "N/A"
    Case "Not known":  StandardiseText = "N/A"
    Case "Not given":  StandardiseText = "N/A"
    Case "None":  StandardiseText = "N/A"
    Case "N/K":  StandardiseText = "N/A"
    Case "N.A":  StandardiseText = "N/A"
    Case "N-A":  StandardiseText = "N/A"
    Case "unknown":  StandardiseText = "Unknown"
    Case Else: StandardiseText = var
End Select

End Function

Public Function StandardiseText2(var, Optional IncludeDates As Boolean = False) As Variant
' Can be used to ensure certain text values are standardised in otherwise free-format fields
'
' e.g. "n/a", "n\a", "na" all become "N/A"
' Dates in text fields can also be formatted as dd mmm yyyy

Select Case var
    Case Null:  StandardiseText2 = Null: Exit Function
    Case IncludeDates And IsDate(var):    var = Format(var, "dd mmm yyyy")
    Case "na":  StandardiseText2 = "N/A"
    Case "n/a":  StandardiseText2 = "N/A"
    Case "n\a":  StandardiseText2 = "N/A"
    Case "n a":  StandardiseText2 = "N/A"
    Case "Not known":  StandardiseText2 = "N/A"
    Case "Not given":  StandardiseText2 = "N/A"
    Case "None":  StandardiseText2 = "N/A"
    Case "N/K":  StandardiseText2 = "N/A"
    Case "N.A":  StandardiseText2 = "N/A"
    Case "N-A":  StandardiseText2 = "N/A"
    Case "Unknown":  StandardiseText2 = "N/A"
    Case "No Ref":  StandardiseText2 = "N/A"
    Case "No reference":  StandardiseText2 = "N/A"
    Case "None given":  StandardiseText2 = "N/A"
    Case "None reference":  StandardiseText2 = "N/A"
    Case Else: StandardiseText2 = var
End Select

End Function


Public Function getTagType(TagValue) As Variant

' tagValue  format must be examined to determine the type of tag.
' Not easy, they vary a lot.
' Sometimes the project can be used to tell as all cases
' for those projects used thesame type of tag.

Dim rsTagTypes As Recordset, strTagTypes As String, strSep As String, n As Integer

Set rsTagTypes = CurrentDb.OpenRecordset("select * from _TagTypes where not isnull(TagFormat) order by DisplaySeq", dbOpenDynaset)

strTagTypes = "": strSep = ""

With rsTagTypes
    If Not (.EOF And .BOF) Then
        .MoveFirst
        Do While Not .EOF
            If Not IsNull(ReformatField(TagValue, !TagFormat, True)) Then
                strTagTypes = strTagTypes & strSep & !TagType
                strSep = ", "
            End If
            .MoveNext
        Loop
    End If
End With

If strTagTypes = "" Then
    getTagType = "Other"
    Exit Function
End If

' Change last separator comma to an " or "
If InStr(strTagTypes, ",") > 0 Then
    n = Len(strTagTypes)
    Do While Mid$(strTagTypes, n, 1) <> ",": n = n - 1: Loop
    strTagTypes = Left$(strTagTypes, n - 1) & " or" & Mid$(strTagTypes, n + 1)
End If

getTagType = strTagTypes

Exit Function


ErrorHandler:
    msgSystemError Err, Error$, "getTagType"

End Function



Public Function GetTagFormat(varTagType) As String
GetTagFormat = DLookup("tagFormat", "_TagTypes", "TagType = '" & varTagType & "'")
End Function

Public Function IsReallyNumeric(varData) As Boolean
If Not IsNumeric(varData) Then
    IsReallyNumeric = False: Exit Function
End If

If InStr(varData, "&") > 0 Then
    IsReallyNumeric = False: Exit Function
End If
        
IsReallyNumeric = True

End Function
