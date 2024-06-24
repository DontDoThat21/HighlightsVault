Highlights Vault is a .NET Core MVC application that stores descriptions, names, dates, a picture, and a "highlight" video clip per row in an organized manner through an intuitive user interface..
The application has two "modes" powered by simple Password entry (SQL table entry with a password. Should consider changing this ESPECIALLY if/when not using this locally.)

Features
* User Profiles: Each user can create a profile via manual entry, or optionally automatically created and linked to their Steam account, providing automation of details with Steam's API.
* Grudge Entries: Users can log Highlights pertaining to a specific user/friend, including details such as associated video clips, user descriptions, and their Steam ID if desired.
* Edit and Delete: Ability to edit and delete Highlight entries within a specific time frame after initial creation.
* Search and Filter: Search functionality to find specific Highlights based on user name, Steam ID, Highlight description, and Highlight date.
* Responsive Design: Designed to work seamlessly across different devices and screen sizes.

![image](https://github.com/DontDoThat21/HighlightsVault/assets/46426868/0dc85695-222c-4258-82dc-8885cb2f5682)

Supports Steam Api adding and automatically saving (profile picture to the database directly as bytes/blob object) Steam User's datas (name, picture) to a SQL Server DB.
https://steamcommunity.com/dev/apikey

Designed to use Entity Framework with a SQL Server.
The DDL for the tables can all be found below!
The database name I used was "HighlightsVault".

Ready to run on IIS.
Ready to run in the included Docker web server.
Ready to run in AWS Elastic Beanstalk.

The biggest regret I have with this app, is writing a "AddHighlight" (controller IActionResult method) and using it for BOTH Singular, and Multiple and/or Multiple Group entries.
It's not too awful but, I probably should have seperated these methods. Maybe one day!

Make sure if intending to use Steam IDs for Highlight creation, to register for a Steam API key and replace the placeholder/non-working keys!

Thanks for viewing.

---- Main table

USE [HighlightsVault]
GO

/****** Object:  Table [dbo].[HighlightsVault]    Script Date: 6/20/2024 6:49:20 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HighlightsVault](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[steamID] [nvarchar](50) NOT NULL,
	[GroupId] [int] NULL,
	[HighlightPersonName] [nvarchar](100) NULL,
	[UserDescription] [nvarchar](510) NULL,
	[ProfileUrl] [nvarchar](255) NULL,
	[ProfilePictureUrl] [nvarchar](255) NULL,
	[ProfilePicture] [varbinary](max) NULL,
	[HighlightDate] [datetime] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Clip] [varbinary](max) NULL,
 CONSTRAINT [PK__HighlightVa__3214EC2721277B20] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[HighlightsVault]  WITH CHECK ADD  CONSTRAINT [FK_HighlightsVault_Groups] FOREIGN KEY([GroupId])
REFERENCES [dbo].[HighlightsVaultGroups] ([GroupId])
GO

ALTER TABLE [dbo].[HighlightsVault] CHECK CONSTRAINT [FK_HighlightsVault_Groups]
GO


---- Highlight Vaults foreign key table containing group definitions when adding multiple users at once.

USE [HighlightsVault]
GO

/****** Object:  Table [dbo].[HighlightsVaultGroups]    Script Date: 6/20/2024 6:50:18 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HighlightsVaultGroups](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK__HighlightVaultsGro__149AF36ABF705B5F] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

---- Fairly primitive table that stores simple passwords that trigger different views for the main page text box entry.

USE [HighlightsVault]
GO

/****** Object:  Table [dbo].[Passwords]    Script Date: 6/20/2024 6:50:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Passwords](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PasswordValue] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


