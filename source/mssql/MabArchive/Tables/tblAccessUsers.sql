USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAccessUsers](
    [SystemID] [int] NOT NULL,
    [NTLogin] [varchar](50) NOT NULL,
    [UserName] [varchar](50) NULL,
    [DT2Login] [varchar](50) NULL
,    CONSTRAINT [PK_tblAccessUsers] PRIMARY KEY NONCLUSTERED
    (
        SystemID, NTLogin
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAccessUsers] WITH CHECK ADD CONSTRAINT [FK_tblAccessUsers_tblAccessSystems] FOREIGN KEY(SystemID)
REFERENCES [dbo].[tblAccessSystems] (SystemID)
GO
ALTER TABLE [dbo].[tblAccessUsers] CHECK CONSTRAINT [FK_tblAccessUsers_tblAccessSystems]
GO
