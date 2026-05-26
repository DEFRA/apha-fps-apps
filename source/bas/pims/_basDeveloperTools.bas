MODULE NAME: _basDeveloperTools
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

' Author:   George Seears
' Date:     September 2001
'
' Tools to make life just that little bit easier for over-worked and under-paid developers.

Type globalVars
    sysPath                 As String
    DBname                  As String
    x                       As Variant
    y                       As Variant
    h                       As Variant
    w                       As Variant
    GlobalEditTableName     As Variant
End Type

Global g As globalVars



Public Function SetStatusBarTextAllControlsAllForms(Optional strNewText As String = " ", Optional blnOverWriteExistingText As Boolean = False)

' Code to set the StatusBarText of all controls in all forms
' Usually used to set them to " " to avoid the undesribal "Form View" in the status bar for controls
' that don't have the status bar text set to anything

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String
    Dim frm As Form
    Dim ctrl As Control
    Dim blnDocAlreadyLoaded As Boolean
    Set db = CurrentDb
    For j = 0 To db.Containers.Count - 1
        Set MyContainer = db.Containers(j)
        If MyContainer.Name = "Forms" Then
            SysCmd acSysCmdInitMeter, "Processing...", MyContainer.Documents.Count - 1
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                If isFormLoaded(fm) Then
                    blnDocAlreadyLoaded = True
                Else
                    DoCmd.OpenForm fm, A_DESIGN, , , , A_HIDDEN 'A_NORMAL
                    blnDocAlreadyLoaded = False
                End If
                Set frm = Forms(fm)
                On Error Resume Next
                For Each ctrl In frm.Controls
                    If Not (ctrl.StatusBarText <> "" And Not blnOverWriteExistingText) Then
                        ctrl.StatusBarText = strNewText
                    End If
                Next
                If Not blnDocAlreadyLoaded Then DoCmd.Close A_FORM, fm, acSaveYes
                SysCmd acSysCmdUpdateMeter, i
            Next i
            SysCmd acSysCmdRemoveMeter
        End If
    Next j
    SysCmd acSysCmdRemoveMeter
End Function


Public Function RemoveOrphanCodeAllForms(Optional frmName = "")

