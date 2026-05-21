MODULE NAME: basPopups
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

'Implements pop up warning message when first starting PIMS.
'Users need to ammend data as instructed.

Sub DoPopup()
    Dim msgstr As String
    Dim twpWidthLrg As Long
    Dim twpHgtLrg As Long
    
    DoCmd.Hourglass True
    msgstr = ""
    msgstr = msgstr & UnderReviewPopup
    msgstr = msgstr & NoFileRefPopup
    msgstr = msgstr & AnnualAndFinalReportsPopup
    msgstr = msgstr & AnnualAndFinalReportsRepAgreedPopup
    msgstr = msgstr & CommercialPopup
    
    
    If msgstr <> "" Then
        msgstr = "Messages for " & DLookup("UserName", "tblAccessUsers", "NTLogin='" & fnGetUserName() & "'") & vbCr & vbLf & vbCr & vbLf & msgstr
        
        DoCmd.OpenForm "frmPopUpMessage"
        Forms![frmPopUpMessage]![txtPopupMessage] = msgstr
        Dim n As Integer
        
        n = Len(msgstr)
        n = (0.7 * n / 100) + 3
        twpHgtLrg = 576 * n
        
        Forms![frmPopUpMessage].InsideHeight = twpHgtLrg
        Forms![frmPopUpMessage].Detail.Height = twpHgtLrg
        Forms![frmPopUpMessage].txtPopupMessage.Height = twpHgtLrg - 1000
        Forms![frmPopUpMessage].btnOK.SetFocus
    End If
    DoCmd.Hourglass False
End Sub



Function UnderReviewPopup()
    Dim db As Database
    Dim rs As Recordset
    Dim sqlstr As String
    Dim op As String
    
    Set db = CurrentDb
    sqlstr = "SELECT tblMilestone.Project+' '+tblMilestone.Number AS PM, tblAccessPrograms.SystemID " _
                & " FROM ((tblRadTrackProg INNER JOIN vProjectLatestDetails ON tblRadTrackProg.Program = vProjectLatestDetails.Program) INNER JOIN tblMilestone ON vProjectLatestDetails.ParentProject = tblMilestone.Project) INNER JOIN tblAccessPrograms ON tblRadTrackProg.Program = tblAccessPrograms.Program " _
                & " WHERE (((tblAccessPrograms.SystemID)=1) AND ((tblMilestone.UnderSDReview)=True) AND ((tblAccessPrograms.NTLogin)='" & fnGetUserName() & "') AND ((tblMilestone.DateDue)>=#4/1/2009#))"

    Set rs = db.OpenRecordset(sqlstr)
    
     op = ""
    With rs
        If Not (.BOF And .EOF) Then
        .MoveFirst
        End If
        While Not .EOF
            op = op & .Fields(0) & ", "
            .MoveNext
        Wend
        
    End With
    
    If op <> "" Then
        op = Left(op, Len(op) - 2) & ". "
        UnderReviewPopup = "These project milestones/deliverables shown as under review, please determine current status and update PIMS as required:  " & op & vbCr & vbLf & vbCr & vbLf
    Else
        UnderReviewPopup = ""
    End If
    
End Function


Function NoFileRefPopup()
    Dim db As Database
    Dim rs As Recordset
    Dim sqlstr As String
    Dim op As String
    
    Set db = CurrentDb
    
    sqlstr = "SELECT G_tlkpProject_RadTrackData.ParentProject " _
                & " FROM (tblRadTrackProg INNER JOIN (G_tlkpProject_RadTrackData INNER JOIN vProjectLatestDetails ON G_tlkpProject_RadTrackData.ParentProject = vProjectLatestDetails.ParentProject) ON tblRadTrackProg.Program = vProjectLatestDetails.Program) INNER JOIN tblAccessPrograms ON tblRadTrackProg.Program = tblAccessPrograms.Program " _
                & " WHERE (((tblAccessPrograms.NTLogin)='" & fnGetUserName() & " ') AND ((G_tlkpProject_RadTrackData.FileRef) Is Null Or (G_tlkpProject_RadTrackData.FileRef)='') AND ((vProjectLatestDetails.LastYear)>=2009) AND ((tblAccessPrograms.SystemID)=1));"

    Set rs = db.OpenRecordset(sqlstr)
    
     op = ""
    With rs
        If Not (.BOF And .EOF) Then
        .MoveFirst
        End If
        While Not .EOF
            op = op & .Fields(0) & ", "
            .MoveNext
        Wend
        
    End With
    
    If op <> "" Then
        op = Left(op, Len(op) - 2) & ". "
        NoFileRefPopup = "These projects do not have a CAC file reference on PIMS, please update as soon as possible:  " & op & vbCr & vbLf & vbCr & vbLf
    Else
        NoFileRefPopup = ""
    End If
    
End Function



