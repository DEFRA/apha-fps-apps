Option Compare Database   'Use database order for string comparisons
Option Explicit

'Global Const CONNECT_Testing = "ODBC;DSN=FPS2001_FDS;APP=Microsoft Access;WSID=;DATABASE=FPS2001"
'Global Const CONNECT_Testing = "ODBC;DRIVER=SQL Server;SERVER=vla44;WSID=PCH01051;DATABASE=fps2005;"
Dim TestArray() As Integer  ' Declare dynamic array.


Global GlobalWGVar As String
Global GlobalNoRep As Integer
Global GlobalMaxRun As Integer
Global GlobalVetWG  As String
 Dim Fttable As Recordset

Sub ReDim_Demo()

    Dim Size As Integer ' Declare Integer variable.
    Dim i As Integer

    Size = Int(100 * Rnd + 1)   ' Generate random number.
    ReDim Preserve TestArray(Size)   ' Make array have Size elements.

    For i = 1 To Size   ' Index for number of elements.
        TestArray(i) = Rnd  ' Put number in each element.
    Next i

    ReDim Preserve TestArray(Size * 10)  ' Make array ten times larger.

    For i = 1 To Size * 10  ' Index for number of elements.
        TestArray(i) = Rnd  ' Put number in each element.
    Next i

End Sub

Function RetGlobalVetWG() As String
RetGlobalVetWG = GlobalVetWG
End Function

Function RetGlobalWGVar() As String
RetGlobalWGVar = GlobalWGVar
End Function