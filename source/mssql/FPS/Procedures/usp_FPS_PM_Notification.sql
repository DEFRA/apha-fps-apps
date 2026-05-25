USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  StoredProcedure [dbo].[usp_FPS_PM_Notification]    Script Date: 03/05/2008 16:04:20 ******/

CREATE procEDURE [dbo].[usp_FPS_PM_Notification] AS

declare @msg varchar(1200)
set @msg=
'Dear recipient, <br><br>

New FPS Post Mortem Reports are available. Please click the  link <a href="http://vla63/postmortem/">Post Mortem Reports</a>.
<br><br>
Please do not reply to this message as it is sent automatically and comes from an unmonitored server.
<br><br>
In case of errors or difficulties using the application please contact APHA Service Desk.
<br><br>'




exec msdb.dbo.sp_send_dbmail 
	@profile_name='sqlmail',
    @recipients = 'FPSPostNot@vla.defra.gsi.gov.uk',
	@subject = 'FPS Post Mortem Reports',
    @body = @msg,
	@body_format='HTML'

GO
