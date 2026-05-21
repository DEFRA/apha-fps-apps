USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ian G
-- Create date: 17/01/2011
-- Description:	Sends notification of the Resource Centre reports to RC managers.
-- =============================================
CREATE PROCEDURE [dbo].[spSendRCReportNotification]
	-- Add the parameters for the stored procedure here
	@mNumber Varchar(10), @Manager Varchar(50), @Email varchar(200)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON
	declare @lbody as varchar(5000)
	declare @lquery as varchar(5000)
	
	set @lquery ='SET NOCOUNT ON;SELECT HLink1, HLink2 FROM vRCReports_Mail WHERE Mnumber=' + '''' + @mNumber + '''' + ' Order By ProfitCentre ;SET NOCOUNT OFF;'
	set @lbody='Dear '  + @Manager + ',<br><br>
Here are the links to your resource centre''s  reports for the current month.  
<br><br>
Please do not reply to this email, as it is system generated and the account is not monitored.  For any questions relating to the data contained within the report, please contact the CAPS General Enquiry on x2237.  For errors with the application, please refer to the <a href="mailto:AHVLAITServiceDesk@ahvla.gsi.gov.uk">AHVLA IT Service Desk (AHVLA)</a>.
<br><br>
'

exec msdb.dbo.sp_send_dbmail   @profile_name =  'sqlmail' ,
       @recipients =  @Email,
       @subject =  'Monthly Reports for Resource Centre'  ,
       @body =  @lbody,
       @body_format =  'HTML' ,
       @query = @lquery ,
       @execute_query_database =  'MAB_Archive' ,
		@query_result_header =0,
       @attach_query_result_as_file =  0 ,
		@exclude_query_output =1

END

GO
