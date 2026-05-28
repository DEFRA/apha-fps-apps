Option Compare Database
Option Explicit


Function SendMessage(Recipient As Variant, Subject As String, MessageText As String, Optional Attachment As Variant, Optional CC As Variant, Optional BCC As Variant, Optional SENDER = Null)
' Sends an email message without user-intervention

On Error GoTo SendMessage_err

Dim email As New Emailoutlook

SendMessage = True

With email
    .DisplayBeforeSending = False
    .Recipient = Recipient & ""
    .Subject = Subject
    .SENDER = SENDER
    .MessageText = MessageText
    If Not IsMissing(Attachment) Then
        .Attachment = Attachment
    End If
    If Not IsMissing(CC) Then
        .CC = CC
    End If
    If Not IsMissing(BCC) Then
        .BCC = BCC
    End If
    .SendEmailMessage
End With

sendMessage_ok:
    Exit Function

SendMessage_err:
    MsgBox "Error " & Format(Err) & ": " & Error$, vbCritical, "SendMessage Failed"
    SendMessage = False

    Exit Function
    
End Function


Sub CreateMessage(Recipient As Variant, Subject As String, MessageText As String, Optional Attachment As Variant, Optional CC As Variant, Optional BCC As Variant, Optional SENDER As Variant = Null)
' Creates an email message, displays it on screen ready to send

On Error GoTo CreateMessage_err

Dim email As New Emailoutlook

With email
    .DisplayBeforeSending = True
    .Recipient = Recipient & ""
    '.SENDER = SENDER
    .Subject = Subject
    .MessageText = MessageText
    If Not IsMissing(Attachment) Then
        .Attachment = Attachment
    End If
    If Not IsMissing(CC) Then
        .CC = CC
    End If
    If Not IsMissing(BCC) Then
        .BCC = BCC
    End If
    
    .SendEmailMessage
End With

CreateMessage_ok:
    Exit Sub

CreateMessage_err:
    MsgBox "Error " & Format(Err) & ": " & Error$, vbCritical, "CreateMessage Failed"
    Exit Sub
    
End Sub