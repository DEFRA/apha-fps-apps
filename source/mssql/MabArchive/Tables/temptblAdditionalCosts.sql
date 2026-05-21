USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[temptblAdditionalCosts](
    [AC_Identity] [int] IDENTITY(1,1) NOT NULL,
    [Project] [int] NULL CONSTRAINT [DF__Temporary__Proje__5EBF139D] DEFAULT (0),
    [Year] [int] NULL CONSTRAINT [DF__TemporaryU__Year__5FB337D6] DEFAULT (0),
    [AccountCat] [nvarchar](50) NULL,
    [Description] [nvarchar](20) NULL,
    [ItemCost] [float] NULL CONSTRAINT [DF__Temporary__ItemC__619B8048] DEFAULT (0),
    [CostEntered] [float] NULL CONSTRAINT [DF__Temporary__CostE__628FA481] DEFAULT (0),
    [Freq] [nvarchar](5) NULL
,    CONSTRAINT [aaaaatemptblAdditionalCosts_PK] PRIMARY KEY NONCLUSTERED
    (
        AC_Identity
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[temptblAdditionalCosts] WITH CHECK ADD CONSTRAINT [temptblAdditionalCosts_FK00] FOREIGN KEY(Project, Year)
REFERENCES [dbo].[temptblProjectYear] (Project, YearNo)
GO
ALTER TABLE [dbo].[temptblAdditionalCosts] CHECK CONSTRAINT [temptblAdditionalCosts_FK00]
GO
CREATE NONCLUSTERED INDEX [tblAdditionalCostsProject] ON [dbo].[temptblAdditionalCosts]
(
    Project
)
GO
CREATE NONCLUSTERED INDEX [temptblProjectYeartemptblAdditionalCosts] ON [dbo].[temptblAdditionalCosts]
(
    Project, Year
)
GO
