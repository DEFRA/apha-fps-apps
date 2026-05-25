USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procEDURE [dbo].[sp_Open_FPS] AS
/* Gives Edit Permisions to users		*/
/* Server: CVLO					*/
/* Database: FPS2000					*/
/* Creation Date 3/6/00 5:20:46 PM 			*/
/* Microsoft SQL Server - Scripting			*/
/* Server: CVLO					*/
/* Database: FPS2000					*/
/* Creation Date 3/6/00 5:20:46 PM 			*/
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblBid]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblBid]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblBid]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblContract]  TO [ContractManager]
GRANT    INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAnimalReq]  TO [public]
GRANT    INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAnimalReq]  TO [AnimalManager]
GRANT    INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAnimalReq]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblPurchase]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblPurchase]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblPurchase]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblStaffJob]  TO [TestManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblTestRequ]  TO [TestManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAdditionalCosts]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAdditionalCosts]  TO [AnimalManager]
GRANT   INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblAdditionalCosts]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblWGEmployee]  TO [AnimalManager]
GRANT   UPDATE  ON [dbo].[vtblWGEmployee]  TO [ContractManager]
GRANT   UPDATE  ON [dbo].[vtblWGEmployee]  TO [TestManager]
GRANT   UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [AnimalManager]
GRANT  UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [ContractManager]
GRANT  UPDATE  ON [dbo].[vtblkpProfitCentre]  TO [TestManager]
GRANT  UPDATE  ON [dbo].[vtblStaff]  TO [public]
GRANT  UPDATE  ON [dbo].[vtblStaff]  TO [ContractManager]
GRANT  UPDATE  ON [dbo].[vtblStaff]  TO [TestManager]
GRANT  UPDATE  ON [dbo].[vWorkGroup]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vWorkGroup]  TO [AnimalManager]
GRANT  UPDATE  ON [dbo].[vWorkGroupGrade]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vWorkGroupGrade]  TO [AnimalManager]
GRANT  UPDATE  ON [dbo].[vtblWGEmployee]  TO [public]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtblWGEmployee]  TO [AnimalManager]
GRANT    UPDATE  ON [dbo].[vtblWGEmployee]  TO [ContractManager]
GRANT  UPDATE  ON [dbo].[vtblWGEmployee]  TO [TestManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[tblAnimals]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vProfitCentreGrade]  TO [AnimalManager]
GRANT   UPDATE  ON [dbo].[vtblStaff]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtlkpProject]  TO [AnimalManager]
GRANT   UPDATE  ON [dbo].[vtlkpProgram]  TO [AnimalManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vContractProject]  TO [ContractManager]
GRANT  UPDATE  ON [dbo].[vtlkpProgram]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vtlkpProject]  TO [ContractManager]
GRANT  INSERT ,  DELETE ,  UPDATE  ON [dbo].[vTestOrProduct_TM]  TO [TestManager]

GO
