Option Compare Database
Option Explicit

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
            If tbl.Connect Like "*DATABASE=FPS2025*" Then
                tbl.Connect = "ODBC;DRIVER=SQL Server;SERVER=DEFACPVWTSQL003;DATABASE=FPS2025_CM;"
                tbl.RefreshLink
            End If
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
                    pty.Value = Replace(pty.Value, "vla88.cvlnt.vla.gov.uk", "DEFACPVWTSQL003")
                    Debug.Print qry.Name & ":   " & pty.Value
                    pty.Value = Replace(pty.Value, "FPS2025", "FPS2025_CM")
                    Debug.Print qry.Name & ":   " & pty.Value
                End If
            End If
        Next pty
        DoEvents
    Next i
        
    MsgBox "Done"
    
End Sub