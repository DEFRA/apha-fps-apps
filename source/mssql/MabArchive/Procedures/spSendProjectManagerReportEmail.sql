USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSendProjectManagerReportEmail] AS

DECLARE @b AS INT
SELECT @b =setting FROM [tbl Settings] WHERE ID='SendEmail'
IF @b=-1
BEGIN
	CREATE TABLE #PMREmailList(
		[ID] [int] IDENTITY(1,1) NOT NULL,
		[mNumber] [varchar](50) NULL,
		[Name] [varchar](50) NULL,
		[Email] [varchar](200) NULL,
	 CONSTRAINT [PK_EmailList] PRIMARY KEY CLUSTERED  
	(
		[ID] ASC
	)) 

	INSERT INTO #PMREmailList(mNumber, Name, Email)
		SELECT     MNumber, ProjectManager,  Email
		FROM         vProjectReports_PMMail
		WHERE (Disable=0) and email is not null
		GROUP BY MNumber, ProjectManager,  Email

	DECLARE @i as int
	DECLARE @mNumber as VARCHAR(50)
	DECLARE @Name as VARCHAR(50)
	DECLARE @Email as VARCHAR(200)
	SET @i=0

	SELECT TOP 1 @mNumber=Mnumber, @Name=Name, @Email=Email FROM #PMREmailList
	ORDER BY ID 
	WHILE @@ROWCOUNT>0
		BEGIN
		EXEC [dbo].[spSendProjectReportNotification]
			@mNumber =@mNumber, @manager=@Name , @Email =@Email
		SET @i=@i+1
		SELECT TOP 1 @mNumber=Mnumber, @Name=Name, @Email=Email FROM #PMREmailList
			WHERE ID>@i
		ORDER BY ID 
		END

	Drop table #PMREmailList

set @Email=(select setting from [tbl Settings] where id='CAPSMailbox')

exec msdb.dbo.sp_send_dbmail   @profile_name =  'sqlmail' ,
       @recipients =  @Email,
       @subject =  'PIMS Monthly Report Links'  ,
       @body =  'The project managers report links have been mailed out.<br><br>
Please do not reply to this email.  If you are having problems please contact the helpdesk.',
       @body_format =  'HTML' 
END

GO
