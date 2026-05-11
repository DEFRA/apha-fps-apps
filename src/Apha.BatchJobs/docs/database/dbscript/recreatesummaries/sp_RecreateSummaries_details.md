# sp_RecreateSummaries - Stored Procedure Details

## Location
- FPS2025/Procedures/sp_RecreateSummaries.sql

## Summary
sp_RecreateSummaries is an orchestration stored procedure that rebuilds monthly summary/calculation data, logs execution, and conditionally refreshes period output tables based on lock status.

## External Sources Used

| Object | Source Path | Notes |
|--------|-------------|-------|
| sp_Get_SP_No | Z:/Lot2/DB/sp_Get_SP_NO.sql | Provided externally by user; not under current workspace root |

## Definition
- Procedure: dbo.sp_RecreateSummaries
- Input parameter: @Month int

## Main Execution Flow
The procedure executes the following in order:
1. sp_deleteFPSTotals
2. sp_createFPSTotals
3. sp_InsertMissingProjects
4. sp_deleteTimeCostCalcs
5. sp_CreateTimeCostCalcs
6. sp_DeleteProjectMonthCasework
7. sp_CreateProjectMonthCasework
8. sp_DeleteProjectMonthFinal
9. sp_deleteProjectMonth2
10. sp_qryJobMonth_Single
11. sp_DeleteProjectMonth3
12. sp_qryJobMonthCum
13. sp_qryJobMonth_Final @Month
14. usp_LogRecreateSummaries @Month

## Lock Check and Conditional Refresh
After logging, the procedure reads period lock state:
- select @periodLocked = periodLocked from tblPeriod where endperiod = @month

If @periodLocked = 0 (period not locked), it runs:
- usp_Refresh_Period_MO @month
- usp_Refresh_Period_psc @month
- usp_Refresh_Period_tcc @month

If the period is locked, these refresh procedures are skipped.

## Related Objects
### Logging Procedure
- FPS2025/Procedures/usp_LogRecreateSummaries.sql
- Captures current user identifier via sp_Get_SP_No
- Inserts into RecreateSummaries_Log(UserID, Period, DateDone)

### Log Table
- FPS2025/Tables/RecreateSummaries_Log.sql
- Columns:
  - ID (identity, PK)
  - UserID (varchar(20))
  - Period (smallint)
  - DateDone (datetime)

## Search Notes Across Root and Subfolders
- Found active definition in FPS2025 only.
- Checked MAB_ARCHIVE/Procedures and did not find sp_RecreateSummaries there.

## Practical Interpretation
This procedure is a monthly rebuild pipeline for FPS summaries:
- Clears and recreates totals and time-cost calculations.
- Rebuilds project month datasets.
- Produces final month and cumulative outputs.
- Logs execution metadata.
- Protects locked periods by skipping final refreshes.

## SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  Stored Procedure dbo.sp_RecreateSummaries    Script Date: 3/4/00 1:48:23 PM ******/
/****** Object:  Stored Procedure dbo.sp_RecreateSummaries    Script Date: 1/12/99 12:14:26 PM ******/
CREATE PROC [dbo].[sp_RecreateSummaries] @Month int AS
declare @periodLocked as smallint

EXECUTE sp_deleteFPSTotals
EXECUTE sp_createFPSTotals
EXECUTE sp_InsertMissingProjects
EXECUTE sp_deleteTimeCostCalcs 
EXECUTE sp_CreateTimeCostCalcs
EXECUTE sp_DeleteProjectMonthCasework
EXECUTE sp_CreateProjectMonthCasework
EXECUTE sp_DeleteProjectMonthFinal 
EXECUTE sp_deleteProjectMonth2 
EXECUTE sp_qryJobMonth_Single
EXECUTE sp_DeleteProjectMonth3
EXECUTE sp_qryJobMonthCum 
EXECUTE sp_qryJobMonth_Final @Month
EXECUTE usp_LogRecreateSummaries @Month
select @periodLocked=periodLocked
FROM         tblPeriod
where endperiod=@month

if @periodLocked=0
begin
  EXECUTE usp_Refresh_Period_MO @month
  EXECUTE usp_Refresh_Period_psc @month
  EXECUTE usp_Refresh_Period_tcc @month
end

