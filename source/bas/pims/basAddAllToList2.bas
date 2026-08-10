MODULE NAME: basAddAllToList2
CODE TYPE: VBA Module
---------------------------------------------

Option Compare Database
Option Explicit

      Function AddAllToList2(c As Control, ID As Long, Row As Long, _
      Col As Long, Code As Integer) As Variant

      '***************************************************************
      ' FUNCTION: AddAllToList2()
      '
      ' PURPOSE:
      '   Adds "(all)" as the first row of a combo box or list box.
      '
      ' USAGE:
      '   1. Create a combo box or list box that displays the data you
      '      want.
      '
      '   2. Change the RowSourceType property from "Table/Query" to
      '      "AddAllToList2."
      '
      '   3. Set the value of the combo box or list box's Tag property to
      '      the column number in which you want "(all)" to appear.
      '
      '   NOTE: Following the column number in the Tag property, you can
      '   enter a semicolon (;) and then any text you want to appear
      '   other than the default "all."
      '
      '         For example
      '
      '             Tag: 2;<None>
      '
      '         displays "<None>" in the second column of the list.
      '
      '***************************************************************
         Static db As Database, rs As Recordset
         Static DISPLAYID As Long
         Static DISPLAYCOL As Integer
         Static DISPLAYTEXT As String
         Dim Semicolon As Integer

      On Error GoTo Err_AddAllToList2

         Select Case Code
            Case LB_INITIALIZE
               ' See if the function is already in use.
               If DISPLAYID <> 0 Then
                  MsgBox "AddAllToList2 is already in use by another Control! "
                  AddAllToList2 = False
                  Exit Function
               End If

               ' Parse the display column and display text from the Tag
               ' property.
               DISPLAYCOL = 1
               DISPLAYTEXT = "(All)"
               If Not IsNull(c.Tag) Then
                  Semicolon = InStr(c.Tag, ";")
                  If Semicolon = 0 Then
                     DISPLAYCOL = Val(c.Tag)
                  Else
                     DISPLAYCOL = Val(Left(c.Tag, Semicolon - 1))
                     DISPLAYTEXT = Mid(c.Tag, Semicolon + 1)
                  End If
               End If

               ' Open the recordset defined in the RowSource property.
               Set db = DBEngine.Workspaces(0).Databases(0)
               Set rs = db.OpenRecordset(c.RowSource, DB_OPEN_SNAPSHOT)

               ' Record and return the ID for this function.
               DISPLAYID = Timer
               AddAllToList2 = DISPLAYID

            Case LB_OPEN
               AddAllToList2 = DISPLAYID

            Case LB_GETROWCOUNT
               ' Return the number of rows in the recordset.
               rs.MoveLast
               AddAllToList2 = rs.RecordCount + 1

            Case LB_GETCOLUMNCOUNT
               ' Return the number of fields (columns) in the recordset.
               AddAllToList2 = rs.Fields.Count

            Case LB_GETCOLUMNWIDTH
               AddAllToList2 = -1

            Case LB_GETVALUE
               ' Are you requesting the first row?
               If Row = 0 Then
                  ' Should the column display "(All)"?
                  If Col = DISPLAYCOL - 1 Then
                     ' If so, return "(All)."
                     AddAllToList2 = DISPLAYTEXT
                  Else
                     ' Otherwise, return NULL.
                     AddAllToList2 = Null
                  End If
               Else
                  ' Grab the record and field for the specified row/column.
                  rs.MoveFirst
                  rs.Move Row - 1
                  AddAllToList2 = rs(Col)
               End If
            Case LB_END
               DISPLAYID = 0
               rs.Close
         End Select

Bye_AddAllToList2:
         Exit Function

Err_AddAllToList2:
Beep:          MsgBox Error$, 16, "AddAllToList2"
         AddAllToList2 = False
         Resume Bye_AddAllToList2
      End Function
