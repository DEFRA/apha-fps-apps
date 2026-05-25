USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE trigger [dbo].[tblProjectYear_DTrig] ON [dbo].[tblProjectYear] FOR DELETE AS
SET NOCOUNT ON
/* * CASCADE DELETES TO 'tblAdditionalCosts' */
DELETE tblAdditionalCosts FROM deleted, tblAdditionalCosts WHERE deleted.Project = tblAdditionalCosts.Project AND deleted.YearNo = tblAdditionalCosts.Year
/* * CASCADE DELETES TO 'tblAnimalReq' */
DELETE tblAnimalReq FROM deleted, tblAnimalReq WHERE deleted.Project = tblAnimalReq.Project AND deleted.YearNo = tblAnimalReq.Year
/* * CASCADE DELETES TO 'tblStaffRequ' */
DELETE tblStaffRequ FROM deleted, tblStaffRequ WHERE deleted.Project = tblStaffRequ.Project AND deleted.YearNo = tblStaffRequ.Year
/* * CASCADE DELETES TO 'tblTestRequ' */
DELETE tblTestRequ FROM deleted, tblTestRequ WHERE deleted.Project = tblTestRequ.Project AND deleted.YearNo = tblTestRequ.Year


GO
