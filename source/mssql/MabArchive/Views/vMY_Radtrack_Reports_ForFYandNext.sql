USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vMY_Radtrack_Reports_ForFYandNext]
AS
SELECT     dbo.MY_Radtrack_Reports.Year, dbo.MY_Radtrack_Reports.Project, dbo.MY_Radtrack_Reports.Type, dbo.MY_Radtrack_Reports.Reminder1, 
                      dbo.MY_Radtrack_Reports.Reminder2, dbo.MY_Radtrack_Reports.ReplyReceived, dbo.MY_Radtrack_Reports.SentToProgManager, 
                      dbo.MY_Radtrack_Reports.SentToProjLeader, dbo.MY_Radtrack_Reports.EmailedToCustomer, dbo.MY_Radtrack_Reports.SignedCopyToCustomer, 
                      dbo.MY_Radtrack_Reports.RepDueDate, dbo.MY_Radtrack_Reports.ID, CASE WHEN EmailedToCustomer IS NULL THEN NULL 
                      WHEN Repduedate IS NULL THEN NULL WHEN emailedtocustomer <= repduedate THEN 'Yes' ELSE 'No' END AS OnTime
FROM         dbo.vLatestMonthYear CROSS JOIN
                      dbo.MY_Radtrack_Reports
WHERE     (dbo.vLatestMonthYear.Year = dbo.MY_Radtrack_Reports.Year) OR
                      (dbo.vLatestMonthYear.Year + 1 = dbo.MY_Radtrack_Reports.Year)

GO