' event subs in forms for which there is no matching event are removed from the forms
' e.g. Private Sub Button3_click in a form that has no Button3.

    Dim db As Database, MyContainer As Container, MyDocument As Document, mdl As Module, strEventControl As String
    Dim i As Integer, j As Integer
    Dim fm As String
    Dim frm As Form
    Dim ctrl As Control
    Dim blnDocAlreadyLoaded As Boolean
    Dim startline As Long, startcolumn As Long, endline As Long, endcolumn As Long, mdlLength As Long, procLength As Long
    Dim pProcKind As Long, procName As String
    Dim strCtrlName As String, strEventName As String, p1 As Integer, p2 As Integer
    Dim strEventPropName As String, strEventPropValue As String
    Dim KillMessage As String, KillResult As Long, blnFound As Boolean
    Set db = CurrentDb
    For j = 0 To db.Containers.Count - 1
        Set MyContainer = db.Containers(j)
        If MyContainer.Name = "Forms" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                If frmName = "" Or fm = frmName Then
                    Debug.Print "-----------------"
                    Debug.Print "Form: " & fm
                    Debug.Print "-----------------"
                    If isFormLoaded(fm) Then
                        blnDocAlreadyLoaded = True
                    Else
                        DoCmd.OpenForm fm, A_DESIGN, , , , A_HIDDEN 'A_NORMAL
                        blnDocAlreadyLoaded = False
                    End If
                    Set frm = Forms(fm)
                    ' process this form
                    If frm.HasModule Then
                        Set mdl = frm.Module
                        With mdl
                            mdlLength = .CountOfLines
                            startline = .CountOfDeclarationLines + 1
                            'Debug.Print "Module Length = " & CStr(mdlLength)
                            Do While startline < mdlLength
                                ' process a proc
                                procName = .ProcOfLine(startline, pProcKind)
                                startline = .ProcBodyLine(procName, pProcKind)
                                procLength = .ProcCountLines(procName, pProcKind)
                                If pProcKind = vbext_pk_Proc Then
                                    ' it's a sub or function
                                    ' Extract the control name and event type
                                    If InStr(procName, "_") > 0 Then
                                        p1 = Len(procName)
                                        Do While Mid$(procName, p1, 1) <> "_": p1 = p1 - 1: Loop
                                        strEventName = Mid$(procName, p1 + 1)
                                        strCtrlName = Left$(procName, p1 - 1)
                                        KillMessage = ""
                                        If strCtrlName <> "Form" And InStr(",Click,DblClick,AfterUpdate,BeforeUpdate,", "," & strEventName & ",") > 0 Then
                                            blnFound = False
                                            For Each ctrl In frm.Controls
                                                If ctrl.EventProcPrefix = strCtrlName Then
                                                    blnFound = True
                                                    Exit For
                                                End If
                                            Next
                                            If blnFound Then
                                                strEventPropName = "On" & strEventName
                                                On Error Resume Next
                                                strEventPropValue = ctrl.Properties(strEventPropName)
                                                If Err = 0 Then
                                                    If strEventPropValue <> "[Event Procedure]" Then
                                                        KillMessage = procName & ": " & strCtrlName & "." & strEventPropName & " does not call event proc!"
                                                    End If
                                                End If
                                                On Error GoTo 0
                                            Else
                                                KillMessage = procName & " EVENT HAS NO CONTROL!!!"
                                            End If
                                        End If
                                        If KillMessage <> "" Then
                                            KillResult = MsgBox(KillMessage & vbNewLine & "Delete this proc?", vbExclamation + vbYesNoCancel)
                                            Select Case KillResult
                                                Case vbYes:
                                                    Debug.Print "Deleting " & procName, startline, procLength
                                                    mdl.DeleteLines startline, procLength - 1
                                                    procLength = 0 ' to stop incrementing startLine, below
                                                    mdlLength = .CountOfLines

                                                Case vbNo:
                                                Case Else:
                                                    If Not blnDocAlreadyLoaded Then DoCmd.Close A_FORM, fm, acSaveNo
                                                    Exit Function
                                            End Select
                                        End If
                                    End If
                                End If
                                ' next proc
                                'Debug.Print startline, procLength
                                startline = startline + procLength
                            Loop
                        End With
                    
                    End If
                    'Debug.Print "Saving form"
                    If Not blnDocAlreadyLoaded Then DoCmd.Close A_FORM, fm, acSaveYes
                    'Debug.Print "Saved"
                End If
            Next i
        End If
    Next j
End Function


Public Function RemoveUnwantedFields(strFormname As String, strSubSystemName As String)

' Code to remove any fields from the specified form that are not defined in sysTables for the specified data set

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String
    Dim frm As Form
    Dim ctrl As Control
    Dim blnDocAlreadyLoaded As Boolean
    Dim strCtrlName As String
    
    Set db = CurrentDb
    Set MyContainer = db.Containers!Forms
    Set MyDocument = MyContainer.Documents(strFormname)
    fm = MyDocument.Name
    If isFormLoaded(fm) Then
        blnDocAlreadyLoaded = True
    Else
        DoCmd.OpenForm fm, A_DESIGN, , , , A_HIDDEN 'A_NORMAL
        blnDocAlreadyLoaded = False
    End If
    Set frm = Forms(fm)
    On Error Resume Next
    For Each ctrl In frm.Controls
        If TypeOf ctrl Is TextBox Then
            If IsNull(DLookup("FieldName", "_qryFieldsBySubsystem", "SubSystem = " & SQLstring(strSubSystemName) & " and FieldName = " & SQLstring(ctrl.Name))) Then
                DeleteControl fm, ctrl.Name
            End If
        End If
    Next
    If Not blnDocAlreadyLoaded Then DoCmd.Close A_FORM, fm, acSaveYes
    
End Function
Public Sub UpgradeComboSourceAllForms()

