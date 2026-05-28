USE [FPS2025]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TestOrProduct](
    [ItemCode] [varchar](20) NOT NULL,
    [ItemDescription] [varchar](200) NULL,
    [TestManager] [varchar](50) NULL,
    [JobStatus] [varchar](2) NULL,
    [UnitPriceVLA] [money] NULL CONSTRAINT [DF__TestOrPro__UnitP__4786A88A] DEFAULT (0),
    [PriceAHVG] [money] NULL,
    [Owner] [varchar](2) NULL,
    [ChargeMethod] [varchar](5) NULL,
    [ShortDescription] [char](18) NULL,
    [DefraUnitPrice] [money] NOT NULL CONSTRAINT [DF__TestOrPro__Defra__335592AB] DEFAULT ((0))
,    CONSTRAINT [PK__TestOrProduct__487ACCC3] PRIMARY KEY CLUSTERED
    (
        ItemCode
    )
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[TestOrProduct] WITH CHECK ADD CONSTRAINT [Owner_Cannot_Be_Null] CHECK (NOT [owner] IS NULL AND ([owner]='PT' OR ([owner]='PA' OR ([owner]='SD' OR [owner]='LT'))))
GO