GO
```

## Dependency Check: sp_deleteFPSTotals

### Result
Dependent child object exists.

### Dependent Child Object: FPSYearTotals (Table)

### Location
- FPS2025/Tables/FPSYearTotals.sql

### Why Dependent
sp_deleteFPSTotals executes DELETE from FPSYearTotals, so the table is a direct dependency.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FPSYearTotals](
  [ParentProject] [varchar](20) NOT NULL,
  [Program] [varchar](10) NOT NULL,
  [TotalAdditionalCosts] [money] NULL,
  [TotalAnimalCosts] [float] NULL,
  [TotalStaffCosts] [float] NULL,
  [TotalTestCosts] [float] NULL,
  [TotalCosts] [float] NULL,
  [CustIncome] [money] NOT NULL,
  [TransferIncome] [money] NOT NULL,
  [TotalIncome] [money] NOT NULL,
  [Budget_CVL] [money] NULL,
  [RequiredProfit] [money] NULL,
  [Manager] [varchar](50) NULL,
  [Customer] [varchar](50) NULL,
  [ProjectStatus] [varchar](50) NULL,
  [PVSIncome] [money] NULL,
  [PlanCaseworkDebit] [money] NULL,
  [TotalPayCosts] [float] NULL
,    CONSTRAINT [PK_FPSYearTotals] PRIMARY KEY CLUSTERED
  (
    ParentProject
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No additional child procedure dependency exists inside sp_deleteFPSTotals (no EXEC statements).
- No trigger dependency on FPSYearTotals was found under FPS2025/Triggers.

## Dependency Check: sp_createFPSTotals

### Result
Dependent child object exists.

### Dependent Child Object: FPSYearTotals (Table)

### Location
- FPS2025/Tables/FPSYearTotals.sql

### Why Dependent
sp_createFPSTotals executes INSERT INTO FPSYearTotals.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FPSYearTotals](
  [ParentProject] [varchar](20) NOT NULL,
  [Program] [varchar](10) NOT NULL,
  [TotalAdditionalCosts] [money] NULL,
  [TotalAnimalCosts] [float] NULL,
  [TotalStaffCosts] [float] NULL,
  [TotalTestCosts] [float] NULL,
  [TotalCosts] [float] NULL,
  [CustIncome] [money] NOT NULL,
  [TransferIncome] [money] NOT NULL,
  [TotalIncome] [money] NOT NULL,
  [Budget_CVL] [money] NULL,
  [RequiredProfit] [money] NULL,
  [Manager] [varchar](50) NULL,
  [Customer] [varchar](50) NULL,
  [ProjectStatus] [varchar](50) NULL,
  [PVSIncome] [money] NULL,
  [PlanCaseworkDebit] [money] NULL,
  [TotalPayCosts] [float] NULL
,    CONSTRAINT [PK_FPSYearTotals] PRIMARY KEY CLUSTERED
  (
    ParentProject
  )
) ON [PRIMARY]
GO
```

### Dependent Child Object: qryTotalAdditionalCosts (View)

### Location
- FPS2025/Views/qryTotalAdditionalCosts.sql

### Why Dependent
sp_createFPSTotals selects FROM qryTotalAdditionalCosts via LEFT JOIN on tlkpProject.ParentProject.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryTotalAdditionalCosts    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[qryTotalAdditionalCosts] AS
SELECT DISTINCT tblAdditionalCosts.JobCode, 
Sum(tblAdditionalCosts.ItemCost) AS TotalAdditionalCosts
FROM tblAdditionalCosts
GROUP BY tblAdditionalCosts.JobCode

GO
```

### Dependent Child Object: qryTotalAnimalCosts (View)

### Location
- FPS2025/Views/qryTotalAnimalCosts.sql

### Why Dependent
sp_createFPSTotals selects FROM qryTotalAnimalCosts via LEFT JOIN on tlkpProject.ParentProject.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryTotalAnimalCosts]
AS
SELECT DISTINCT ParentProject AS JobCode, SUM(Cost) AS TotalAnimalCosts
FROM         dbo.vProjectAnimalPlan
GROUP BY ParentProject

GO
```

### Dependent Child Object: qryTotalStaffCosts (View)

### Location
- FPS2025/Views/qryTotalStaffCosts.sql

### Why Dependent
sp_createFPSTotals selects FROM qryTotalStaffCosts via LEFT JOIN on tlkpProject.ParentProject.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryTotalStaffCosts]
AS
SELECT DISTINCT ParentProject AS JobCode, SUM(Cost) AS TotalStaffCosts, SUM(PayCost) AS TotalPayCosts
FROM         dbo.vProjectStaffPlan
GROUP BY ParentProject