' Looks for combos with rowSource like 'select LookupID, LookupValue from qryLookups where ListId = 88'
' and replaces with 'qryLookups_88'
' Also generates the passthru-query that it refers to if it doesn't exist already

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String
    Dim frm As Form
    Dim ctrl As Control
    Dim blnDocAlreadyLoaded As Boolean
    Dim strListID As String
    Set db = CurrentDb
    For j = 0 To db.Containers.Count - 1
        Set MyContainer = db.Containers(j)
        If MyContainer.Name = "Forms" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                SysCmd acSysCmdSetStatus, "Processing " & fm
                If isFormLoaded(fm) Then
                    blnDocAlreadyLoaded = True
                Else
                    DoCmd.OpenForm fm, A_DESIGN, , , , A_HIDDEN 'A_NORMAL
                    blnDocAlreadyLoaded = False
                End If
                Set frm = Forms(fm)
                On Error Resume Next
                For Each ctrl In frm.Controls
                    If TypeOf ctrl Is ComboBox Then
                        If ctrl.RowSource Like "select LookupID, LookupValue from qryLookups where ListId*" Then
                            strListID = getLookupListID(ctrl.RowSource)
                            checkQryLookup strListID '  generate query if it doesn't exist
                            ctrl.RowSource = "qryLookups_" & strListID
                        End If
                    End If
                Next
                If Not blnDocAlreadyLoaded Then DoCmd.Close A_FORM, fm, acSaveYes
            Next i
        End If
    Next j
    SysCmd acSysCmdClearStatus
End Sub



Public Function getLookupListID(strRowSource As String) As String
Dim i As Integer, strListID As String

For i = Len(strRowSource) To 1 Step -1
    If IsNumeric(Mid$(strRowSource, i, 1)) Then Exit For
Next

strListID = ""

For i = i To 1 Step -1
    If Not IsNumeric(Mid$(strRowSource, i, 1)) Then
        Exit For
    Else
        strListID = Mid$(strRowSource, i, 1) & strListID
    End If
Next

getLookupListID = strListID
        
End Function

Public Function checkQryLookup(strListID As String) As Boolean
' Looks to see if there is query called "qryLookup_" & strListID.
' If there isn't creates one.
' It will be a pass-thru based on

Dim db As Database, qdefs As QueryDefs, qdef As QueryDef, qdef00 As QueryDef
Dim strQdefName As String, strSQL As String
Dim lngErr As Long, rs As Recordset

Set db = CurrentDb
Set qdefs = db.QueryDefs

strQdefName = "qryLookups_" & strListID

On Error Resume Next
Set qdef = qdefs(strQdefName)
lngErr = Err

On Error GoTo 0

If lngErr <> 0 Then ' it's missing
    DoCmd.CopyObject , strQdefName, acQuery, "qryLookups_00"
    qdefs.Refresh
    Set qdef = qdefs(strQdefName)
    qdef.sql = replaceChars(qdef.sql, "00", strListID)
    
End If

End Function
Public Function getControlSizePos()
On Error Resume Next
Dim ctrl As Control
Set ctrl = SelectedControl(Screen.ActiveForm)
Set ctrl = SelectedControl(Screen.ActiveReport)
g.x = ctrl.Left
g.y = ctrl.Top
g.h = ctrl.Height
g.w = ctrl.Width

End Function

Public Function controlSetAll()
ControlSetPosSize g.x, g.y, g.h, g.w
End Function

Public Function controlSetLeft()
ControlSetPosSize g.x, Null, Null, Null
End Function

Public Function controlSetTop()
ControlSetPosSize Null, g.y, Null, Null
End Function

Public Function controlSetHeight()
ControlSetPosSize Null, Null, g.h, Null
End Function

Public Function controlSetWidth()
ControlSetPosSize Null, Null, Null, g.w
End Function


Function SelectedControl(frm As Object) As Control
' returns handle to first selected control on a form/report in design view
    Dim intI As Integer, ctl As Control
    On Error GoTo continue
    If frm.CurrentView <> 0 Then
        MsgBox "Form must be in design view"
        ' Form is not in Design view.
        Exit Function
    End If
    
continue:
    For intI = 0 To frm.Count - 1
        Set ctl = frm(intI)
        If ctl.InSelection = True Then
            Set SelectedControl = ctl
            Exit Function
        End If
    Next intI
End Function

Public Function ControlSetPosSize(x, y, h, w)
' set the LEFT property of all selected controls in active form/REPORT

