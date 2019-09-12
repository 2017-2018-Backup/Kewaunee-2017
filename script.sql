USE [Kewaunee]
GO
/****** Object:  Table [dbo].[Projects]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Projects](
	[ProjectId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProjectName] [nvarchar](max) NOT NULL,
	[ProjectNo] [nvarchar](max) NOT NULL,
	[CustomerNameAddress] [nvarchar](max) NOT NULL,
	[BuildingName] [nvarchar](max) NOT NULL,
	[FloorNo] [bigint] NOT NULL,
	[RoomNo] [bigint] NOT NULL,
	[LabName] [nvarchar](max) NOT NULL,
	[DrawingRevNo] [nvarchar](max) NOT NULL,
	[LabLength] [bigint] NULL,
	[LabWidth] [bigint] NULL,
	[TrueCeilingHeight] [bigint] NULL,
	[FalseCeilingHeight] [bigint] NULL,
	[ProjectPath] [nvarchar](max) NULL,
	[isOpenProject] [bit] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED 
(
	[ProjectId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Profile]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Profile](
	[ProfileId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProfileDescription] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Profile] PRIMARY KEY CLUSTERED 
(
	[ProfileId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Features]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Features](
	[FeatureId] [bigint] IDENTITY(1,1) NOT NULL,
	[Feature] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_Features] PRIMARY KEY CLUSTERED 
(
	[FeatureId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customer]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Customer](
	[Code] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Address] [nvarchar](max) NOT NULL,
	[City] [nvarchar](max) NOT NULL,
	[State] [nvarchar](max) NOT NULL,
	[Country] [nvarchar](max) NOT NULL,
	[PhoneNo] [int] NOT NULL,
	[MobileNo] [nvarchar](max) NOT NULL,
	[FaxNo] [nvarchar](max) NOT NULL,
	[PrimaryContact] [nvarchar](max) NOT NULL,
	[Designation] [nvarchar](max) NOT NULL,
	[Logo] [varbinary](max) NULL,
	[TinNo] [int] NOT NULL,
	[CSTNo] [int] NOT NULL,
	[VATNo] [int] NOT NULL,
	[ServiceTaxRegistrationNo] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[TaxMaster]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TaxMaster](
	[TaxId] [bigint] IDENTITY(1,1) NOT NULL,
	[TaxDescription] [nvarchar](max) NOT NULL,
	[TaxValue] [decimal](18, 2) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDepreciated] [bit] NULL,
	[IsDeleted] [bit] NOT NULL,
	[DepreciatedDate] [datetime] NULL,
 CONSTRAINT [PK_TaxMaster] PRIMARY KEY CLUSTERED 
(
	[TaxId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SavedProfiles]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SavedProfiles](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ProfileId] [bigint] NOT NULL,
	[FeatureId] [bigint] NOT NULL,
	[Read] [bit] NOT NULL,
	[Edit] [bit] NOT NULL,
	[Delete] [bit] NOT NULL,
 CONSTRAINT [PK_SavedProfiles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LoginCreation]    Script Date: 01/09/2017 07:36:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoginCreation](
	[UserID] [bigint] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](max) NOT NULL,
	[Designation] [nvarchar](max) NOT NULL,
	[Department] [nvarchar](max) NOT NULL,
	[ProfileId] [bigint] NOT NULL,
	[Password] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NULL,
	[DeletedDate] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_LoginCreation] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  ForeignKey [FK_LoginCreation_Profile]    Script Date: 01/09/2017 07:36:41 ******/
ALTER TABLE [dbo].[LoginCreation]  WITH CHECK ADD  CONSTRAINT [FK_LoginCreation_Profile] FOREIGN KEY([ProfileId])
REFERENCES [dbo].[Profile] ([ProfileId])
GO
ALTER TABLE [dbo].[LoginCreation] CHECK CONSTRAINT [FK_LoginCreation_Profile]
GO
/****** Object:  ForeignKey [FK_SavedProfiles_Features]    Script Date: 01/09/2017 07:36:41 ******/
ALTER TABLE [dbo].[SavedProfiles]  WITH CHECK ADD  CONSTRAINT [FK_SavedProfiles_Features] FOREIGN KEY([FeatureId])
REFERENCES [dbo].[Features] ([FeatureId])
GO
ALTER TABLE [dbo].[SavedProfiles] CHECK CONSTRAINT [FK_SavedProfiles_Features]
GO
/****** Object:  ForeignKey [FK_SavedProfiles_Profile]    Script Date: 01/09/2017 07:36:41 ******/
ALTER TABLE [dbo].[SavedProfiles]  WITH CHECK ADD  CONSTRAINT [FK_SavedProfiles_Profile] FOREIGN KEY([ProfileId])
REFERENCES [dbo].[Profile] ([ProfileId])
GO
ALTER TABLE [dbo].[SavedProfiles] CHECK CONSTRAINT [FK_SavedProfiles_Profile]
GO