GO
```

### Dependent Child Object: qryTotalTestCosts (View)

### Location
- FPS2025/Views/qryTotalTestCosts.sql

### Why Dependent
sp_createFPSTotals selects FROM qryTotalTestCosts via LEFT JOIN on tlkpProject.ParentProject.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryTotalTestCosts    Script Date: 3/4/00 1:48:17 PM ******/
CREATE VIEW [dbo].[qryTotalTestCosts] AS
SELECT DISTINCT vtblTestRequ.JobCode, Sum(NoTests*TestPrice) AS TotalTestCosts
FROM vtblTestRequ
GROUP BY vtblTestRequ.JobCode

GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_createFPSTotals (no EXEC statements).

## Dependency Check: sp_InsertMissingProjects

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonth (Table)

### Location
- FPS2025/Tables/ProjectMonth.sql

### Why Dependent
sp_InsertMissingProjects executes INSERT INTO ProjectMonth.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [int] NOT NULL,
  [CostProfile] [money] NULL
,    CONSTRAINT [PK_ProjectMonth_1__16] PRIMARY KEY CLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_InsertMissingProjects (no EXEC statements).

## Dependency Check: sp_deleteTimeCostCalcs

### Result
Dependent child object exists.

### Dependent Child Object: TimeCostCalcs (Table)

### Location
- FPS2025/Tables/TimeCostCalcs.sql

### Why Dependent
sp_deleteTimeCostCalcs executes DELETE FROM timecostcalcs.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimeCostCalcs](
  [WorkGroup] [varchar](50) NOT NULL,
  [JobCode] [varchar](50) NOT NULL,
  [Project] [varchar](20) NOT NULL,
  [Month] [float] NOT NULL,
  [StaffID] [varchar](50) NOT NULL,
  [GradeCode] [varchar](10) NULL,
  [Name] [varchar](50) NULL,
  [ChargeRate] [money] NULL,
  [Class] [varchar](255) NULL,
  [Time] [float] NULL,
  [Cost] [float] NULL,
  [Division] [varchar](10) NULL,
  [JobCodeOld] [varchar](14) NULL,
  [Pay] [money] NULL,
  [NonPay] [money] NULL,
  [Overhead] [money] NULL
,    CONSTRAINT [aaaaaTimeCostCalcs_PK] PRIMARY KEY NONCLUSTERED
  (
    WorkGroup, JobCode, Project, Month, StaffID
  )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Class] ON [dbo].[TimeCostCalcs]
(
  Class
)
GO
CREATE NONCLUSTERED INDEX [Project] ON [dbo].[TimeCostCalcs]
(
  Project
)
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_deleteTimeCostCalcs (no EXEC statements).

## Dependency Check: sp_CreateTimeCostCalcs

### Result
Dependent child object exists.

### Dependent Child Object: TimeCostCalcs (Table)

### Location
- FPS2025/Tables/TimeCostCalcs.sql

### Why Dependent
sp_CreateTimeCostCalcs executes INSERT INTO TimeCostCalcs.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimeCostCalcs](
  [WorkGroup] [varchar](50) NOT NULL,
  [JobCode] [varchar](50) NOT NULL,
  [Project] [varchar](20) NOT NULL,
  [Month] [float] NOT NULL,
  [StaffID] [varchar](50) NOT NULL,
  [GradeCode] [varchar](10) NULL,
  [Name] [varchar](50) NULL,
  [ChargeRate] [money] NULL,
  [Class] [varchar](255) NULL,
  [Time] [float] NULL,
  [Cost] [float] NULL,
  [Division] [varchar](10) NULL,
  [JobCodeOld] [varchar](14) NULL,
  [Pay] [money] NULL,
  [NonPay] [money] NULL,
  [Overhead] [money] NULL
,    CONSTRAINT [aaaaaTimeCostCalcs_PK] PRIMARY KEY NONCLUSTERED
  (
    WorkGroup, JobCode, Project, Month, StaffID
  )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Class] ON [dbo].[TimeCostCalcs]
(
  Class
)
GO
CREATE NONCLUSTERED INDEX [Project] ON [dbo].[TimeCostCalcs]
(
  Project
)
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_CreateTimeCostCalcs (no EXEC statements).

## Dependency Check: sp_DeleteProjectMonthCasework

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonthCasework (Table)

### Location
- FPS2025/Tables/ProjectMonthCasework.sql

### Why Dependent
sp_DeleteProjectMonthCasework executes DELETE FROM ProjectMonthCasework.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonthCasework](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [int] NOT NULL,
  [CWDebit] [float] NULL,
  [CWCredit] [float] NULL
,    CONSTRAINT [PK_ProjectMonthCasework_1__10] PRIMARY KEY CLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_DeleteProjectMonthCasework (no EXEC statements).

## Dependency Check: sp_CreateProjectMonthCasework

### Result
Dependent child objects exist.

### Dependent Child Object: ProjectMonthCasework (Table)

### Location
- FPS2025/Tables/ProjectMonthCasework.sql

### Why Dependent
sp_CreateProjectMonthCasework executes INSERT INTO ProjectMonthCasework.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonthCasework](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [int] NOT NULL,
  [CWDebit] [float] NULL,
  [CWCredit] [float] NULL
,    CONSTRAINT [PK_ProjectMonthCasework_1__10] PRIMARY KEY CLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Dependent Child Object: qryProjectMonthCW (View)

### Location
- FPS2025/Views/qryProjectMonthCW.sql

### Why Dependent
sp_CreateProjectMonthCasework selects FROM qryProjectMonthCW.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryProjectMonthCW    Script Date: 3/4/00 1:48:16 PM ******/
CREATE VIEW [dbo].[qryProjectMonthCW] AS
SELECT DISTINCT ProjectMonth.Project, 
	ProjectMonth.MonthNo, 
	PlanCaseworkDebit/12 AS CWDebit, 
	TransferIncome*CaseworkSub/12 as CWCredit

FROM tlkpProject INNER JOIN ProjectMonth 
	ON tlkpProject.ParentProject = ProjectMonth.Project

GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_CreateProjectMonthCasework (no EXEC statements).

## Dependency Check: sp_DeleteProjectMonthFinal

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonthFinal (Table)

### Location
- FPS2025/Tables/ProjectMonthFinal.sql

### Why Dependent
sp_DeleteProjectMonthFinal executes DELETE FROM ProjectMonthFinal.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonthFinal](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [float] NOT NULL,
  [PeriodName] [varchar](50) NULL,
  [CumFlag] [float] NULL,
  [CostProfile] [money] NULL,
  [Subcontracts] [money] NULL,
  [Animals] [money] NULL,
  [NonAnimals] [money] NULL,
  [TimeCosts] [money] NULL,
  [TransferCosts] [money] NULL,
  [TotalCost] [money] NULL,
  [Invoices] [money] NULL,
  [COIW] [money] NULL,
  [PortSales] [money] NULL,
  [CumCost] [money] NULL,
  [CumProfile] [money] NULL,
  [SumOfCostProfile] [money] NULL,
  [CumInvoices] [money] NULL,
  [CumCOIW] [money] NULL,
  [CumPortSales] [money] NULL,
  [MstoneDue] [int] NULL,
  [Due__Done] [float] NULL,
  [OnTime] [float] NULL,
  [SumOfMstoneDue] [float] NULL,
  [SumOfDue__Done] [float] NULL,
  [SumOfOnTime] [float] NULL,
  [CWDebit] [money] NULL,
  [CWCredit] [money] NULL,
  [CumCWDebit] [money] NULL,
  [CumCWCredit] [money] NULL,
  [TotalHours] [float] NULL,
  [CumTotalHours] [float] NULL,
  [CumSubContracts] [float] NULL,
  [x] [int] NULL,
  [CumTestCosts] [float] NULL,
  [PayCosts] [float] NULL,
  [CumPayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonthFinal_PK] PRIMARY KEY NONCLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_DeleteProjectMonthFinal (no EXEC statements).

## Dependency Check: sp_deleteProjectMonth2

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonth2 (Table)

### Location
- FPS2025/Tables/ProjectMonth2.sql

### Why Dependent
sp_deleteProjectMonth2 executes DELETE FROM ProjectMonth2.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth2](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [float] NOT NULL,
  [CostProfile] [money] NULL,
  [Subcontracts] [money] NULL,
  [Animals] [money] NULL,
  [NonAnimal] [money] NULL,
  [TimeCosts] [float] NULL,
  [TransferCosts] [float] NULL,
  [TotalCost] [money] NULL,
  [Invoices] [money] NULL,
  [COIW] [money] NULL,
  [SumOfCostProfile] [money] NULL,
  [PortSales] [float] NULL,
  [MstoneDue] [int] NULL,
  [Due__Done] [float] NULL,
  [OnTime] [float] NULL,
  [TotalHours] [float] NULL,
  [PayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonth2_PK] PRIMARY KEY NONCLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_deleteProjectMonth2 (no EXEC statements).

## Dependency Check: sp_qryJobMonth_Single

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonth2 (Table)

### Location
- FPS2025/Tables/ProjectMonth2.sql

### Why Dependent
sp_qryJobMonth_Single executes INSERT INTO ProjectMonth2.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth2](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [float] NOT NULL,
  [CostProfile] [money] NULL,
  [Subcontracts] [money] NULL,
  [Animals] [money] NULL,
  [NonAnimal] [money] NULL,
  [TimeCosts] [float] NULL,
  [TransferCosts] [float] NULL,
  [TotalCost] [money] NULL,
  [Invoices] [money] NULL,
  [COIW] [money] NULL,
  [SumOfCostProfile] [money] NULL,
  [PortSales] [float] NULL,
  [MstoneDue] [int] NULL,
  [Due__Done] [float] NULL,
  [OnTime] [float] NULL,
  [TotalHours] [float] NULL,
  [PayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonth2_PK] PRIMARY KEY NONCLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Dependent Child Object: qryJobMonth_SubContracts (View)

### Location
- FPS2025/Views/qryJobMonth_SubContracts.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonth_SubContracts via LEFT JOIN on Project and Month.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryJobMonth_SubContracts] as 
SELECT Project, Month, Sum(Animals1) AS Animals, Sum(Other1) AS Other, 
Sum(Animals1) + Sum(Other1) AS Total
FROM qryJobMonth_SubContracts1
GROUP BY Project, Month

GO
```

