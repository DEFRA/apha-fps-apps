MODULE NAME: basBlob
CODE TYPE: VBA Module
---------------------------------------------

Option Explicit
Const BlockSize = 32768

'**************************************************************
' FUNCTION: ReadBLOB()
'
' PURPOSE:

'   Reads a BLOB from a disk file and stores the contents in the
'   specified table and field.
'
' PREREQUISITES:
'   The specified table with the OLE object field to contain the
'   binary data must be opened in Visual Basic code and the correct
'   record navigated to prior to calling the ReadBLOB() function.
'
' ARGUMENTS:
'   Source - The path and filename of the binary information
'            to be read and stored.
'   T      - The table object to store the data in.
'   Field  - The OLE object field in table T to store the data in.
'
' RETURN:
'   The number of bytes read from the Source file.
'**************************************************************
Function ReadBLOB(Source As String, T As DAO.Recordset, _
sField As String)
    Dim NumBlocks As Integer, SourceFile As Integer, i As Integer
    Dim FileLength As Long, LeftOver As Long
    Dim FileData As String
    Dim RetVal As Variant

    On Error GoTo Err_ReadBLOB

    ' Open the source file.
    SourceFile = FreeFile
    Open Source For Binary Access Read As SourceFile

    ' Get the length of the file.
    FileLength = LOF(SourceFile)
    If FileLength = 0 Then
        ReadBLOB = 0
        Exit Function
    End If

    ' Calculate the number of blocks to read and leftover bytes.
    NumBlocks = FileLength \ BlockSize
    LeftOver = FileLength Mod BlockSize

    ' SysCmd is used to manipulate status bar meter.
    RetVal = SysCmd(acSysCmdInitMeter, "Reading BLOB", _
             FileLength \ 1000)

    ' Put first record in edit mode.
    ' T.MoveFirst
    T.AddNew
    'T.Edit

    ' Read the leftover data, writing it to the table.
    FileData = String$(LeftOver, 32)
    Get SourceFile, , FileData
    T(sField).AppendChunk (FileData)

    RetVal = SysCmd(acSysCmdUpdateMeter, LeftOver / 1000)

    ' Read the remaining blocks of data, writing them to the table.
    FileData = String$(BlockSize, 32)
    For i = 1 To NumBlocks
        Get SourceFile, , FileData
        T(sField).AppendChunk (FileData)

        RetVal = SysCmd(acSysCmdUpdateMeter, BlockSize * i / 1000)
    Next i

    ' Update the record and terminate function.
    T.Update
    RetVal = SysCmd(acSysCmdRemoveMeter)
    Close SourceFile
    ReadBLOB = FileLength
    Exit Function

Err_ReadBLOB:
    ReadBLOB = -Err
    MsgBox Err.Number & Err.Description
    Exit Function

End Function

'**************************************************************
' FUNCTION: WriteBLOB()
'
' PURPOSE:
'   Writes BLOB information stored in the specified table and field
'   to the specified disk file.
'
' PREREQUISITES:
'   The specified table with the OLE object field containing the
'   binary data must be opened in Visual Basic code and the correct
'   record navigated to prior to calling the WriteBLOB() function.
'
' ARGUMENTS:
'   T           - The table object containing the binary information.
'   sField      - The OLE object field in table T containing the
'                 binary information to write.
'   Destination - The path and filename to write the binary
'                 information to.
'
' RETURN:
'   The number of bytes written to the destination file.
'**************************************************************
Function WriteBLOB(T As DAO.Recordset, sField As String, _
Destination As String)
    Dim NumBlocks As Integer, DestFile As Integer, i As Integer
    Dim FileLength As Long, LeftOver As Long
    Dim FileData As String
    Dim RetVal As Variant

    On Error GoTo Err_WriteBLOB

    ' Get the size of the field.
    FileLength = T(sField).FieldSize()
    If FileLength = 0 Then
        WriteBLOB = 0
        Exit Function
    End If

    ' Calculate number of blocks to write and leftover bytes.
    NumBlocks = FileLength \ BlockSize
    LeftOver = FileLength Mod BlockSize

    ' Remove any existing destination file.
    DestFile = FreeFile
    Open Destination For Output As DestFile
    Close DestFile

    ' Open the destination file.
    Open Destination For Binary As DestFile

    ' SysCmd is used to manipulate the status bar meter.
    RetVal = SysCmd(acSysCmdInitMeter, _
    "Writing BLOB", FileLength / 1000)

    ' Write the leftover data to the output file.
    FileData = T(sField).GetChunk(0, LeftOver)
    Put DestFile, , FileData

    ' Update the status bar meter.
    RetVal = SysCmd(acSysCmdUpdateMeter, LeftOver / 1000)

    ' Write the remaining blocks of data to the output file.
    For i = 1 To NumBlocks
        ' Reads a chunk and writes it to output file.
        FileData = T(sField).GetChunk((i - 1) * BlockSize _
           + LeftOver, BlockSize)
        Put DestFile, , FileData

        RetVal = SysCmd(acSysCmdUpdateMeter, _
        ((i - 1) * BlockSize + LeftOver) / 1000)
    Next i

    ' Terminates function
    RetVal = SysCmd(acSysCmdRemoveMeter)
    Close DestFile
    WriteBLOB = FileLength
    Exit Function

Err_WriteBLOB:
    WriteBLOB = -Err
    Exit Function

End Function

'**************************************************************
' SUB: CopyFile
'
' PURPOSE:
'   Demonstrates how to use ReadBLOB() and WriteBLOB().
'
' PREREQUISITES:
'   A table called BLOB that contains an OLE Object field called
'   Blob.
'
' ARGUMENTS:
'   Source - The path and filename of the information to copy.
'   Destination - The path and filename of the file to write
'                 the binary information to.
'
' EXAMPLE:
'   CopyFile "c:\windows\winfile.hlp", "c:\windows\winfil_1.hlp"
'**************************************************************
Sub CopyFile(Source As String, Destination As String)
    Dim BytesRead As Variant, BytesWritten As Variant
    Dim Msg As String
    Dim db As DAO.Database
    Dim T As DAO.Recordset

    ' Open the BLOB table.
    Set db = CurrentDb()
    Set T = db.OpenRecordset("BLOB", dbOpenTable)

    ' Create a new record and move to it.
    T.AddNew
    T.Update
    T.MoveLast

    BytesRead = ReadBLOB(Source, T, "Blob")

    Msg = "Finished reading """ & Source & """"
    Msg = Msg & Chr$(13) & ".. " & BytesRead & " bytes read."
    MsgBox Msg, 64, "Copy File"

    BytesWritten = WriteBLOB(T, "Blob", Destination)

    Msg = "Finished writing """ & Destination & """"
    Msg = Msg & Chr$(13) & ".. " & BytesWritten & " bytes written."
    MsgBox Msg, 64, "Copy File"
End Sub

    