Function AnnualAndFinalReportsPopup()
    Dim db As Database
    Dim rs As Recordset
    Dim sqlstr As String
    Dim op As String
    
    Set db = CurrentDb
    

    sqlstr = "SELECT MY_Radtrack_Reports.Project & ' ' & MY_Radtrack_Reports.Type AS Rep, tblAccessPrograms.SystemID " _
                & " FROM ((vProjectLatestDetails INNER JOIN tblRadTrackProg ON vProjectLatestDetails.Program = tblRadTrackProg.Program) INNER JOIN MY_Radtrack_Reports ON vProjectLatestDetails.ParentProject = MY_Radtrack_Reports.Project) INNER JOIN tblAccessPrograms ON tblRadTrackProg.Program = tblAccessPrograms.Program " _
                & " WHERE (((tblAccessPrograms.SystemID)=1) AND ((tblAccessPrograms.NTLogin)='" & fnGetUserName() & "') AND ((MY_Radtrack_Reports.EmailedToCustomer)<DateAdd('ww',-2,Now())) AND ((MY_Radtrack_Reports.SignedCopyToCustomer) Is Null) AND ((MY_Radtrack_Reports.Year)>=2009)); "

    Set rs = db.OpenRecordset(sqlstr)
    
     op = ""
    With rs
        If Not (.BOF And .EOF) Then
        .MoveFirst
        End If
        While Not .EOF
            op = op & .Fields(0) & ", "
            .MoveNext
        Wend
        
    End With
    
    If op <> "" Then
        op = Left(op, Len(op) - 2) & ". "
        AnnualAndFinalReportsPopup = "The signed copy of the annual/final report is now due for these projects, please send a copy to the customer and update PIMS accordingly:  " & op & vbCr & vbLf & vbCr & vbLf
    Else
        AnnualAndFinalReportsPopup = ""
    End If
    
End Function


Function AnnualAndFinalReportsRepAgreedPopup()
    Dim db As Database
    Dim rs As Recordset
    Dim sqlstr As String
    Dim op As String
    
    Set db = CurrentDb
    sqlstr = "SELECT MY_Radtrack_Reports.Project & ' ' & MY_Radtrack_Reports.Type AS Rep " _
                & " FROM ((vProjectLatestDetails INNER JOIN tblRadTrackProg ON vProjectLatestDetails.Program = tblRadTrackProg.Program) INNER JOIN MY_Radtrack_Reports ON vProjectLatestDetails.ParentProject = MY_Radtrack_Reports.Project) INNER JOIN tblAccessPrograms ON tblRadTrackProg.Program = tblAccessPrograms.Program " _
                & " WHERE (((tblAccessPrograms.NTLogin)='" & fnGetUserName() & "') AND ((MY_Radtrack_Reports.EmailedToCustomer)<DateAdd('m',4,Now())) AND ((MY_Radtrack_Reports.ReportAgreedDate) Is Null) AND ((MY_Radtrack_Reports.Year)>=2009) AND ((tblAccessPrograms.SystemID)=1)); "

    Set rs = db.OpenRecordset(sqlstr)
    
    op = ""
     
    With rs
        If Not (.BOF And .EOF) Then
        .MoveFirst
        End If
        While Not .EOF
            op = op & .Fields(0) & ", "
            .MoveNext
        Wend
        
    End With
    
    If op <> "" Then
        op = Left(op, Len(op) - 2) & ". "
        AnnualAndFinalReportsRepAgreedPopup = "The date that the annual/final reports for these projects were agreed by the customer is blank, please confirm whether agreed or not and update PIMS if appropriate:  " & op & vbCr & vbLf & vbCr & vbLf
    Else
        AnnualAndFinalReportsRepAgreedPopup = ""
    End If
    
End Function


Function CommercialPopup()
    Dim db As Database
    Dim rs As Recordset
    Dim sqlstr As String
    Dim op As String
    
    Set db = CurrentDb
    sqlstr = "SELECT G_tlkpProject_RadTrackData.ParentProject " _
                & " FROM (vProjectLatestDetails INNER JOIN G_tlkpProject_RadTrackData ON vProjectLatestDetails.ParentProject = G_tlkpProject_RadTrackData.ParentProject) INNER JOIN tblAccessPrograms ON vProjectLatestDetails.Program = tblAccessPrograms.Program " _
                & " WHERE (((vProjectLatestDetails.LastYear)>=2009) AND (iif(isnull( [revisedEndDate]),[EndDate],[revisedEndDate])<DateAdd('m',1,Now())) AND ((G_tlkpProject_RadTrackData.ClosedDate) Is Null) AND ((vProjectLatestDetails.Program)='Commercial') AND ((tblAccessPrograms.SystemID)=1) AND ((tblAccessPrograms.NTLogin)='" & fnGetUserName() & "'));"

    Set rs = db.OpenRecordset(sqlstr)
    
    op = ""
     
    With rs
        If Not (.BOF And .EOF) Then
        .MoveFirst
        End If
        While Not .EOF
            op = op & .Fields(0) & ", "
            .MoveNext
        Wend
        
    End With
    
    If op <> "" Then
        op = Left(op, Len(op) - 2) & ". "
        CommercialPopup = "Please check whether these projects have been invoiced and, if appropriate, add a date to the 'Closed Date' field:  " & op & vbCr & vbLf & vbCr & vbLf
    Else
        CommercialPopup = ""
    End If
    
End Function
