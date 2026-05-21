USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ian G
-- Create date: 22/12/2008
-- Description:	Sends notification of the Pims project reports to project managers.
-- Change Log: See below
-- VA 15/05/2012 - Amended e-mail text following request by Steve Martin, F0430206
-- IG 24/09/2013 - text change from Steve Martin
-- =============================================
CREATE PROCEDURE [dbo].[spSendProjectReportNotification]
	@mNumber Varchar(10),
	@Manager Varchar(50),
	@Email Varchar(200)
AS
BEGIN
	SET NOCOUNT ON
	
	DECLARE @lbody AS Varchar(5000)
	DECLARE @lquery AS Varchar(5000)
	
	SET @lquery = 'SET NOCOUNT ON;SELECT HLink FROM vProjectReports_PMMail WHERE Mnumber=' + '''' + @mNumber + '''' + ' Order By ParentProject ;SET NOCOUNT OFF;'
	SET @lbody= 'Dear ' + @Manager + ',
		<br>
		<br>
		<h2>Monthly Project Reports</h2>
		<h3>Information (The text below was last updated on 24th September 2013)</h3>
		<ul>
			<li>Expenditure shown against each project is using the appropriate rate, whether the standard rate (“Non-Defra”) or the reduced rate (“Defra”) for hours, tests and animals.</li>
			<li>There are sub-reports for time, test, animal and project-specific costs via the column heading shown in blue text. (For example, “Time” will take you to the sub-report that details the hours recorded by individual by month).</li>
			<li>The report distribution list allows the project leader and any nominated deputy to receive an emailed notification when their project reports have been updated (please contact <a href="mailto:CAPSmailbox@vla.defra.gsi.gov.uk">CAPS Time and Test Processing</a> if you wish to add an individual to the distribution list). </li>
			<li>Guidance can be accessed via the link: <a href="http:/vla43/caps-guidance-document-monthly-project-reports.pdf" >CAPS Guidance Document (002)</a></li>
		</ul>
		<p>Please do not reply to this email, as it is system generated and the account is not monitored. For any questions relating to the data contained within the report, please contact the CAPS General Enquiry on x2237. For errors with the application, please refer to the APHA Service Desk.</p>'

	EXEC msdb.dbo.sp_send_dbmail
		@profile_name = 'sqlmail',
		@recipients = @Email,
		@subject = 'Monthly Project Reports',
		@body = @lbody,
		@body_format = 'HTML',
		@query = @lquery,
		@execute_query_database = 'MAB_Archive',
		@query_result_header = 0,
		@attach_query_result_as_file = 0,
		@exclude_query_output = 1
END

GO
