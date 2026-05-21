USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--PL 01.10.2024
--Updated to run from DEFACPVWPSQL001 via linked servef to VLA88
--


CREATE procedure [dbo].[sp_AddMY_ProjectMonthFinal] (@cFPSVersion as VarChar(20), @vcFPSYear as int)
AS
declare @sqlstr as varchar(1200)
set @sqlstr ='INSERT INTO  MY_ProjectMonthFinal
SELECT ' + Cast(@vcFPSYear as VarChar(4)) +', 
	
	[Project] ,
	[MonthNo] ,
	[PeriodName] ,
	[CumFlag] ,
	[CostProfile] ,
	[Subcontracts] ,
	[Animals] ,
	[NonAnimals] ,
	[TimeCosts] ,
	[TransferCosts] ,
	[TotalCost] ,
	[Invoices] ,
	[COIW] ,
	[PortSales],
	[CumCost],
	[CumProfile] ,
	[SumOfCostProfile] ,
	[CumInvoices] ,
	[CumCOIW] ,
	[CumPortSales] ,
	[MstoneDue] ,
	[Due__Done] ,
	[OnTime] ,
	[SumOfMstoneDue] ,
	[SumOfDue__Done] ,
	[SumOfOnTime],
	[CWDebit] ,
	[CWCredit] ,
	[CumCWDebit] ,
	[CumCWCredit] ,
	[TotalHours] ,
	[CumTotalHours] ,
	[CumSubContracts] ,
	[CumTestCosts],
	PayCosts ,
	cumPayCosts 

FROM  '+ @cFPSVersion + '.dbo.ProjectMonthFinal'
Exec(@sqlstr)


GO