### Dependent Child Object: qryJobMonth_Time (View)

### Location
- FPS2025/Views/qryJobMonth_Time.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonth_Time via LEFT JOIN on Project and Month.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_Time    Script Date: 3/4/00 1:48:15 PM ******/
CREATE VIEW [dbo].[qryJobMonth_Time]
AS
SELECT DISTINCT Project, Month, SUM(Cost) AS SumOfCost, SUM(Time) AS SumOfHours, SUM(Pay) AS SumOfPayRate
FROM         dbo.TimeCostCalcs
GROUP BY Project, Month

GO
```

### Dependent Child Object: qryJobMonthMilestone (View)

### Location
- FPS2025/Views/qryJobMonthMilestone.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonthMilestone via LEFT JOIN on Project and DueMonth/MonthNo.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[qryJobMonthMilestone]
AS
SELECT DISTINCT 
    Project, DueMonth, COUNT(MilestoneRef) AS MstoneDue, 
    SUM(CompleteFlag) AS Due__Done, SUM(OnTimeFlag) 
    AS OnTime
FROM qryMilestone1
GROUP BY Project, DueMonth

GO
```

### Dependent Child Object: qryJobMonth_TransfersTotal (View)

### Location
- FPS2025/Views/qryJobMonth_TransfersTotal.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonth_TransfersTotal via LEFT JOIN on Project and Month.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_TransfersTotal    Script Date: 3/4/00 1:48:20 PM ******/
CREATE VIEW [dbo].[qryJobMonth_TransfersTotal] AS
SELECT DISTINCT Project, Month, Sum(TransferCost) AS SumOfTransferCost
FROM qryJobMonth_TransferUnion
GROUP BY Project, Month

