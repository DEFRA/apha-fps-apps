USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAdditionalCosts](
    [AC_Identity] [int] IDENTITY(1,1) NOT NULL,
    [Project] [varchar](50) NULL,
    [Year] [int] NULL CONSTRAINT [DF__TemporaryU__Year__300424B4] DEFAULT ((0)),
    [AccountCat] [nvarchar](50) NOT NULL,
    [Description] [nvarchar](100) NOT NULL,
    [ItemCost] [float] NULL CONSTRAINT [DF__Temporary__ItemC__31EC6D26] DEFAULT ((0)),
    [CostEntered] [float] NOT NULL CONSTRAINT [DF__Temporary__CostE__32E0915F] DEFAULT ((0)),
    [Freq] [nvarchar](5) NULL
,    CONSTRAINT [aaaaatblAdditionalCosts_PK] PRIMARY KEY NONCLUSTERED
    (
        AC_Identity
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAdditionalCosts] WITH CHECK ADD CONSTRAINT [FK_tblAdditionalCosts_tblProjectYear] FOREIGN KEY(Year, Project)
REFERENCES [dbo].[tblProjectYear] (YearNo, Project)
GO
ALTER TABLE [dbo].[tblAdditionalCosts] CHECK CONSTRAINT [FK_tblAdditionalCosts_tblProjectYear]
GO
CREATE NONCLUSTERED INDEX [tblAdditionalCostsProject] ON [dbo].[tblAdditionalCosts]
(
    Project
)
GO
CREATE NONCLUSTERED INDEX [tblProjectYeartblAdditionalCosts] ON [dbo].[tblAdditionalCosts]
(
    Project, Year
)
GO