If IsNull(x) And IsNull(y) And IsNull(h) And IsNull(w) Then
    MsgBox "You must Get Size/Pos first"
    Exit Function
End If

Dim frm As Object
Dim intI As Integer, ctl As Control
    
On Error Resume Next
Set frm = Screen.ActiveForm
Set frm = Screen.ActiveReport

On Error GoTo continue
    If frm.CurrentView <> 0 Then
        MsgBox "Form/Report must be in design view"
        ' Form is not in Design view.
        Exit Function
    End If
    
continue:
    For intI = 0 To frm.Count - 1
        Set ctl = frm(intI)
        If ctl.InSelection = True Then
            If Not IsNull(x) Then ctl.Left = x
            If Not IsNull(y) Then ctl.Top = y
            If Not IsNull(h) Then ctl.Height = h
            If Not IsNull(w) Then ctl.Width = w
        End If
    Next intI

End Function


Public Sub SetFormPopUpMenu(Optional strMenuBarName As String = "popStandard")

' Code to set the menu property of all reports

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer, c As Integer
    Dim doc As String
    Dim obj As Object
    Dim ctrl As Control
    Set db = CurrentDb
    Set MyContainer = db.Containers("Forms")
    For i = 0 To MyContainer.Documents.Count - 1
        Set MyDocument = MyContainer.Documents(i)
        doc = MyDocument.Name
        DoCmd.OpenForm doc, A_DESIGN, , , , acHidden
        Set obj = Forms(doc)
        obj.ShortcutMenuBar = strMenuBarName
        obj.ShortcutMenu = True
        DoCmd.Close acForm, doc, acSaveYes
                        
    Next i
End Sub

Public Sub SetFormMenuBar(Optional strMenuBarName As String = "Form_General", Optional blnForceOverwrite As Boolean = False)

' Code to set the menu property of all reports

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer, c As Integer
    Dim doc As String
    Dim obj As Object
    Dim ctrl As Control
    Set db = CurrentDb
    Set MyContainer = db.Containers("Forms")
    SysCmd acSysCmdInitMeter, "Processing...", MyContainer.Documents.Count - 1
    For i = 0 To MyContainer.Documents.Count - 1
        Set MyDocument = MyContainer.Documents(i)
        doc = MyDocument.Name
        DoCmd.OpenForm doc, A_DESIGN, , , , acHidden
        Set obj = Forms(doc)
        If blnForceOverwrite Or isBlank(obj.MenuBar) Then
            obj.MenuBar = strMenuBarName
        End If
        DoCmd.Close acForm, doc, acSaveYes
        SysCmd acSysCmdUpdateMeter, i
    Next i
    SysCmd acSysCmdRemoveMeter
End Sub

Public Sub SetReportMenuBar(Optional strMenuBarName As String = "Report_General")

' Code to set the menu property of all reports

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim doc As String
    Dim obj As Object
    Dim ctrl As Control
    Set db = CurrentDb
    Set MyContainer = db.Containers("Reports")
    For i = 0 To MyContainer.Documents.Count - 1
        Set MyDocument = MyContainer.Documents(i)
        doc = MyDocument.Name
        DoCmd.OpenReport doc, A_DESIGN
        Set obj = Reports(doc)
        obj.MenuBar = strMenuBarName
        DoCmd.Close acReport, doc, acSaveYes
                        
    Next i
End Sub

Public Function ControlSetLookup()
' set the ROWSOURCE property of all selected controls in active form
' Rowsource will be like "select LookupValue from qryLookups where ListId = 88" where
' the number is the ListID from tblLookupLists

Dim frm As Form
Dim intI As Integer, ctl As Control
Dim strRowSource As String, strRowSourceType As String, varListID As Variant
    