GO
```

### Dependent Child Object: qryJobMonth_Invoices (View)

### Location
- FPS2025/Views/qryJobMonth_Invoices.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonth_Invoices via LEFT JOIN on Month and ProjectParent.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_Invoices    Script Date: 3/4/00 1:48:16 PM ******/
CREATE VIEW [dbo].[qryJobMonth_Invoices] as
SELECT Proj_Invoice.ProjectParent, Proj_Invoice.Month, 
Sum(Proj_Invoice.Amount) AS SumOfAmount1, Sum(Proj_Invoice.CostOfWork) AS WorkCost
FROM Proj_Invoice
GROUP BY Proj_Invoice.ProjectParent, Proj_Invoice.Month

GO
```

### Dependent Child Object: qryJobMonthPortfolioSales (View)

### Location
- FPS2025/Views/qryJobMonthPortfolioSales.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonthPortfolioSales via LEFT JOIN on Month and PlanPortfolio.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonthPortfolioSales    Script Date: 3/4/00 1:48:19 PM ******/
CREATE VIEW [dbo].[qryJobMonthPortfolioSales] as
SELECT DISTINCT tlkpTestCapability.PlanPortfolio, MonthlyOutput.Month, 
Sum(unitprice * volume) AS Fee
FROM tlkpTestReqmt INNER JOIN (tlkpTestCapability INNER JOIN MonthlyOutput ON 
(tlkpTestCapability.WorkGroup = MonthlyOutput.WorkGroup) AND 
(tlkpTestCapability.TestCode = MonthlyOutput.TestCode)) ON 
(tlkpTestReqmt.Buyer = MonthlyOutput.Buyer) AND 
(tlkpTestReqmt.TestCode = MonthlyOutput.TestCode)
GROUP BY tlkpTestCapability.PlanPortfolio, MonthlyOutput.Month

GO
```

### Dependent Child Object: qryJobMonth_TotProfile (View)

### Location
- FPS2025/Views/qryJobMonth_TotProfile.sql

### Why Dependent
sp_qryJobMonth_Single selects FROM qryJobMonth_TotProfile via LEFT JOIN on Project.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  View dbo.qryJobMonth_TotProfile    Script Date: 3/4/00 1:48:15 PM ******/
CREATE VIEW [dbo].[qryJobMonth_TotProfile] as
SELECT DISTINCT ProjectMonth.Project, Sum(ProjectMonth.CostProfile) AS SumOfCostProfile
FROM ProjectMonth
GROUP BY ProjectMonth.Project

GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_qryJobMonth_Single (no EXEC statements).

## Dependency Check: sp_DeleteProjectMonth3

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonth3 (Table)

### Location
- FPS2025/Tables/ProjectMonth3.sql

### Why Dependent
sp_DeleteProjectMonth3 executes DELETE FROM ProjectMonth3.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth3](
  [EndPeriod] [float] NOT NULL,
  [PeriodName] [varchar](50) NULL,
  [Project] [varchar](20) NOT NULL,
  [CumCost] [money] NULL,
  [CumInvoices] [money] NULL,
  [CumCOIW] [money] NULL,
  [CumPortSales] [float] NULL,
  [CumProfile] [money] NULL,
  [SumOfCostProfile] [money] NULL,
  [SumOfMstoneDue] [float] NULL,
  [SumOfDue__Done] [float] NULL,
  [SumOfOnTime] [float] NULL,
  [CumCWDebit] [money] NULL,
  [CumCWCredit] [money] NULL,
  [CumTotalHours] [float] NULL,
  [CumSubContracts] [float] NULL,
  [CumTestCosts] [float] NULL,
  [CumPayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonth3_PK] PRIMARY KEY NONCLUSTERED
  (
    EndPeriod, Project
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_DeleteProjectMonth3 (no EXEC statements).

## Dependency Check: sp_qryJobMonthCum

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonth3 (Table)

### Location
- FPS2025/Tables/ProjectMonth3.sql

### Why Dependent
sp_qryJobMonthCum executes INSERT INTO ProjectMonth3.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonth3](
  [EndPeriod] [float] NOT NULL,
  [PeriodName] [varchar](50) NULL,
  [Project] [varchar](20) NOT NULL,
  [CumCost] [money] NULL,
  [CumInvoices] [money] NULL,
  [CumCOIW] [money] NULL,
  [CumPortSales] [float] NULL,
  [CumProfile] [money] NULL,
  [SumOfCostProfile] [money] NULL,
  [SumOfMstoneDue] [float] NULL,
  [SumOfDue__Done] [float] NULL,
  [SumOfOnTime] [float] NULL,
  [CumCWDebit] [money] NULL,
  [CumCWCredit] [money] NULL,
  [CumTotalHours] [float] NULL,
  [CumSubContracts] [float] NULL,
  [CumTestCosts] [float] NULL,
  [CumPayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonth3_PK] PRIMARY KEY NONCLUSTERED
  (
    EndPeriod, Project
  )
) ON [PRIMARY]
GO
```

