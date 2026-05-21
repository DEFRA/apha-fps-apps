USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[sp_Close_FPS] AS
/* Removes editing ability to FPS, but not view 	*/
/* Microsoft SQL Server - Scripting			*/
/* Server: CVLO					*/
/* Database: FPS2000					*/
/* Creation Date 3/6/00 5:20:46 PM 			*/

REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblBid]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblBid]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblBid]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblContract]  TO [ContractManager]
REVOKE    INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAnimalReq]  TO [public]
REVOKE    INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAnimalReq]  TO [AnimalManager]
REVOKE    INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAnimalReq]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblPurchase]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblPurchase]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblPurchase]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [TestManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [TestManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAdditionalCosts]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAdditionalCosts]  TO [AnimalManager]
REVOKE   INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAdditionalCosts]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblWGEmployee]  TO [AnimalManager]
REVOKE   UPDATE  ON [dbo].[vtblWGEmployee]  TO [ContractManager]
REVOKE   UPDATE  ON [dbo].[vtblWGEmployee]  TO [TestManager]
REVOKE   UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [AnimalManager]
REVOKE  UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [ContractManager]
REVOKE  UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [TestManager]
REVOKE  UPDATE  ON [dbo].[vtblStaff]  TO [public]
REVOKE  UPDATE  ON [dbo].[vtblStaff]  TO [ContractManager]
REVOKE  UPDATE  ON [dbo].[vtblStaff]  TO [TestManager]
REVOKE  UPDATE  ON [dbo].[vWorkGroup]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vWorkGroup]  TO [AnimalManager]
REVOKE  UPDATE  ON [dbo].[vWorkGroupGrade]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vWorkGroupGrade]  TO [AnimalManager]
REVOKE  UPDATE  ON [dbo].[vtblWGEmployee]  TO [public]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblWGEmployee]  TO [AnimalManager]
REVOKE    UPDATE  ON [dbo].[vtblWGEmployee]  TO [ContractManager]
REVOKE  UPDATE  ON [dbo].[vtblWGEmployee]  TO [TestManager]

REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[tblAnimals]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vProfitCentreGrade]  TO [AnimalManager]
REVOKE   UPDATE  ON [dbo].[vtblStaff]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtlkpProject]  TO [AnimalManager]
REVOKE   UPDATE  ON [dbo].[vtlkpProgram]  TO [AnimalManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vContractProject]  TO [ContractManager]
REVOKE  UPDATE  ON [dbo].[vtlkpProgram]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtlkpProject]  TO [ContractManager]
REVOKE  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vTestOrProduct_TM]  TO [TestManager]

GO