Set frm = Screen.ActiveForm

    If frm.CurrentView <> 0 Then
        MsgBox "Form must be in design view"
        Exit Function
    Else
        For intI = 0 To frm.Count - 1
            Set ctl = frm(intI)
            If ctl.InSelection = True Then
                On Error Resume Next
                strRowSourceType = ctl.RowSourceType
                If Err = 0 Then
                    If strRowSourceType = "Table/Query" Then
                        strRowSource = ctl.RowSource
                        varListID = DLookup("ListId", "tblLookupLists", "[oldTableName] = " & SQLstring([strRowSource]))
                        If Not IsNull(varListID) Then
                            ctl.RowSource = "select LookupID, LookupValue from qryLookups where ListId = " & CStr(varListID)
                            ctl.ColumnCount = 2
                            ctl.ColumnWidths = "0cm"

                        End If
                    
                    End If
                End If
            End If
        Next intI
    End If

End Function

Public Function ControlSetObjName()
' set the NAME property of all selected controls to that of the ControlSource

Dim frm As Form
Dim intI As Integer, ctl As Control
Dim strName As String, strControlSource As String
    
Set frm = Screen.ActiveForm

    If frm.CurrentView <> 0 Then
        MsgBox "Form must be in design view"
        Exit Function
    Else
        On Error GoTo nextCtl
        For intI = 0 To frm.Count - 1
            Set ctl = frm(intI)
            If ctl.InSelection = True Then
                If Not IsNull(ctl.ControlSource) And ctl.Name <> ctl.ControlSource Then
                    ctl.Name = ctl.ControlSource
                End If
            End If
nextCtl:
        Next intI
    End If

End Function

Public Function ControlSetNotInList()
' set the OnNotInList property of all selected controls
' Only controls that have a OnNotInList property will be processed.
' Only controls whose LimitToList property is true will be processed.

' Sets OnNotInList = "[Event Procedure]"
' Writes a NotInList event
'   Private Sub ctrlname_NotInList(NewData As String, Response As Integer)
'   Response = addItem_ToTable(NewData, "tlkpBlink")
'   End Sub

Dim frm As Form
Dim intI As Integer, ctl As Control, mdl As Module, lngLineNo As Long
Dim strName As String, strControlSource As String
    
Set frm = Screen.ActiveForm

    If frm.CurrentView <> 0 Then
        MsgBox "Form must be in design view"
        Exit Function
    Else
        On Error GoTo nextCtl
        For intI = 0 To frm.Count - 1
            Set ctl = frm(intI)
            If ctl.InSelection Then
                If ctl.OnNotInList = "" And ctl.LimitToList = True Then
                    Debug.Print ctl.Name
                    ctl.OnNotInList = "[Event Procedure]"
                    Set mdl = frm.Module
                    lngLineNo = mdl.CreateEventProc("NotInList", ctl.Name)
                    mdl.InsertLines lngLineNo + 1, vbTab & "Response = addToPickList(NewData)"
                End If
            End If
nextCtl:
        Next intI
    End If

End Function


Function RefreshAttachment(Optional tableName) As Integer

On Error GoTo errHandler

Dim db As Database, strTableName As String, tbl As TableDef

Set db = DBEngine(0)(0)

If IsMissing(tableName) Then
    ' currently selected in db window
    If Application.CurrentObjectType <> acTable Then
        MsgBox "Select a table in database window first"
        Exit Function
    End If
    strTableName = Application.CurrentObjectName
Else
    strTableName = tableName
End If
Set tbl = db.TableDefs(strTableName)
tbl.RefreshLink

RefreshAttachment = True

    Exit Function


errHandler:
    RefreshAttachment = False
    msgSystemError Err, Error$, "RefreshAttachment"


End Function

Function RefreshCurrentAttachment() As Integer

If RefreshAttachment() Then
    MsgBox "Relink OK"
End If
End Function


Public Sub CreateGeoToolsBar()

' Creates the geoTools commandBar if it doesn't exist

Dim cmb As Variant, cbars As Variant
Dim CBARNAME As String

CBARNAME = InputBox("Specify name for geoTools toolbar", "Create geoTools Toolbar", "geoTools")
If CBARNAME = "" Then Exit Sub

Set cbars = Application.CommandBars
On Error Resume Next
Set cmb = cbars(CBARNAME)
If Err = 0 Then
    MsgBox CBARNAME & " command bar already exists"
    Exit Sub
End If
On Error GoTo 0

Set cmb = cbars.Add(CBARNAME)
cmb.Position = 1

