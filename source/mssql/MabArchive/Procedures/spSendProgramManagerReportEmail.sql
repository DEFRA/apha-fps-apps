USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSendProgramManagerReportEmail] AS

DECLARE @b AS INT
SELECT @b =setting FROM [tbl Settings] WHERE ID='SendEmail'
IF @b=-1
BEGIN
	CREATE TABLE #ProgEmailList(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[mNumber] [varchar](50) NULL,
		[Name] [varchar](50) NULL,
		[Email] [varchar](200) NULL,
	 CONSTRAINT [PK_#ProgEmailList] PRIMARY KEY CLUSTERED  
	(
		[ID] ASC
	)) 

	INSERT INTO #ProgEmailList(mNumber, Name, Email)
		SELECT     MNumber, ProjectManager,  Email
		FROM         vProgramReports_Mail
		WHERE Disable=0 and EMAIL is not null
		GROUP BY MNumber, ProjectManager,  Email

	DECLARE @i as int
	DECLARE @mNumber as VARCHAR(50)
	DECLARE @Name as VARCHAR(50)
	DECLARE @Email as VARCHAR(200)
	SET @i=0

	SELECT TOP 1 @mNumber=Mnumber, @Name=Name, @Email=Email FROM #ProgEmailList
	ORDER BY ID 
	WHILE @@ROWCOUNT>0
		BEGIN
		EXEC [dbo].[spSendProgramReportNotification]
			@mNumber =@mNumber, @manager=@Name , @Email =@Email
		SET @i=@i+1
		SELECT TOP 1 @mNumber=Mnumber, @Name=Name, @Email=Email FROM #ProgEmailList
			WHERE ID>@i
		ORDER BY ID 
		END
	Drop table #ProgEmailList
END

GO
