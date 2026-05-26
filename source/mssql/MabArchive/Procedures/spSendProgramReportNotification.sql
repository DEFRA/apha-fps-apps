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
CREATE PROCEDURE [dbo].[spSendProgramReportNotification]
	-- Add the parameters for the stored procedure here
	@mNumber Varchar(10), @Manager Varchar(50), @Email varchar(200)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON
	declare @lbody as varchar(5000)
	declare @lquery as varchar(5000)
	
	set @lquery ='SET NOCOUNT ON;SELECT HLink  FROM vProgramReports_Mail WHERE Mnumber=' + '''' + @mNumber + '''' + ' Order By Program ;SET NOCOUNT OFF;'
--	set @lbody='Dear '  + @Manager + ',<br><br>
--Here are the links to your program reports for the current month.  
--<br><br>
--Please do not reply to this email, as it is system generated and the account is not monitored.  For any questions relating to the data contained within the report, please contact the CAPS General Enquiry on x2237.  For errors with the application, please refer to the ITU helpesk.
--<br><br>
--'
	set @lbody='Dear '  + @Manager + ',<br><br>
<h2 > Programme Expenditure Reports – April 2011 onwards.
</h2>
<h3 >Background
</h3>
<p >
CAPS have been working with ITU to improve programme expenditure reports. The developments were specifically designed to automate the process for distributing reports (via links to intraVet) and to update the content of the reports where necessary. 
</p>
<h3 >Changes
</h3>
<p>
The main changes are:
</p>
 
<ul  type="disc">
<li >
An email will be sent to the programme manager and deputy (where applicable) which contains hyperlinks to programme-specific reports. This emailed link will be sent automatically once project expenditure information has been finalised for the month. 
 </li>
 </ul>
<ul  type="disc">
<li>
The reports have not been significantly changed, but rather re-formatted so that the information is more relevant. 

</li> </ul>
<ul  type="disc">
<li>
A new set of reports have been designed specifically for the Research & development Internal Investment Fund (RDIIF) Programme. 

 </li></ul>
<h3 >Guidance
</h3>
<p>
CAPS Guidance Document (003) is available on intraVet and has been updated to include the changes described above
</p>
Please do not reply to this email, as it is system generated and the account is not monitored.  For any questions relating to the data contained within the report, please contact the CAPS General Enquiry on x2237.  For errors with the application, please refer to the APHA Service Desk..
<br><br>
'
exec msdb.dbo.sp_send_dbmail   @profile_name =  'sqlmail' ,
       @recipients =  @Email,
       @subject =  'Monthly Reports for Program'  ,
       @body =  @lbody,
       @body_format =  'HTML' ,
       @query = @lquery ,
       @execute_query_database =  'MAB_Archive' ,
		@query_result_header =0,
       @attach_query_result_as_file =  0 ,
		@exclude_query_output =1

END

GO