CreateToolbarButton cmb, "Get Size/Pos", "=getControlSizePos()", "Store size and position data of first selected object"
CreateToolbarButton cmb, "Set Left", "=ControlSetLeft()", "Set LEFT property of all selected objects"
CreateToolbarButton cmb, "Set Top", "=ControlSetTop()", "Set TOP property of all selected objects"
CreateToolbarButton cmb, "Set Width", "=ControlSetWidth()", "Set WIDTH property of all selected objects"
CreateToolbarButton cmb, "Set Height", "=ControlSetHeight()", "Set HEIGHT property of all selected objects"
CreateToolbarButton cmb, "Set All", "=ControlSetAll()", "Set size and position properties of all selected objects"
CreateToolbarButton cmb, "Set Names", "=ControlSetObjName()", "Set object names of all selected object to the object's ControlSource."
CreateToolbarButton cmb, "Set NotInList", "=ControlSetNotInList()", "Only for list objects with LimitToList true. Sets OnNotInList and creates required event proc. "
CreateToolbarButton cmb, "ReLink", "=RefreshCurrentAttachment()", "Refresh link for any table currently selected in databas window"
CreateToolbarButton cmb, "Set Date Rules", "=ControlSetDateValidation()", "Set Validation rule and text, delete input mask"
CreateToolbarButton cmb, "Set Autodrop", "=ControlSetAutoComboDrop()", "Set Autodrop for a combo"
'CreateToolbarButton cmb, "Set Lookups", "=ControlSetLookup()", "For converting specific table lookups to use multiple-lookup table. See ControlSetLookup() code in basGeoTools"
'CreateToolbarButton cmb, "ErrHandler", "=CreateOnErrorCode()", "DO NOT USE - insert Error Handler in current module."
CreateToolbarButton cmb, "Global Edit", "=GlobalEditor()", "Find and Replace of data in all text columns in a table. Substrings ar eprocessed, not whole values"

cmb.Visible = True


End Sub


Public Function CreateToolbarButton(cmb As Variant, strName As String, strAction As String, Optional varToolTip As Variant)

Dim cbc As Variant
Set cbc = cmb.Controls.Add(1)
cbc.caption = strName
cbc.Style = 2
cbc.OnAction = strAction
If Not IsMissing(varToolTip) Then
    cbc.TooltipText = varToolTip
End If

End Function





Public Sub FormPropertiesCopy(strFormNameFrom As String, strFormNameTo As String)
' Copies all properties from one form to another

Dim ff As Form, tf As Form, p As Property
Set ff = Forms(strFormNameFrom)
Set tf = Forms(strFormNameTo)

On Error Resume Next

For Each p In ff.Properties
    tf.Properties(p.Name) = p.Value
    Debug.Print p.Name, "=", p.Value
Next


End Sub

Public Function ControlSetDateValidation()

Dim frm As Form
Dim intI As Integer, ctl As Control
    
Set frm = Screen.ActiveForm

    If frm.CurrentView <> 0 Then
        MsgBox "Form must be in design view"
        ' Form is not in Design view.
        Exit Function
    Else
        For intI = 0 To frm.Count - 1
            Set ctl = frm(intI)
            If ctl.InSelection = True Then
                ctl.InputMask = ""
                ctl.ValidationRule = "isValidDate([Screen].[ActiveControl])=True"
                ctl.ValidationText = "Please enter a valid date between 1 Jan 1900 and 31 Dec 2999"
            End If
        Next intI
    End If


End Function


Public Function ControlSetAutoComboDrop()

Dim frm As Form
Dim intI As Integer, ctl As Control, mdl As Module, lngLine As Long
    
Set frm = Screen.ActiveForm

    If frm.CurrentView <> 0 Then
        MsgBox "Form must be in design view"
        ' Form is not in Design view.
        Exit Function
    Else
        Set mdl = frm.Module
        For intI = 0 To frm.Count - 1
            Set ctl = frm(intI)
            If ctl.InSelection = True Then
                If TypeOf ctl Is ComboBox Then
                    If ctl.OnKeyPress = "" Then
                        ctl.OnKeyPress = "[Event Procedure]"
                        lngLine = mdl.CreateEventProc("KeyPress", ctl.Name)
                        frm.Module.InsertLines lngLine + 1, "AutoDropCombo KeyAscii"
                    End If
                End If
            End If
        Next intI
        frm.SetFocus
    End If


