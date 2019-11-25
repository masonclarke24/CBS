USE [master]
GO
DROP DATABASE CBS
/****** Object:  Database [CBS]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE DATABASE [CBS]

ALTER DATABASE [CBS] SET COMPATIBILITY_LEVEL = 130
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [CBS].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [CBS] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [CBS] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [CBS] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [CBS] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [CBS] SET ARITHABORT OFF 
GO
ALTER DATABASE [CBS] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [CBS] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [CBS] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [CBS] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [CBS] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [CBS] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [CBS] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [CBS] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [CBS] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [CBS] SET  ENABLE_BROKER 
GO
ALTER DATABASE [CBS] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [CBS] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [CBS] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [CBS] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [CBS] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [CBS] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [CBS] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [CBS] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [CBS] SET  MULTI_USER 
GO
ALTER DATABASE [CBS] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [CBS] SET DB_CHAINING OFF 
GO
ALTER DATABASE [CBS] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [CBS] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [CBS] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [CBS] SET QUERY_STORE = OFF
GO
USE [CBS]
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
USE [CBS]
GO
/****** Object:  UserDefinedTableType [dbo].[GolferList]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE TYPE [dbo].[GolferList] AS TABLE(
	[MemberNumber] [varchar](40) NULL
)
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoleClaims]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoleClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](256) NULL,
	[NormalizedName] [nvarchar](256) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](450) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](450) NOT NULL,
	[ProviderKey] [nvarchar](450) NOT NULL,
	[ProviderDisplayName] [nvarchar](max) NULL,
	[UserId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](450) NOT NULL,
	[RoleId] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](450) NOT NULL,
	[UserName] [nvarchar](256) NULL,
	[NormalizedUserName] [nvarchar](256) NULL,
	[Email] [nvarchar](256) NULL,
	[NormalizedEmail] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[ConcurrencyStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEnd] [datetimeoffset](7) NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[MemberName] [varchar](40) NOT NULL,
	[MemberNumber] [char](10) NOT NULL,
 CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_AspNetUsers_MemberNumber] UNIQUE NONCLUSTERED 
(
	[MemberNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserTokens]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserTokens](
	[UserId] [nvarchar](450) NOT NULL,
	[LoginProvider] [nvarchar](450) NOT NULL,
	[Name] [nvarchar](450) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[LoginProvider] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GolferMembershipLevels]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GolferMembershipLevels](
	[MemberNumber] [nvarchar](450) NOT NULL,
	[MembershipLevel] [char](6) NULL,
PRIMARY KEY CLUSTERED 
(
	[MemberNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MembershipLevels]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MembershipLevels](
	[MembershipLevel] [char](6) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[MembershipLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PermissableTeeTimes]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PermissableTeeTimes](
	[Time] [time](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Time] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StandingTeeTimeGolfers]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StandingTeeTimeGolfers](
	[MemberNumber] [nvarchar](450) NOT NULL,
	[ID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC,
	[MemberNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StandingTeeTimeRequests]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StandingTeeTimeRequests](
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[RequestedTime] [time](7) NOT NULL,
	[DayOfWeek]  AS (datename(weekday,[StartDate])),
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_StandingTeeTimeRequests] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ__Standing__0DF0B875B7E73BB7] UNIQUE NONCLUSTERED 
(
	[StartDate] ASC,
	[EndDate] ASC,
	[RequestedTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TeeTimeGolfers]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeeTimeGolfers](
	[Date] [date] NOT NULL,
	[Time] [time](7) NOT NULL,
	[MemberNumber] [nvarchar](450) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TeeTimes]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeeTimes](
	[Date] [date] NOT NULL,
	[Time] [time](7) NOT NULL,
	[Phone] [varchar](14) NOT NULL,
	[NumberOfCarts] [tinyint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Date] ASC,
	[Time] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TeeTimesForMembershipLevel]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TeeTimesForMembershipLevel](
	[Time] [time](7) NOT NULL,
	[DayOfWeek] [char](9) NOT NULL,
	[MembershipLevel] [char](6) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Time] ASC,
	[DayOfWeek] ASC,
	[MembershipLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetRoleClaims_RoleId]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[NormalizedName] ASC
)
WHERE ([NormalizedName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserClaims_UserId]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserLogins_UserId]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AspNetUserRoles_RoleId]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [EmailIndex]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE NONCLUSTERED INDEX [EmailIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedEmail] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 11/22/2019 9:32:28 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[NormalizedUserName] ASC
)
WHERE ([NormalizedUserName] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ('') FOR [MemberName]
GO
ALTER TABLE [dbo].[AspNetUsers] ADD  DEFAULT ('') FOR [MemberNumber]
GO
ALTER TABLE [dbo].[AspNetRoleClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetRoleClaims] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserTokens]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserTokens] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[GolferMembershipLevels]  WITH CHECK ADD FOREIGN KEY([MembershipLevel])
REFERENCES [dbo].[MembershipLevels] ([MembershipLevel])
GO
ALTER TABLE [dbo].[GolferMembershipLevels]  WITH CHECK ADD  CONSTRAINT [FK_GolferMembershipLevels_AspNetUsers] FOREIGN KEY([MemberNumber])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[GolferMembershipLevels] CHECK CONSTRAINT [FK_GolferMembershipLevels_AspNetUsers]
GO
ALTER TABLE [dbo].[StandingTeeTimeGolfers]  WITH CHECK ADD  CONSTRAINT [FK__StandingT__Membe__4AB81AF0] FOREIGN KEY([MemberNumber])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[StandingTeeTimeGolfers] CHECK CONSTRAINT [FK__StandingT__Membe__4AB81AF0]
GO
ALTER TABLE [dbo].[StandingTeeTimeGolfers]  WITH CHECK ADD  CONSTRAINT [FK_StandingTeeTimeGolfers_StandingTeeTimeRequests1] FOREIGN KEY([ID])
REFERENCES [dbo].[StandingTeeTimeRequests] ([ID])
GO
ALTER TABLE [dbo].[StandingTeeTimeGolfers] CHECK CONSTRAINT [FK_StandingTeeTimeGolfers_StandingTeeTimeRequests1]
GO
ALTER TABLE [dbo].[StandingTeeTimeRequests]  WITH CHECK ADD  CONSTRAINT [FK__StandingT__Reque__4CA06362] FOREIGN KEY([RequestedTime])
REFERENCES [dbo].[PermissableTeeTimes] ([Time])
GO
ALTER TABLE [dbo].[StandingTeeTimeRequests] CHECK CONSTRAINT [FK__StandingT__Reque__4CA06362]
GO
ALTER TABLE [dbo].[TeeTimeGolfers]  WITH CHECK ADD FOREIGN KEY([MemberNumber])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[TeeTimeGolfers]  WITH CHECK ADD FOREIGN KEY([Date], [Time])
REFERENCES [dbo].[TeeTimes] ([Date], [Time])
GO
ALTER TABLE [dbo].[TeeTimeGolfers]  WITH CHECK ADD FOREIGN KEY([Date], [Time])
REFERENCES [dbo].[TeeTimes] ([Date], [Time])
GO
ALTER TABLE [dbo].[TeeTimes]  WITH CHECK ADD FOREIGN KEY([Time])
REFERENCES [dbo].[PermissableTeeTimes] ([Time])
GO
ALTER TABLE [dbo].[TeeTimesForMembershipLevel]  WITH CHECK ADD FOREIGN KEY([MembershipLevel])
REFERENCES [dbo].[MembershipLevels] ([MembershipLevel])
GO
ALTER TABLE [dbo].[TeeTimesForMembershipLevel]  WITH CHECK ADD FOREIGN KEY([Time])
REFERENCES [dbo].[PermissableTeeTimes] ([Time])
GO
ALTER TABLE [dbo].[StandingTeeTimeRequests]  WITH CHECK ADD  CONSTRAINT [CK_STTR_Dates] CHECK  ((datediff(day,[EndDate],[StartDate])<(0)))
GO
ALTER TABLE [dbo].[StandingTeeTimeRequests] CHECK CONSTRAINT [CK_STTR_Dates]
GO
ALTER TABLE [dbo].[StandingTeeTimeRequests]  WITH CHECK ADD  CONSTRAINT [ck_WeekdayMatch] CHECK  ((datepart(weekday,[StartDate])=datepart(weekday,[EndDate])))
GO
ALTER TABLE [dbo].[StandingTeeTimeRequests] CHECK CONSTRAINT [ck_WeekdayMatch]
GO
/****** Object:  StoredProcedure [dbo].[FindDailyTeeSheet]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FindDailyTeeSheet](@date DATE)
AS
	SELECT
		LEFT(CONVERT(NVARCHAR,PermissableTeeTimes.Time, 24),5) [Time],
		TeeTimes.NumberOfCarts,
		TeeTimes.Phone,
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE TeeTimeGolfers.MemberNumber = AspNetUsers.Id) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN TeeTimes ON
		PermissableTeeTimes.Time = TeeTimes.Time LEFT OUTER JOIN
		TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Date = @date
GO
/****** Object:  StoredProcedure [dbo].[GetPermittedTeeTimes]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPermittedTeeTimes](@memberNumber NVARCHAR(450), @dayOfWeek CHAR(10))
AS
	SELECT
		[Time]
	FROM
		TeeTimesForMembershipLevel INNER JOIN GolferMembershipLevels ON
		TeeTimesForMembershipLevel.MembershipLevel = GolferMembershipLevels.MembershipLevel
	WHERE
		GolferMembershipLevels.MemberNumber = @memberNumber AND DayOfWeek = @dayOfWeek

GO
/****** Object:  StoredProcedure [dbo].[MemberExists]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[MemberExists](@memberNumber VARCHAR(10))
AS
	IF EXISTS(SELECT * FROM AspNetUsers WHERE MemberNumber = @memberNumber)
		SELECT 0
	SELECT 1
GO
/****** Object:  StoredProcedure [dbo].[RequestStandingTeeTime]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RequestStandingTeeTime](@startDate DATE, @endDate DATE, @requestedTime TIME, @message VARCHAR(512) out, @memberNumbers AS GolferList READONLY)
AS
BEGIN
	BEGIN TRANSACTION

	INSERT INTO StandingTeeTimeRequests(StartDate,EndDate,RequestedTime)
	VALUES(@startDate, @endDate,@requestedTime)

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to create a standing tee time request for start date ' + CAST(@startDate AS NVARCHAR(15))
		ROLLBACK TRANSACTION
		RAISERROR(@message,16,1)
		RETURN 1
	END
	declare @id int = @@IDENTITY
	INSERT INTO StandingTeeTimeGolfers(ID, MemberNumber)
		SELECT
			@id, MemberNumber
		FROM
			@memberNumbers
		IF @@ERROR <> 0
		BEGIN
		SET @message = 'Unable to add golfers to standing tee time on start date ' + CAST(@startDate AS NVARCHAR(24))
		ROLLBACK TRANSACTION
		IF @@TRANCOUNT > 0
		RAISERROR(@message,16,1)
		RETURN 1
		END

	COMMIT TRANSACTION
	RETURN 0
	END
GO
/****** Object:  StoredProcedure [dbo].[ReserveTeeTime]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ReserveTeeTime]
@date DATE, @time TIME, @numberOfCarts INT, @phone VARCHAR(14), @golfers AS dbo.GolferList READONLY, @message VARCHAR(1024) OUT
AS
BEGIN
	DECLARE @returnCode AS INT = 0
	--Verify that the selected tee time is permissable for anyone
	IF NOT EXISTS(SELECT * FROM PermissableTeeTimes WHERE [Time] = @time)
		BEGIN
			SET @message = 'Selected time: ' + CAST(@time AS nvarchar(8)) + ' is forbidden'
			RAISERROR(@message, 16,1)
			RETURN 1
		END
	--Verify that the selected tt time does not conflict with any member's membership level

	SELECT DISTINCT
		MembershipLevels.MembershipLevel INTO #membership_levels
	FROM
		MembershipLevels INNER JOIN GolferMembershipLevels ON MembershipLevels.MembershipLevel = GolferMembershipLevels.MembershipLevel
		INNER JOIN AspNetUsers ON GolferMembershipLevels.MemberNumber = AspNetUsers.Id
	WHERE
		AspNetUsers.Id IN(SELECT * FROM @golfers)

	DECLARE cursor_membershipLevels CURSOR  
	FOR SELECT
		MembershipLevel
	FROM
		#membership_levels

	DECLARE @membershipLevel VARCHAR(9)
	DECLARE @dayOfWeek VARCHAR(9) =

	CASE DATEPART(weekday, @date)
		WHEN 1 THEN 'Weekend'
		WHEN 7 THEN 'Weekend'
		ELSE 'Weekday'
		END

	
	OPEN cursor_membershipLevels

	FETCH NEXT FROM cursor_membershipLevels INTO @membershipLevel
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS(SELECT * FROM TeeTimesForMembershipLevel WHERE MembershipLevel = @membershipLevel AND DayOfWeek = @dayOfWeek AND [Time] = @time)
			BEGIN
				DECLARE @conflictingMembers VARCHAR(512)
					SELECT @conflictingMembers = COALESCE(@conflictingMembers + ', ', '') + MemberName
					FROM
						AspNetUsers INNER JOIN GolferMembershipLevels ON AspNetUsers.Id = GolferMembershipLevels.MemberNumber
					WHERE MembershipLevel = @membershipLevel AND AspNetUsers.Id IN(SELECT * FROM @golfers)

				SET @message = 'Selected tee time: ' + CAST(@time AS nvarchar(8)) + ' conflicts with membership level: ' + @membershipLevel + ' for members: ' + @conflictingMembers
				CLOSE cursor_membershipLevels
				DEALLOCATE cursor_membershipLevels
				RAISERROR(@message,16,1)
				RETURN 1
			END
		FETCH NEXT FROM cursor_membershipLevels INTO @membershipLevel
	END
	IF CURSOR_STATUS('local','cursor_membershipLevels')>=-1
		BEGIN
			CLOSE cursor_membershipLevels
			DEALLOCATE cursor_membershipLevels
		END	

	BEGIN TRANSACTION
	INSERT INTO TeeTimes(Date,Time,NumberOfCarts, Phone) VALUES(@date,@time,@numberOfCarts, @phone)

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to reserve tee time ' + CAST(@time AS nvarchar(8)) + ' for day ' + CAST(@date AS NVARCHAR(15))
		ROLLBACK TRANSACTION
		RAISERROR(@message,16,1)
		RETURN 1
	END
	COMMIT TRANSACTION
	BEGIN TRANSACTION
	INSERT INTO TeeTimeGolfers(Date, Time, MemberNumber)
		SELECT
			@date,
			@time,
			MemberNumber
		FROM
			@golfers

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to add golfers to tee time for time: ' + CAST(@time AS NVARCHAR(8)) + ' on day: ' + CAST(@date AS NVARCHAR(15))
		ROLLBACK TRANSACTION
		RAISERROR(@message,16,1)
		RETURN 1
	END
	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION
	RETURN @returnCode
END
GO
/****** Object:  StoredProcedure [dbo].[ViewStandingTeeTimeRequests]    Script Date: 11/22/2019 9:32:28 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[ViewStandingTeeTimeRequests](@startDate date, @endDate date)
AS
	DECLARE @dayOfWeek VARCHAR(9) = DATENAME(DAY,@startDate)
	SELECT
		LEFT(CONVERT(NVARCHAR,[Time],24),5) [Requested Time],
		StandingTeeTimeRequests.StartDate [Start Date],
		StandingTeeTimeRequests.EndDate [End Date],
		[DayOfWeek] [Day of Week],
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE StandingTeeTimeGolfers.MemberNumber = AspNetUsers.Id) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN StandingTeeTimeRequests ON
		PermissableTeeTimes.Time = StandingTeeTimeRequests.RequestedTime AND StandingTeeTimeRequests.StartDate BETWEEN @startDate AND @endDate
		LEFT OUTER JOIN StandingTeeTimeGolfers ON StandingTeeTimeRequests.ID = StandingTeeTimeGolfers.ID
GO
USE [master]
GO
ALTER DATABASE [CBS] SET  READ_WRITE 
GO
