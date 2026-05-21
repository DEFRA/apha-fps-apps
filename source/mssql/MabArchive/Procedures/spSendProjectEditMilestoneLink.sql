USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ian G
-- Create date: 22/12/2008
-- Description:	Sends edit link for the Pims Milestones to project managers.
-- =============================================
CREATE PROCEDURE [dbo].[spSendProjectEditMilestoneLink]
	-- Add the parameters for the stored procedure here
	@mNumber Varchar(10), @Manager Varchar(50), @Email varchar(200)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON
	declare @lbody as varchar(5000)
	declare @lquery as varchar(5000)
	declare @m as int

	set @m=DATEPART(month, GETDATE())
	set @lquery ='SET NOCOUNT ON; SELECT EditLink FROM vProjectReports_PMMilestoneMail 
				WHERE Mnumber=' + '''' + @mNumber + ''''+ ' ORDER BY ParentProject ;SET NOCOUNT OFF;'
	set @lbody='Dear '  + @Manager + ',<br><br>
Here are the links to edit the milestones for your projects.  '

	IF @m IN(1,2,3,6,9,12)
		set @lbody=@lbody + 'You need to confirm the data for the milestones/deliverables due this month, even if you do not update it.'

	set  @lbody=@lbody + '<br><br>
If you are not the person named in the email, you are receiving this as a deputy for info only.  You will not be able to edit these milestones/deliverables.
<br><br>
Please do not reply to this email, as it is system generated and the account is not monitored.  For any questions relating to the data contained within the report, please contact the CAPS General Enquiry on x2237.  For errors with the application, please refer to the ITU helpesk.
<br><br>'

exec msdb.dbo.sp_send_dbmail   @profile_name =  'sqlmail' ,
       @recipients =  @Email,
       @subject =  'Milestone and Deliverable Update Request'  ,
       @body =  @lbody,
       @body_format =  'HTML' ,
       @query = @lquery ,
       @execute_query_database =  'MAB_Archive' ,
		@query_result_header =0,
       @attach_query_result_as_file =  0 ,
		@exclude_query_output =1

END

GO