End Function




Public Sub EnumerateCommandBar()

Dim cmb As Variant, cbars As Variant, ctl As Variant
Dim CBARNAME As String

CBARNAME = InputBox("Specify name of commandbar to enumerate", "Enumerate CommandBar", "GeoTools")
If CBARNAME = "" Then Exit Sub

Set cbars = Application.CommandBars
'On Error Resume Next
Set cmb = cbars(CBARNAME)


Debug.Print
Debug.Print "-------------------------------------"
Debug.Print cmb.Name
Debug.Print "-------------------------------------"
For Each ctl In cmb.Controls
    Debug.Print Format(ctl.caption, String(20, "@") & "!") & Format(ctl.OnAction, String(20, "@") & "!")
Next ctl

RunCommand acCmdDebugWindow


End Sub




Public Sub UpgradeDateValidationAllForms()

' Looks for date validation rule like "isValidDate([Screen].[ActiveControl])=True"
' and replaces [Screen].[ActiveControl] with the name of the control

    Dim db As Database, MyContainer As Container, MyDocument As Document
    Dim i As Integer, j As Integer
    Dim fm As String
    Dim frm As Form
    Dim ctrl As Control
    Dim blnDocAlreadyLoaded As Boolean
    Dim strListID As String
    Set db = CurrentDb
    For j = 0 To db.Containers.Count - 1
        Set MyContainer = db.Containers(j)
        If MyContainer.Name = "Forms" Then
            For i = 0 To MyContainer.Documents.Count - 1
                Set MyDocument = MyContainer.Documents(i)
                fm = MyDocument.Name
                SysCmd acSysCmdSetStatus, "Processing " & fm
                If isFormLoaded(fm) Then
                    blnDocAlreadyLoaded = True
                Else
                    DoCmd.OpenForm fm, A_DESIGN, , , , A_HIDDEN 'A_NORMAL
                    blnDocAlreadyLoaded = False
                End If
                Set frm = Forms(fm)
                On Error Resume Next
                For Each ctrl In frm.Controls
                    If TypeOf ctrl Is TextBox Then
                        If ctrl.ValidationRule Like "isValidDate*" Then
                            ctrl.ValidationRule = "isValidDate([" & ctrl.Name & "])=True"
                        End If
                    End If
                Next
                If Not blnDocAlreadyLoaded Then DoCmd.Close A_FORM, fm, acSaveYes
            Next i
        End If
    Next j
    SysCmd acSysCmdClearStatus
End Sub

Public Sub TableNames_ReplacePrefix()

' Used to remove owner prefix after linking to ODBC tables

Dim tdef As TableDef, strName As String, intPrefixLength As Integer
Dim intTableCount As Integer, intTableNo As Integer
Dim varOldPrefix As Variant, varNewPrefix As Variant
Dim strNewName As String

On Error GoTo ErrorHandler

varOldPrefix = InputBox$("Old table name prefix", "Replace Tablename Prefixes", "dbo_")
If Nz(varOldPrefix, "") = "" Then Exit Sub

varNewPrefix = InputBox$("New table name prefix (leave blank just to remove old prefix)", "Replace Tablename Prefixes", "dbo_")


intPrefixLength = Len(varOldPrefix)
intTableCount = CurrentDb.TableDefs.Count
intTableNo = 0

SysCmd acSysCmdInitMeter, "Removing '" & varOldPrefix & "' prefixes...", intTableCount

For Each tdef In CurrentDb.TableDefs
    intTableNo = intTableNo + 1
    strName = tdef.Name
    If Left$(strName, intPrefixLength) = varOldPrefix Then
        strName = Mid$(tdef.Name, intPrefixLength + 1)
        If Nz(varNewPrefix, "") <> "" Then
            strName = varNewPrefix & strName
        End If
    End If
    If tdef.Name <> strName Then tdef.Name = strName
    SysCmd acSysCmdUpdateMeter, intTableNo
Next

