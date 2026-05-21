MODULE NAME: basEmailMAPI
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit
' Has to be run under Citrix to work as Citrix provides an Exchange profile.

Function SendMapiMail(ByRef Recipient As String, ByRef Subject As String, ByRef MessageText As String, Optional attachment As Variant) As Boolean
On Error GoTo SendMapiMail_Error

   SendMapiMail = False
   
   Dim MapiSession As Object
   Dim MapiMessage As Object
   Dim MapiRecipient As Object
   Dim MapiAttachment As Object
   Dim Recpt
   Dim errObj As Long
   Dim errMsg
   Dim recips() As Variant, recip As Variant, recipCount As Integer
   
   
   Const mapiTo = 1
   Const mapiHigh = 2
   Const mapiBCC = 3
   Const CdoFileData = 1
   
   Set MapiSession = CreateObject("Mapi.Session")
   
   MapiSession.Logon GetUserID()
   
   Set MapiMessage = MapiSession.Outbox.Messages.Add
   
    ' create an array of recipients
    
    recipCount = breakApart(Recipient, ";", recips())

  
   With MapiMessage
        If recipCount > 0 Then
            For Each recip In recips()
                Set MapiRecipient = .Recipients.Add
                MapiRecipient.Name = recip
                MapiRecipient.Type = mapiTo
            Next
        End If

      'MapiRecipient.Type = mapiBCC
      
      If Not IsMissing(attachment) Then
        Set MapiAttachment = .attachments.Add
        MapiAttachment.Name = attachment
        MapiAttachment.Type = CdoFileData
        MapiAttachment.ReadFromFile attachment
        MapiAttachment.Source = attachment
      End If
      
      For Recpt = 1 To .Recipients.Count
        .Recipients(Recpt).Resolve
      Next Recpt
      .Subject = Subject
      .Text = MessageText
      .Importance = mapiHigh
      .DeliveryReceipt = True
      .Send 'showdialog:=true  ' doesn't work under citrix :o(
   End With
   
   SendMapiMail = True
   
SendMapiMail_Exit:
   On Error Resume Next
   Close
   Set MapiSession = Nothing  ' Clear the object variable.
   DBEngine.Idle dbFreeLocks
   DBEngine.Idle dbRefreshCache
   Exit Function
   
SendMapiMail_Error:
   Dim DisplayMessage As String
   DisplayMessage = Choose(Application.CurrentObjectType, "Query", "Form", "Report", "Macro", "Module")
   DisplayMessage = DisplayMessage & vbTab & ": " & Application.CurrentObjectName & vbNewLine
   DisplayMessage = DisplayMessage & "Event" & vbTab & ": SendMapiMail" & vbNewLine
   DisplayMessage = DisplayMessage & "Error" & vbTab & ": " & Err & vbNewLine
   DisplayMessage = DisplayMessage & "MessageText" & vbTab & ": " & Error$ & vbNewLine
   DisplayMessage = DisplayMessage & vbNewLine & "If this error persists please note down these details together with what you were trying to do and call for technical assistance."
   Beep
   MsgBox DisplayMessage, vbCritical + vbOKOnly, "An error has occured in "
   SendMapiMail = False
   Resume SendMapiMail_Exit
   
End Function
