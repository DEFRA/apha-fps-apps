USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_AddMY_ProjectMonthFinal_Pact1998]
 (@cFPSVersion as VarChar(10), @vcFPSYear as int)
AS
declare @sqlstr as varchar(1200)
set @sqlstr ='INSERT INTO  MY_ProjectMonthFinal
(Year,
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
	[SumOfOnTime])
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
	[SumOfOnTime]

FROM  '+ @cFPSVersion + '.dbo.ProjectMonthFinal'

Exec(@sqlstr)

GO
