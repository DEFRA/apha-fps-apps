USE [MAB_Archive_CM]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MY_TestOrProduct](
    [Year] [smallint] NOT NULL,
    [ItemCode] [varchar](20) NOT NULL,
    [ItemDescription] [varchar](200) NULL,
    [TestManager] [varchar](50) NULL,
    [JobStatus] [varchar](2) NULL,
    [UnitPriceVLA] [money] NULL,
    [PriceAHVG] [money] NULL,
    [Owner] [varchar](2) NULL,
    [ChargeMethod] [varchar](5) NULL,
    [ShortDescription] [char](18) NULL,
    [DefraUnitPrice] [money] NULL
,    CONSTRAINT [PK_MY_TestOrProduct] PRIMARY KEY CLUSTERED
    (
        Year, ItemCode
    )
) ON [PRIMARY]
GO
