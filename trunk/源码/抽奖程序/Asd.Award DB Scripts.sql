USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpOption]    Script Date: 01/19/2012 10:41:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TmpOption]') AND type in (N'U'))
DROP TABLE [dbo].[TmpOption]
GO

USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpOption]    Script Date: 01/19/2012 10:41:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TmpOption](
	[BelongTo] [varchar](2) NOT NULL,
	[Level] [char](1) NOT NULL,
	[HitSequence] [int] NOT NULL,
	[AwardQty] [int] NOT NULL,
	[Remark] [nvarchar](50) NULL,
 CONSTRAINT [PK_TmpOption] PRIMARY KEY CLUSTERED 
(
	[BelongTo] ASC,
	[Level] ASC,
	[HitSequence] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpTicket]    Script Date: 01/19/2012 10:42:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TmpTicket]') AND type in (N'U'))
DROP TABLE [dbo].[TmpTicket]
GO

USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpTicket]    Script Date: 01/19/2012 10:42:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TmpTicket](
	[TicketNO] [nvarchar](50) NOT NULL,
	TicketCount [nvarchar](50) NULL,
 CONSTRAINT [PK_TmpTicket] PRIMARY KEY CLUSTERED 
(
	[TicketNO] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpSetting]    Script Date: 01/19/2012 10:42:03 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TmpSetting]') AND type in (N'U'))
DROP TABLE [dbo].[TmpSetting]
GO

USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpSetting]    Script Date: 01/19/2012 10:42:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TmpSetting](
	[SettingName] [nvarchar](50) NOT NULL,
	[SettingValue] [nvarchar](50) NULL,
	[Remark] [nvarchar](50) NULL,
 CONSTRAINT [PK_TempSetting] PRIMARY KEY CLUSTERED 
(
	[SettingName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpAward]    Script Date: 01/19/2012 10:41:22 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TmpAward]') AND type in (N'U'))
DROP TABLE [dbo].[TmpAward]
GO

USE [AsdLync]
GO

/****** Object:  Table [dbo].[TmpAward]    Script Date: 01/19/2012 10:41:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TmpAward](
	[Level] [nvarchar](50) NULL,
	[TicketNO] [nvarchar](50) NOT NULL,
	[Remark] [nvarchar](50) NULL,
	[UpdateTime] [datetime] NULL,
 CONSTRAINT [PK_TmpAward] PRIMARY KEY CLUSTERED 
(
	[TicketNO] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



insert into [111].[dbo].[TmpTicket] values ('±±æ©','1000');
insert into [111].[dbo].[TmpTicket] values ('…Ó€⁄','1000');
