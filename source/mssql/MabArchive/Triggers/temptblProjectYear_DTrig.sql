USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[temptblProjectYear_DTrig] ON [dbo].[temptblProjectYear] FOR DELETE AS
SET NOCOUNT ON
/* * CASCADE DELETES TO 'temptblAdditionalCosts' */
DELETE temptblAdditionalCosts FROM deleted, temptblAdditionalCosts WHERE deleted.Project = temptblAdditionalCosts.Project AND deleted.YearNo = temptblAdditionalCosts.Year
/* * CASCADE DELETES TO 'temptblAnimalReq' */
DELETE temptblAnimalReq FROM deleted, temptblAnimalReq WHERE deleted.Project = temptblAnimalReq.Project AND deleted.YearNo = temptblAnimalReq.Year
/* * CASCADE DELETES TO 'temptblStaffRequ' */
DELETE temptblStaffRequ FROM deleted, temptblStaffRequ WHERE deleted.Project = temptblStaffRequ.Project AND deleted.YearNo = temptblStaffRequ.Year
/* * CASCADE DELETES TO 'temptblTestReq' */
DELETE temptblTestReq FROM deleted, temptblTestReq WHERE deleted.Project = temptblTestReq.Project AND deleted.YearNo = temptblTestReq.Year


GO