### Dependent Child Object: tblPeriod (Table)

### Location
- FPS2025/Tables/tblPeriod.sql

### Why Dependent
sp_qryJobMonthCum selects FROM tblPeriod INNER JOIN tblkPeriodMonth to drive the period structure for cumulative grouping.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPeriod](
    [PeriodName] [varchar](50) NOT NULL,
    [PeriodType] [varchar](50) NULL,
    [StartPeriod] [float] NULL,
    [EndPeriod] [float] NULL,
    [FinalSummariesRun] [smallint] NULL,
    [PeriodLocked] [smallint] NOT NULL CONSTRAINT [DF_tblPeriod_PeriodLocked] DEFAULT ((0))
,    CONSTRAINT [aaaaatblPeriod_PK] PRIMARY KEY NONCLUSTERED
    (
        PeriodName
    )
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [EndPeriod] ON [dbo].[tblPeriod]
(
    EndPeriod
)
GO
```

### Dependent Child Object: tblkPeriodMonth (View)

### Location
- FPS2025/Views/tblkPeriodMonth.sql

### Why Dependent
sp_qryJobMonthCum joins tblPeriod INNER JOIN tblkPeriodMonth ON tblPeriod.PeriodName = tblkPeriodMonth.PeriodName to map months to periods.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[tblkPeriodMonth]
AS
SELECT     dbo.tblPeriodMonth.EndMonth, dbo.tblPeriodMonth.MonthNo, dbo.tblPeriod.PeriodName
FROM         dbo.tblPeriod INNER JOIN
                      dbo.tblPeriodMonth ON dbo.tblPeriod.EndPeriod = dbo.tblPeriodMonth.EndMonth

GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_qryJobMonthCum (no EXEC statements).

## Dependency Check: sp_qryJobMonth_Final

### Result
Dependent child object exists.

### Dependent Child Object: ProjectMonthFinal (Table)

### Location
- FPS2025/Tables/ProjectMonthFinal.sql

### Why Dependent
sp_qryJobMonth_Final executes INSERT INTO ProjectMonthFinal.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProjectMonthFinal](
  [Project] [varchar](20) NOT NULL,
  [MonthNo] [float] NOT NULL,
  [PeriodName] [varchar](50) NULL,
  [CumFlag] [float] NULL,
  [CostProfile] [money] NULL,
  [Subcontracts] [money] NULL,
  [Animals] [money] NULL,
  [NonAnimals] [money] NULL,
  [TimeCosts] [money] NULL,
  [TransferCosts] [money] NULL,
  [TotalCost] [money] NULL,
  [Invoices] [money] NULL,
  [COIW] [money] NULL,
  [PortSales] [money] NULL,
  [CumCost] [money] NULL,
  [CumProfile] [money] NULL,
  [SumOfCostProfile] [money] NULL,
  [CumInvoices] [money] NULL,
  [CumCOIW] [money] NULL,
  [CumPortSales] [money] NULL,
  [MstoneDue] [int] NULL,
  [Due__Done] [float] NULL,
  [OnTime] [float] NULL,
  [SumOfMstoneDue] [float] NULL,
  [SumOfDue__Done] [float] NULL,
  [SumOfOnTime] [float] NULL,
  [CWDebit] [money] NULL,
  [CWCredit] [money] NULL,
  [CumCWDebit] [money] NULL,
  [CumCWCredit] [money] NULL,
  [TotalHours] [float] NULL,
  [CumTotalHours] [float] NULL,
  [CumSubContracts] [float] NULL,
  [x] [int] NULL,
  [CumTestCosts] [float] NULL,
  [PayCosts] [float] NULL,
  [CumPayCosts] [float] NULL
,    CONSTRAINT [aaaaaProjectMonthFinal_PK] PRIMARY KEY NONCLUSTERED
  (
    Project, MonthNo
  )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside sp_qryJobMonth_Final (no EXEC statements).

## Dependency Check: usp_LogRecreateSummaries

### Result
Dependent child objects exist.

### Dependent Child Object: RecreateSummaries_Log (Table)

### Location
- FPS2025/Tables/RecreateSummaries_Log.sql

### Why Dependent
usp_LogRecreateSummaries executes INSERT INTO RecreateSummaries_Log.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RecreateSummaries_Log](
  [ID] [int] IDENTITY(1,1) NOT NULL,
  [UserID] [varchar](20) NULL,
  [Period] [smallint] NULL,
  [DateDone] [datetime] NULL
,    CONSTRAINT [PK_RecreateSummaries_Log] PRIMARY KEY CLUSTERED
  (
    ID
  )
) ON [PRIMARY]
GO
```

### Dependent Child Object: sp_Get_SP_No (Stored Procedure)

### Result
Dependent child object provided.

### Location
- Z:/Lot2/DB/sp_Get_SP_NO.sql

### Dependency Detail
- Reference found in usp_LogRecreateSummaries: EXEC [dbo].[sp_Get_SP_No]
- SQL definition provided externally.

### SQL Code Extract (Provided)
```sql
CREATE PROCEDURE [dbo].[sp_Get_SP_No]   @Mno  varchar(20)
OUTPUT AS
SELECT @MNo = SUBSTRING(SYSTEM_USER, CHARINDEX('\\', SYSTEM_USER) + 1, 20)
GO
```

### Additional Dependency Check
- No other child procedure dependency exists inside usp_LogRecreateSummaries.

---

## Conditional Refresh Procedures (Executed only when @periodLocked = 0)

---

## Conditional Child Procedure: usp_Refresh_Period_MO

### Location
- FPS2025/Procedures/usp_Refresh_Period_MO.sql

### Purpose
Deletes existing rows for the period from Period_MonthlyOutput then repopulates it from MonthlyOutput joined to project, cost centre, workgroup, and test requirement data.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_MO]
	@period int