SysCmd acSysCmdRemoveMeter
CurrentDb.TableDefs.Refresh

Exit Sub

ErrorHandler:
    Stop
    SysCmd acSysCmdRemoveMeter
    Resume

End Sub

Public Function GlobalEditor()
Dim strTableName As String, strOld As String, strNew As String
strTableName = InputBox$("Table Name", "Global Edit", g.GlobalEditTableName)
If strTableName = "" Then Exit Function

g.GlobalEditTableName = strTableName

strOld = InputBox$("Find string", "Global Edit")
If strOld = "" Then Exit Function

strNew = InputBox$("New string", "Global Edit", strOld)

GlobalEdit strTableName, strOld, strNew

End Function

Public Sub GlobalEdit(tableName, strOld As String, strNew As String)
' Find and Replace on all text fields in a given table

Dim db As Database, rs As Recordset, fld As field, str, newVal As String, varNewValue As Variant

Set db = CurrentDb
Set rs = db.OpenRecordset(tableName, , dbSeeChanges)
With rs
    If Not (.EOF And .BOF) Then
        .MoveFirst
        Do While Not .EOF
            .Edit
            For Each fld In .Fields
                If Not IsNull(fld) And fld.Type = dbText Then
                    varNewValue = replaceChars(fld, strOld, strNew)
                    If fld <> varNewValue Then
                        Debug.Print "Field " & fld.Name, SQLstring(fld, True) & " --> " & SQLstring(varNewValue, True)
                        fld = varNewValue
                    End If
                End If
            Next
            .Update
            .MoveNext
        Loop
    End If
End With


End Sub


Public Sub RefreshTableLinks()

    Dim i As Integer
    Dim db As Database
    Dim tbl As TableDef
    Dim qry As QueryDef
    
    Set db = CurrentDb
    
    For i = 0 To db.TableDefs.Count - 1
        Set tbl = db.TableDefs(i)
        If tbl.Attributes And DB_ATTACHEDODBC Then
            If tbl.Connect Like "*DATABASE=MAB_Archive*" Then
                tbl.Connect = "ODBC;DRIVER=SQL Server;SERVER=DEFACPVWTSQL003;DATABASE=MAB_Archive_CM;"
                tbl.RefreshLink
            End If
'            If tbl.Connect Like "*DATABASE=FPS2021*" Then
'                tbl.Connect = "ODBC;DRIVER=SQL Server;SERVER=DEFACPVWPSQL001;DATABASE=FPS2021;"
'                tbl.RefreshLink
'            End If
        End If
        DoEvents
        Debug.Print "Done " & tbl.Name
    Next i
        
    MsgBox "Done"
     
End Sub

Sub FindLinkedQueryDefs()

    Dim db As Database
    Dim qry As QueryDef
    Dim pty As Property
    Dim i As Integer
    Dim strNew As String
    
    Set db = CurrentDb()
    On Error Resume Next

    For i = 0 To db.QueryDefs.Count - 1
        Set qry = db.QueryDefs(i)
        For Each pty In qry.Properties
            If pty.Name = "Connect" Then
                If pty.Value Like "ODBC*" Then
                    Debug.Print qry.Name & ":   " & pty.Value
                End If
            End If
        Next pty
        DoEvents
    Next i
        
        MsgBox "Done"


End Sub

Sub RefreshQueryDefs()

    Dim db As Database
    Dim qry As QueryDef
    Dim pty As Property
    Dim i As Integer
    Dim strNew As String
    
    Set db = CurrentDb()
    On Error Resume Next

    For i = 0 To db.QueryDefs.Count - 1
        Set qry = db.QueryDefs(i)
        For Each pty In qry.Properties
            If pty.Name = "Connect" Then
                If pty.Value Like "ODBC*" Then
                    'pty.Value = Replace(pty.Value, "vla88.cvlnt.vla.gov.uk", "DEFACPVWPSQL001")
                    pty.Value = Replace(pty.Value, "MAB_Archive", "MAB_Archive_CM")
                    Debug.Print qry.Name & ":   " & pty.Value
                End If
            End If
        Next pty
        DoEvents
    Next i
        
    MsgBox "Done"
    
End Sub
