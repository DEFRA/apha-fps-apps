Option Compare Database
Option Explicit
Function ConnectStr()

    ConnectStr = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("SQLServer") & ";DATABASE=" & SettingsGet("SQLDatabase") & ";Trusted_Connection=Yes;"

End Function

Function MABConnectStr()

    MABConnectStr = "ODBC;DRIVER=SQL Server;SERVER=" & SettingsGet("MABServer") & ";DATABASE=" & SettingsGet("MABDatabase") & ";Trusted_Connection=Yes;"

End Function