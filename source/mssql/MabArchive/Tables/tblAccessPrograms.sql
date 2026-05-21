USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblAccessPrograms](
    [SystemID] [int] NOT NULL,
    [NTLogin] [varchar](50) NOT NULL,
    [Program] [varchar](10) NOT NULL
,    CONSTRAINT [PK_tblAccessPrograms] PRIMARY KEY CLUSTERED
    (
        SystemID, NTLogin, Program
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblAccessPrograms] WITH CHECK ADD CONSTRAINT [FK_tblAccessPrograms_tblAccessUsers] FOREIGN KEY(SystemID, NTLogin)
REFERENCES [dbo].[tblAccessUsers] (SystemID, NTLogin)
GO
ALTER TABLE [dbo].[tblAccessPrograms] CHECK CONSTRAINT [FK_tblAccessPrograms_tblAccessUsers]
GO
ALTER TABLE [dbo].[tblAccessPrograms] WITH CHECK ADD CONSTRAINT [FK_tblAccessPrograms_tblRadTrackProg] FOREIGN KEY(Program)
REFERENCES [dbo].[tblRadTrackProg] (Program)
GO
ALTER TABLE [dbo].[tblAccessPrograms] CHECK CONSTRAINT [FK_tblAccessPrograms_tblRadTrackProg]
GO