as
delete from [dbo].[Period_MonthlyOutput]
where period=@period
INSERT INTO [dbo].[Period_MonthlyOutput]
(	 [Period]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[IsDefraProject]
      ,[OPC]
      ,[OCC]
      ,[Month]
      ,[SPC]
      ,[WorkGroup]
      ,[SCC]
      ,[TestCode]
      ,[Volume]
      ,[TestPrice]
      ,[TotalCost]
)
SELECT  @Period,
	tlkpProject.ParentProject AS Project, 
	tlkpProject.OracleProjectCode, 
	tlkpProject.SubAccountCode, 
	case tlkpProject.IsDefraProject when 0 then 'No' else 'Yes' end IsDefraProject, 
	CostCentre.ProfitCentre AS OPC, 
	CostCentre.CostCentre AS OCC, 
	MonthlyOutput.Month, 
	WorkGroup.ProfitCentre AS SPC, 
	WorkGroup.WorkGroup, 
	WorkGroup.CostCentre AS SCC,  
	MonthlyOutput.TestCode, 
	MonthlyOutput.Volume, 
	tlkpTestReqmt.UnitPrice as TestPrice, 
	convert(money,[UnitPrice]*[Volume]) AS TotalCost

FROM ((tlkpProject LEFT JOIN CostCentre 
	ON tlkpProject.CostCentre = CostCentre.CostCentre) 
	INNER JOIN (MonthlyOutput 
	INNER JOIN WorkGroup ON MonthlyOutput.WorkGroup = WorkGroup.WorkGroup) 
	ON tlkpProject.ParentProject = MonthlyOutput.Buyer) 
	INNER JOIN tlkpTestReqmt 
	ON (MonthlyOutput.Buyer = tlkpTestReqmt.projectBuyerCode) 
	AND (MonthlyOutput.TestCode = tlkpTestReqmt.TestCode)

GO
```

## Dependency Check: usp_Refresh_Period_MO

### Result
Dependent child object exists.

### Dependent Child Object: Period_MonthlyOutput (Table)

### Location
- FPS2025/Tables/Period_MonthlyOutput.sql

### Why Dependent
usp_Refresh_Period_MO executes DELETE FROM Period_MonthlyOutput and INSERT INTO Period_MonthlyOutput.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Period_MonthlyOutput](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Period] [int] NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [IsDefraProject] [varchar](3) NOT NULL,
    [OPC] [varchar](50) NULL,
    [OCC] [float] NULL,
    [Month] [float] NOT NULL,
    [SPC] [varchar](50) NOT NULL,
    [WorkGroup] [varchar](50) NOT NULL,
    [SCC] [float] NULL,
    [TestCode] [varchar](20) NOT NULL,
    [Volume] [float] NULL,
    [TestPrice] [money] NULL,
    [TotalCost] [money] NULL
,    CONSTRAINT [PK_Period_MonthlyOutput_1] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside usp_Refresh_Period_MO (no EXEC statements).

---

## Conditional Child Procedure: usp_Refresh_Period_psc

### Location
- FPS2025/Procedures/usp_Refresh_Period_PSC.sql

### Purpose
Deletes existing rows for the period from Period_Proj_SubContract then repopulates it from Proj_SubContract joined to project and cost centre data.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_PSC]
	@period int
as
delete from [dbo].Period_Proj_SubContract
where period=@period
INSERT INTO [dbo].[Period_Proj_SubContract]
		([Period]
      ,[SubContCounter]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[IsDefraProject]
      ,[OPC]
      ,[OCC]
      ,[Month]
      ,[Amount]
      ,[AcctCode])
Select @period,
     dbo.Proj_SubContract.SubContCounter, 
	dbo.Proj_SubContract.Project, 
	dbo.tlkpProject.OracleProjectCode, 
    dbo.tlkpProject.SubAccountCode, 
	CASE tlkpProject.IsDefraProject WHEN 0 THEN 'No' ELSE 'Yes' END AS IsDefraProject, 
	dbo.CostCentre.ProfitCentre AS OPC, 
    dbo.CostCentre.CostCentre AS OCC, 
	dbo.Proj_SubContract.Month, 
	dbo.Proj_SubContract.Amount, 
	dbo.Proj_SubContract.AcctCode

FROM         dbo.CostCentre RIGHT OUTER JOIN
                      dbo.tlkpProject ON dbo.CostCentre.CostCentre = dbo.tlkpProject.CostCentre INNER JOIN
                      dbo.Proj_SubContract ON dbo.tlkpProject.ParentProject = dbo.Proj_SubContract.Project

GO
```

