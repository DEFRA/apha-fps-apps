Option Compare Database
Option Explicit

Function LogEvent(sEvent As String)
'LogEvent ("Startup")
'Needs SystemName and Runmode in settings

If SettingsGet("Runmode") = "Live" Then
    On Error Resume Next
End If

Dim db As DAO.Database
Dim qd As DAO.QueryDef
Dim sql As String
Dim con  As String

sql = "Insert into dbo.AccessLog([DB],[Event],[DBUser]) Values (" & _
    "'" & SettingsGet("SystemName") & "', " & _
    "'" & sEvent & "'," & _
    "'" & UserName() & "')"

If SettingsGet("Runmode") = "Live" Then
    con = "ODBC;Driver={SQL Server};Server=vla88.cvlnt.vla.gov.uk;Database=AccessLog;Uid=AccessLogUser;Pwd=JFjFu8f7Fyh;"
Else
    'con = "ODBC;Driver={SQL Server};Server=vla88test.cvlnt.vla.gov.uk;Database=AccessLog;Uid=AccessLogUser;Pwd=JFjFu8f7Fyh;"
    con = "ODBC;Driver={SQL Server};Server=DEFACPVWTSQL004;Database=AccessLog;Uid=AccessLogUser;Pwd=JFjFu8f7Fyh;"
End If

Set db = CurrentDb
Set qd = db.CreateQueryDef

With qd
    .Name = ""
    .Connect = con
    .sql = sql
    .ReturnsRecords = False
    .Execute
End With

Set db = Nothing
Set qd = Nothing

End Function