## Dependency Check: usp_Refresh_Period_psc

### Result
Dependent child object exists.

### Dependent Child Object: Period_Proj_Subcontract (Table)

### Location
- FPS2025/Tables/Period_Proj_Subcontract.sql

### Why Dependent
usp_Refresh_Period_PSC executes DELETE FROM Period_Proj_SubContract and INSERT INTO Period_Proj_SubContract.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Period_Proj_Subcontract](
    [Period] [tinyint] NOT NULL,
    [SubContCounter] [int] NOT NULL,
    [Project] [varchar](20) NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [IsDefraProject] [varchar](3) NOT NULL,
    [OPC] [varchar](50) NULL,
    [OCC] [float] NULL,
    [Month] [float] NULL,
    [Amount] [money] NULL,
    [AcctCode] [varchar](30) NULL
,    CONSTRAINT [PK_Period_Proj_Subcontract] PRIMARY KEY CLUSTERED
    (
        Period, SubContCounter
    )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside usp_Refresh_Period_PSC (no EXEC statements).

---

## Conditional Child Procedure: usp_Refresh_Period_tcc

### Location
- FPS2025/Procedures/usp_Refresh_Period_TCC.sql

### Purpose
Deletes existing rows for the period from Period_TimeCostCalcs then repopulates it from TimeCostCalcs joined to project, cost centre, workgroup, and employee data.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[usp_Refresh_Period_TCC]
	@period int
as
delete from [dbo].[Period_TimeCostCalcs]
where period=@period

INSERT INTO .[dbo].[Period_TimeCostCalcs]
(
      [Period]
      ,[Project]
      ,[OracleProjectCode]
      ,[SubAccountCode]
      ,[Month]
      ,[DefraProject]
      ,[OCC]
      ,[OPC]
      ,[SPC]
      ,[SCC]
      ,[Name]
      ,[GradeCode]
      ,[SPNumber]
      ,[ChargeRate]
      ,[Pay]
      ,[Nonpay]
      ,[Overhead]
      ,[Time]
      ,[TotalCost])

SELECT @Period, 
	tlkpProject.ParentProject AS Project,	
	tlkpProject.OracleProjectCode, 
	tlkpProject.SubAccountCode, 
	TimeCostCalcs.Month, 
	case tlkpProject.IsDefraProject when 0 then 'No' else 'Yes' end AS DefraProject, 
	CostCentre.CostCentre AS OCC, 
	CostCentre.ProfitCentre AS OPC, 
	WorkGroup.ProfitCentre AS SPC, 
	WorkGroup.CostCentre AS SCC, 
	TimeCostCalcs.Name, 
	TimeCostCalcs.GradeCode, 
	tblWGEmployee.SPNumber, 
	TimeCostCalcs.ChargeRate, 
	TimeCostCalcs.Pay, 
	TimeCostCalcs.Nonpay, 
	TimeCostCalcs.Overhead, 
	TimeCostCalcs.Time, 
	TimeCostCalcs.Cost AS TotalCost
FROM dbo.tblWGEmployee INNER JOIN ((tlkpProject LEFT JOIN CostCentre ON tlkpProject.CostCentre = CostCentre.CostCentre) INNER JOIN (TimeCostCalcs INNER JOIN WorkGroup ON TimeCostCalcs.WorkGroup = WorkGroup.WorkGroup) ON tlkpProject.ParentProject = TimeCostCalcs.Project) ON tblWGEmployee.PACTid = TimeCostCalcs.StaffID

GO
```

## Dependency Check: usp_Refresh_Period_tcc

### Result
Dependent child object exists.

### Dependent Child Object: Period_TimeCostCalcs (Table)

### Location
- FPS2025/Tables/Period_TimeCostCalcs.sql

### Why Dependent
usp_Refresh_Period_TCC executes DELETE FROM Period_TimeCostCalcs and INSERT INTO Period_TimeCostCalcs.

### SQL Code Extract (As-Is)
```sql
USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Period_TimeCostCalcs](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Period] [int] NOT NULL,
    [Project] [varchar](20) NOT NULL,
    [OracleProjectCode] [varchar](50) NULL,
    [SubAccountCode] [varchar](50) NULL,
    [Month] [float] NOT NULL,
    [DefraProject] [varchar](3) NOT NULL,
    [OCC] [float] NULL,
    [OPC] [varchar](50) NULL,
    [SPC] [varchar](50) NOT NULL,
    [SCC] [float] NULL,
    [Name] [varchar](50) NULL,
    [GradeCode] [varchar](10) NULL,
    [SPNumber] [varchar](10) NOT NULL,
    [ChargeRate] [money] NULL,
    [Pay] [money] NULL,
    [Nonpay] [money] NULL,
    [Overhead] [money] NULL,
    [Time] [float] NULL,
    [TotalCost] [money] NULL
,    CONSTRAINT [PK_Period_TimeCostCalcs_1] PRIMARY KEY CLUSTERED
    (
        ID
    )
) ON [PRIMARY]
GO
```

### Additional Dependency Check
- No child procedure dependency exists inside usp_Refresh_Period_TCC (no EXEC statements).

