USE CBS
GO

SELECT * FROM AccountTransactions

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'GolferMembershipLevels')
	DROP TABLE GolferMembershipLevels
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'TeeTimesForMembershipLevel')
	DROP TABLE TeeTimesForMembershipLevel
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'MembershipLevels')
	DROP TABLE MembershipLevels
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'StandingTeeTimeGolfers')
	DROP TABLE StandingTeeTimeGolfers
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'StandingTeeTimeRequests')
	DROP TABLE StandingTeeTimeRequests
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'TeeTimeGolfers')
	DROP TABLE TeeTimeGolfers
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'TeeTimes')
	DROP TABLE TeeTimes
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'MembershipApplication')
	DROP TABLE MembershipApplication
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'PermissableTeeTimes')
	DROP TABLE PermissableTeeTimes
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'AccountTransactions')
	DROP TABLE AccountTransactions
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'ScoreDetails')
	DROP TABLE ScoreDetails
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'ScoreCard')
	DROP TABLE ScoreCard
GO

IF EXISTS(SELECT * FROM SYS.TABLES WHERE [name] LIKE 'HandicapReport')
	DROP TABLE HandicapReport
GO
IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'ViewStandingTeeTimeRequests')
	DROP PROCEDURE ViewStandingTeeTimeRequests
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'ReserveTeeTime')
	DROP PROCEDURE ReserveTeeTime
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'RequestStandingTeeTime')
	DROP PROCEDURE RequestStandingTeeTime
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'GetPermittedTeeTimes')
	DROP PROCEDURE GetPermittedTeeTimes
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'FindDailyTeeSheet')
	DROP PROCEDURE FindDailyTeeSheet
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'FindReservedTeeTimes')
	DROP PROCEDURE FindReservedTeeTimes
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'CancelTeeTime')
	DROP PROCEDURE CancelTeeTime
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'UpdateTeeTime')
	DROP PROCEDURE UpdateTeeTime
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'FindSTTR')
	DROP PROCEDURE FindSTTR
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'CancelSTTR')
	DROP PROCEDURE CancelSTTR
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'RecordMembershipApplication')
	DROP PROCEDURE RecordMembershipApplication
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'GetMembershipApplications')
	DROP PROCEDURE GetMembershipApplications
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'UpdateMembershipApplication')
	DROP PROCEDURE UpdateMembershipApplication
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'CreateMemberAccount')
	DROP PROCEDURE CreateMemberAccount
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'AssessMembershipFees')
	DROP PROCEDURE AssessMembershipFees
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'GetAccountDetail')
	DROP PROCEDURE GetAccountDetail
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'ViewAllAccountsSummary')
	DROP PROCEDURE ViewAllAccountsSummary
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [name] LIKE 'RecordScores')
	DROP PROCEDURE RecordScores
GO

IF EXISTS(SELECT * FROM SYS.TYPES WHERE [name] LIKE 'HoleByHoleScores')
	DROP TYPE HoleByHoleScore
GO

IF EXISTS(SELECT * FROM SYS.TYPES WHERE [name] LIKE 'GolferList')
	DROP TYPE GolferList
GO

IF EXISTS(SELECT * FROM SYS.TYPES WHERE [name] LIKE 'FeeDetails')
	DROP TYPE FeeDetails
GO

CREATE TABLE PermissableTeeTimes
(
	[Time] TIME CONSTRAINT PK_PermisTeeTime_Time PRIMARY KEY
)
GO

CREATE TABLE TeeTimes
(
	[Date] DATE NOT NULL,
	[Time] TIME CONSTRAINT FK_TeeTimes_Time FOREIGN KEY REFERENCES PermissableTeeTimes([Time]) ON DELETE CASCADE ON UPDATE CASCADE,
	Phone VARCHAR(14) NOT NULL,
	NumberOfCarts TINYINT NOT NULL,
	CheckedIn BIT DEFAULT(0),
	ReservedBy NVARCHAR(450) CONSTRAINT FK_TeeTimes_ReservedBy REFERENCES AspNetUsers(Id) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT PK_TeeTimes_DateTime PRIMARY KEY ([Date],[Time])
)
GO
CREATE TABLE TeeTimeGolfers
(
	[Date] DATE NOT NULL,
	[Time] Time NOT NULL,
	UserId NVARCHAR(450) NOT NULL CONSTRAINT FK_TeeTimeGolfers_UserId FOREIGN KEY REFERENCES AspNetUsers(Id),
	CONSTRAINT FK_TeeTimeGolfers_DateTime FOREIGN KEY ([Date],[Time]) REFERENCES TeeTimes([Date],[Time]) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT PK_TeeTimeGolfers_DateTimeMemNumber PRIMARY KEY ([Date],[Time],UserId)
)
GO

CREATE TABLE StandingTeeTimeRequests
(
	ID INT IDENTITY PRIMARY KEY,
	StartDate DATE,
	EndDate DATE,
	RequestedTime TIME CONSTRAINT FK_STTR_RequestedTime FOREIGN KEY REFERENCES PermissableTeeTimes([Time]),
	[DayOfWeek] AS DATENAME(DW, StartDate),
	SubmittedBy NVARCHAR(450) FOREIGN KEY REFERENCES AspNetUsers(Id) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT CHK_STTR_StartDate CHECK (DATEPART(DW, StartDate) = DATEPART(DW, EndDate)),
	CONSTRAINT CHK_STTR_EndDate CHECK (DATEDIFF(DAY, StartDate, EndDate) > 0)
)
GO

CREATE TABLE StandingTeeTimeGolfers
(
	ID INT CONSTRAINT FK_STTGolfers_ID FOREIGN KEY REFERENCES StandingTeeTimeRequests(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	UserId NVARCHAR(450) CONSTRAINT FK_STTGolfers_UserId FOREIGN KEY REFERENCES AspNetUsers(Id),
	PRIMARY KEY (ID,UserId)
)
GO

CREATE TABLE MembershipLevels
(
	MembershipLevel CHAR(10) CONSTRAINT PK_MembershipLevels_MembershipLevel PRIMARY KEY
)
GO

CREATE TABLE TeeTimesForMembershipLevel
(
	[Time] TIME CONSTRAINT FK_TeeTimeForMemLevel_Time FOREIGN KEY REFERENCES PermissableTeeTimes([Time]),
	[DayOfWeek] INT CONSTRAINT CHK_TeeTimeForMemLevel_DayOfWeek CHECK ([DayOfWeek] IN(1,2,3,4,5,6,7)),
	MembershipLevel CHAR(10) CONSTRAINT FK_TeeTimeForMemLevel_MemLevel FOREIGN KEY REFERENCES MembershipLevels(MembershipLevel),
	PRIMARY KEY ([Time],[DayOfWeek],MembershipLevel)
)
GO


CREATE TABLE MembershipApplication
(
	LastName VARCHAR(25) NOT NULL,
	FirstName VARCHAR(25) NOT NULL,
	ApplicantAddress VARCHAR(64) NOT NULL,
	ApplicantCity VARCHAR(35) NOT NULL,
	ApplicantPostalCode VARCHAR(7) NOT NULL CONSTRAINT CHK_MembershipApplication_AppPostalCode CHECK (ApplicantPostalCode LIKE '[A-Z][0-9][A-Z] [0-9][A-Z][0-9]'),
	ApplicantPhone VARCHAR(10) NOT NULL CONSTRAINT CHK_MembershipApplication_AppPhone CHECK (ApplicantPhone LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	ApplicantAlternatePhone VARCHAR(10) NOT NULL CONSTRAINT CHK_MembershipApplication_AltPhone CHECK (ApplicantAlternatePhone LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	Email VARCHAR(48) NOT NULL CONSTRAINT CHK_MembershipApplication_Email CHECK (Email LIKE '%@%.%'),
	DateOfBirth DATE NOT NULL CONSTRAINT CHK_MembershipApplication_DateOfBirth CHECK (DateOfBirth < GETDATE()),
	MembershipType VARCHAR(15) NOT NULL CONSTRAINT CHK_MembershipApplication_MembershipType CHECK (MembershipType IN('Shareholder','Associate')),
	Occupation VARCHAR(25) NOT NULL,
	CompanyName VARCHAR(30) NOT NULL,
	EmployerAddress VARCHAR(64) NOT NULL,
	EmployerCity VARCHAR(35) NOT NULL,
	EmployerPostalCode VARCHAR(7) NOT NULL CONSTRAINT CHK_MembershipApplication_EmployerPostalCode CHECK (EmployerPostalCode LIKE '[A-Z][0-9][A-Z] [0-9][A-Z][0-9]'),
	EmployerPhone VARCHAR(10) NOT NULL CONSTRAINT CHK_MembershipApplication_EmployerPhone CHECK (EmployerPhone LIKE '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
	ProspectiveMemberCertification BIT NOT NULL,
	ApplicationDate DATE NOT NULL,
	SponsoringShareholder1 VARCHAR(25) NOT NULL,
	Shareholder1SigningDate DATE NOT NULL CONSTRAINT CHK_MembershipApplication_Sh1Date CHECK (Shareholder1SigningDate <= GETDATE()),
	SponsoringShareholder2 VARCHAR(25) NOT NULL,
	Shareholder2SigningDate DATE NOT NULL CONSTRAINT CHK_MembershipApplication_Sh2Date CHECK (Shareholder2SigningDate <= GETDATE()),
	ShareholderCertification BIT NOT NULL,
	ApplicationStatus VARCHAR(10) NULL CONSTRAINT CHK_MembershipApplication_ApplicationStatus CHECK (ApplicationStatus IN('Accepted','Denied','OnHold', 'Waitlisted')),
	CONSTRAINT PK_MembershipApplication PRIMARY KEY (LastName,FirstName,ApplicationDate)
)
GO

CREATE TABLE AccountTransactions
(
	UserId NVARCHAR(450) NOT NULL CONSTRAINT FK_AccountTransactions_UserId FOREIGN KEY REFERENCES AspNetUsers(Id),
	TransactionDate DATETIME NOT NULL,
	BookedDate DATETIME NULL,
	Amount MONEY NOT NULL,
	Description VARCHAR(40) NOT NULL,
	DueDate DATE NOT NULL,
	CONSTRAINT PK_AccountTransactions PRIMARY KEY (UserId, TransactionDate, Description)
)

CREATE TABLE ScoreCard
(
	Course VARCHAR(30) NOT NULL,
	Rating DECIMAL(6,3) NOT NULL,
	Slope DECIMAL(6,3) NOT NULL,
	Date DATETIME NOT NULL,
	UserId NVARCHAR(450) NOT NULL CONSTRAINT FK_ScoreCard_UserId FOREIGN KEY REFERENCES AspNetUsers(Id),
	CONSTRAINT PK_ScoreCard_DateId PRIMARY KEY (Date,UserId)
)

CREATE TABLE ScoreDetails
(
	Date DATETIME NOT NULL,
	UserId NVARCHAR(450) NOT NULL,
	Hole INT NOT NULL CONSTRAINT CHK_ScoreDetails_Hole CHECK (Hole BETWEEN 1 AND 18),
	Score INT NOT NULL,
	CONSTRAINT FK_ScoreDetails_DateId FOREIGN KEY (Date,UserId) REFERENCES ScoreCard(Date,UserId),
	CONSTRAINT PK_ScoreDetails_DateIdHole PRIMARY KEY (Date, UserId, Hole)
)

CREATE TABLE HandicapReport
(
	UserId NVARCHAR(450) NOT NULL CONSTRAINT FK_HandicapReport_UserId FOREIGN KEY REFERENCES AspNetUsers(Id),
	LastUpdated DATE NOT NULL CONSTRAINT CHK_HandicapReport_LastUpdate CHECK (LastUpdated <= GETDATE()),
	Month AS DATENAME(MONTH,LastUpdated),
	Year AS DATEPART(YEAR,LastUpdated),
	HandicapFactor DECIMAL(6,3) NOT NULL,
	Average DECIMAL(6,3) NOT NULL,
	BestOfTenAverage DECIMAL(6,3) NOT NULL,
	CONSTRAINT PK_HandicapReport_UserIdDate PRIMARY KEY (UserId, LastUpdated)
)

CREATE TYPE GolferList AS TABLE
(
	UserId NVARCHAR(450) NOT NULL
)
GO

CREATE TYPE FeeDetails AS TABLE
(
	Description VARCHAR(35),
	Amount MONEY,
	DueDate DATE
)
GO

CREATE TYPE HoleByHoleScores AS TABLE
(
	HoleScore INT
)
GO

CREATE PROCEDURE FindDailyTeeSheet(@date DATE)
AS
	SELECT DISTINCT
		LEFT(CONVERT(NVARCHAR,PermissableTeeTimes.Time, 24),5) [Time],
		Date,
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE TeeTimeGolfers.UserId = AspNetUsers.Id) [Member Name],
		TeeTimeGolfers.UserId,
		(SELECT ReservedBy FROM TeeTimes WHERE TeeTimes.[Date] = TeeTimeGolfers.[Date] AND TeeTimes.[Time] = TeeTimeGolfers.[Time]) [ReservedBy],
		(SELECT Phone FROM TeeTimes WHERE TeeTimes.[Date] = TeeTimeGolfers.[Date] AND TeeTimes.[Time] = TeeTimeGolfers.[Time]) [Phone],
		(SELECT NumberOfCarts FROM TeeTimes WHERE TeeTimes.[Date] = TeeTimeGolfers.[Date] AND TeeTimes.[Time] = TeeTimeGolfers.[Time]) NumberofCarts,
		(SELECT CheckedIn FROM TeeTimes WHERE TeeTimes.[Date] = TeeTimeGolfers.[Date] AND TeeTimes.[Time] = TeeTimeGolfers.[Time]) [Checked In]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN TeeTimeGolfers ON PermissableTeeTimes.Time = TeeTimeGolfers.Time AND TeeTimeGolfers.Date = @date
GO


CREATE PROCEDURE [dbo].[GetPermittedTeeTimes](@userId NVARCHAR(450), @dayOfWeek INT)
AS
	SELECT
		[Time]
	FROM
		TeeTimesForMembershipLevel
	WHERE
		MembershipLevel = (SELECT TOP 1 MembershipLevel FROM AspNetUsers WHERE Id = @userId) AND [DayOfWeek] = @dayOfWeek
GO


CREATE PROCEDURE [dbo].[RequestStandingTeeTime](@startDate DATE, @endDate DATE, @requestedTime TIME, @submittedBy VARCHAR(450), @message VARCHAR(512) out, @userIds AS GolferList READONLY)
AS
BEGIN
	BEGIN TRANSACTION

	INSERT INTO StandingTeeTimeRequests(StartDate,EndDate,RequestedTime, SubmittedBy)
	VALUES(@startDate, @endDate,@requestedTime, @submittedBy)

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to create a standing tee time request for start date ' + CAST(@startDate AS NVARCHAR(15))
		ROLLBACK TRANSACTION
		RAISERROR(@message,16,1)
		RETURN 1
	END
	declare @id int = @@IDENTITY
	INSERT INTO StandingTeeTimeGolfers(ID, UserId)
		SELECT
			@id, UserId
		FROM
			@userIds
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

CREATE PROCEDURE [dbo].[ReserveTeeTime]
@date DATE, @time TIME, @numberOfCarts INT, @phone VARCHAR(14), @reservedBy VARCHAR(450), @golfers AS dbo.GolferList READONLY, @message VARCHAR(1024) OUT
AS
BEGIN
	DECLARE @returnCode AS INT = 0

	BEGIN TRANSACTION
	INSERT INTO TeeTimes(Date,Time,NumberOfCarts, Phone, ReservedBy) VALUES(@date,@time,@numberOfCarts, @phone, @reservedBy)

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to reserve tee time ' + CAST(@time AS nvarchar(8)) + ' for day ' + CAST(@date AS NVARCHAR(15))
		ROLLBACK TRANSACTION
		RAISERROR(@message,16,1)
		RETURN 1
	END

	INSERT INTO TeeTimeGolfers(Date, Time, UserId)
		SELECT
			@date,
			@time,
			UserId
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


CREATE PROCEDURE [dbo].[ViewStandingTeeTimeRequests](@startDate date, @endDate date)
AS
	DECLARE @dayOfWeek VARCHAR(9) = DATENAME(WEEKDAY,@startDate)
	SELECT
		LEFT(CONVERT(NVARCHAR,[Time],24),5) [Requested Time],
		StandingTeeTimeRequests.StartDate [Start Date],
		StandingTeeTimeRequests.EndDate [End Date],
		[DayOfWeek] [Day of Week],
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE StandingTeeTimeGolfers.UserId = AspNetUsers.Id) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN StandingTeeTimeRequests ON
		PermissableTeeTimes.Time = StandingTeeTimeRequests.RequestedTime AND (StandingTeeTimeRequests.StartDate BETWEEN @startDate AND @endDate) AND StandingTeeTimeRequests.DayOfWeek = @dayOfWeek
		LEFT OUTER JOIN StandingTeeTimeGolfers ON StandingTeeTimeRequests.ID = StandingTeeTimeGolfers.ID
GO

CREATE PROCEDURE FindReservedTeeTimes(@userID VARCHAR(450))
AS
	DECLARE cursorReservedTeeTimes CURSOR FOR
	SELECT
		TeeTimes.[Date],
		TeeTimes.[Time]
	FROM 
		TeeTimes INNER JOIN TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Time = TeeTimeGolfers.Time
	WHERE 
		TeeTimeGolfers.UserId = @userID

	DECLARE @date Date
	DECLARE @time Time

	OPEN cursorReservedTeeTimes

	FETCH NEXT FROM cursorReservedTeeTimes INTO @date, @time

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT
		TeeTimes.[Date],
		TeeTimes.[Time],
		NumberOfCarts,
		Phone,
		TeeTimeGolfers.UserId,
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE TeeTimeGolfers.UserId = AspNetUsers.Id) [Member Name],
		ReservedBy,
		CheckedIn AS [Checked In]
	FROM
		TeeTimes INNER JOIN TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Time = TeeTimeGolfers.Time
	WHERE
		TeeTimes.[Date] = @date AND TeeTimes.Time = @time

	FETCH NEXT FROM cursorReservedTeeTimes INTO @date, @time
	END

	CLOSE cursorReservedTeeTimes
	DEALLOCATE cursorReservedTeeTimes
GO
CREATE PROCEDURE CancelTeeTime(@date DATE, @time TIME, @userID VARCHAR(450))
AS
	BEGIN TRANSACTION

	DELETE TeeTimes WHERE Date = @date AND Time = @time AND ReservedBy = @userID

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to cancel tee time',16,1)
		RETURN 1
	END

	DELETE TeeTimeGolfers WHERE [Date] = @date AND [Time] = @time AND UserId = @userID

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to remove golfer from tee time',16,1)
		RETURN 1
	END
	COMMIT TRANSACTION

	RETURN 0
GO

CREATE PROCEDURE UpdateTeeTime (@date DATE, @time TIME, @numberOfCarts INT = NULL, @phone VARCHAR(14) = NULL, @checkedIn BIT = NULL, @newGolfers AS dbo.GolferList READONLY)
AS
	BEGIN TRANSACTION

	UPDATE TeeTimes
		SET NumberOfCarts = COALESCE(@numberOfCarts, NumberOfCarts),
			Phone = COALESCE(@phone, Phone),
			CheckedIn = COALESCE(@checkedIn, CheckedIn)
		WHERE [Date] = @date AND [Time] = @time

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to update tee time',16,1)
		RETURN 1
	END

	INSERT INTO TeeTimeGolfers([Date],[Time], UserId)
		SELECT
			@date, @time, UserId
		FROM @newGolfers

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to update tee time details',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO

CREATE PROCEDURE FindSTTR(@userID NVARCHAR(450))
AS
	SELECT
		LEFT(CONVERT(NVARCHAR,RequestedTime,24),5) [Requested Time],
		StandingTeeTimeRequests.StartDate [Start Date],
		StandingTeeTimeRequests.EndDate [End Date],
		[DayOfWeek] [Day of Week],
		SubmittedBy [Submitted By],
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE StandingTeeTimeGolfers.UserId = AspNetUsers.Id) [Member Name]
	FROM
		StandingTeeTimeRequests INNER JOIN StandingTeeTimeGolfers ON StandingTeeTimeRequests.ID = StandingTeeTimeGolfers.ID
	WHERE
		StandingTeeTimeRequests.SubmittedBy = @userId
GO

CREATE PROCEDURE CancelSTTR(@startDate DATE, @endDate Date, @requestedTime TIME)
AS
	BEGIN TRANSACTION

	DELETE StandingTeeTimeRequests WHERE StartDate = @startDate AND EndDate = @endDate AND RequestedTime = @requestedTime

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to cancel standing tee time request',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO

CREATE PROCEDURE RecordMembershipApplication(@lastName VARCHAR(25), @firstName VARCHAR(25), @applicantAddress VARCHAR(64), @applicantCity VARCHAR(35),
@applicantPostalCode VARCHAR(7), @applicantPhone VARCHAR(10), @applicantAlternatePhone VARCHAR(10), @email VARCHAR(48), @dateOfBirth DATE, @membershipType VARCHAR(25), @occupation VARCHAR(25),
@companyName VARCHAR(30), @employerAddress VARCHAR(64), @employerCity VARCHAR(35), @employerPostalCode VARCHAR(7), @employerPhone VARCHAR(10), @prospectiveMemberCertification BIT, @sponsoringShareholder1 VARCHAR(25),
@shareholder1SigningDate DATE, @sponsoringShareholder2 VARCHAR(25), @shareholder2SigningDate DATE, @shareholderCertification BIT)
AS
	BEGIN TRANSACTION

	INSERT INTO MembershipApplication VALUES(@lastName, @firstName, @applicantAddress, @applicantCity, @applicantPostalCode, @applicantPhone, @applicantAlternatePhone, @email, @dateOfBirth,
	@membershipType, @occupation, @companyName, @employerAddress, @employerCity, @employerPostalCode, @employerPhone, @prospectiveMemberCertification, GETDATE(), @sponsoringShareholder1, @shareholder1SigningDate,
	@sponsoringShareholder2, @shareholder2SigningDate, @shareholderCertification, NULL)

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to record membership application',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO

CREATE PROCEDURE GetMembershipApplications(@startDate DATE, @endDate DATE)
AS
	SELECT * FROM MembershipApplication WHERE ApplicationDate BETWEEN @startDate AND @endDate
GO

CREATE PROCEDURE UpdateMembershipApplication(@lastName VARCHAR(25), @firstName VARCHAR(25), @applicationDate DATE, @applicationStatus VARCHAR(10))
AS
	BEGIN TRANSACTION
	UPDATE MembershipApplication SET ApplicationStatus = @applicationStatus WHERE LastName = @lastName AND FirstName = @firstName AND ApplicationDate = @applicationDate

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to update membership application',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO


CREATE PROCEDURE CreateMemberAccount(@membershipType VARCHAR(15), @lastName VARCHAR(25), @firstName VARCHAR(25), @email VARCHAR(48), @phoneNumber VARCHAR(25), 
@passwordHash VARCHAR(450), @securityStamp VARCHAR(450), @concurrencyStamp VARCHAR(450), @id VARCHAR(450))
AS
	BEGIN TRANSACTION
	INSERT INTO AspNetUsers VALUES(@id, @email, UPPER(@email), @email, UPPER(@email), 0, @passwordHash, LOWER(@securityStamp), LOWER(@concurrencyStamp), @phoneNumber, 0, 0, NULL, 1, 0, CONCAT(@firstName,' ',@lastName),
	(SELECT TOP 1 MemberNumber FROM AspNetUsers ORDER BY 1 DESC) + 1, 'Gold', @membershipType)

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to add member',16,1)
		RETURN 1
	END

	INSERT INTO AspNetUserRoles(UserId, RoleId) 
		SELECT @id, Id FROM AspNetRoles WHERE [Name] IN('Golfer', @membershipType)

	IF @@ROWCOUNT = 0 OR @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to add member to role ',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO


CREATE PROCEDURE AssessMembershipFees(@userId NVARCHAR(450), @feeDetails FeeDetails READONLY)
AS
	BEGIN TRANSACTION

	INSERT INTO AccountTransactions
	SELECT
		@userId,
		GETDATE(),
		NULL,
		Amount,
		Description,
		DueDate
	FROM
		@feeDetails

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to add membership fees',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO

CREATE PROCEDURE GetAccountDetail(@email VARCHAR(48), @fromDate DATE, @toDate DATE)
AS
	SELECT
		MemberName [Name],
		MembershipLevel,
		MembershipType,
		SUM(Amount) [Balance]
	FROM
		AspNetUsers INNER JOIN AccountTransactions ON UserId = Id
	WHERE
		Email = @email
	GROUP BY 
		MemberName,
		MembershipLevel,
		MembershipType

	SELECT
		TransactionDate,
		BookedDate,
		Amount,
		Description,
		DueDate
	FROM
		AccountTransactions INNER JOIN AspNetUsers ON UserId = Id
	WHERE
		CONVERT(DATE, TransactionDate) BETWEEN @fromDate AND @toDate
GO

CREATE PROCEDURE ViewAllAccountsSummary
AS
	SELECT
		MemberName [Name],
		Email,
		SUM(Amount) [Balance]
	FROM
		AspNetUsers INNER JOIN AccountTransactions ON AspNetUsers.Id = AccountTransactions.UserId
	GROUP BY
		MemberName,
		Email
GO

CREATE PROCEDURE RecordScores(@course VARCHAR(30), @rating DECIMAL(6,3), @slope DECIMAL(6,3), @date DATETIME, @email NVARCHAR(128), @holeByHoleScores AS HoleByHoleScores READONLY, @message VARCHAR(128) OUT)
AS
	BEGIN TRANSACTION

	DECLARE @userId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = @email)

	INSERT INTO ScoreCard(Course, Rating, Slope, Date, UserId)
	SELECT TOP 1
		@course,
		@rating,
		@slope,
		@date,
		@userId

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		SET @message = 'Unable to create scorecard for user with email ''' + @email + '''' 
		RAISERROR(@message,16,1)
		RETURN 1
	END

	INSERT INTO ScoreDetails(Date, UserId, Hole, Score)
	SELECT
		@date,
		@userId,
		ROW_NUMBER() OVER(ORDER BY @date),
		HoleScore
	FROM
		@holeByHoleScores

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		SET @message = 'Unable to add score details to scoreCard' 
		RAISERROR(@message,16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO

CREATE PROCEDURE UpdateHandicapReport(@email NVARCHAR(128), @message VARCHAR(128) OUT)
AS

	DECLARE @userId NVARCHAR(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE Email = @email)
	DECLARE @lifetimeRounds INT = (SELECT COUNT(*) FROM ScoreCard WHERE UserId = @userId)
	
	IF @lifetimeRounds < 5
	BEGIN
		SET @message = 'Cannot update handicap report at this time due to insufficient history'
		RAISERROR(@message,16,1)
		RETURN 1
	END 

	DECLARE @numberOfDifferentailsToUse INT = CASE
		WHEN @lifetimeRounds BETWEEN 5 AND 6 THEN 1
		WHEN @lifetimeRounds BETWEEN 7 AND 8 THEN 2
		WHEN @lifetimeRounds BETWEEN 9 AND 10 THEN 3
		WHEN @lifetimeRounds BETWEEN 11 AND 12 THEN 4
		WHEN @lifetimeRounds BETWEEN 13 AND 14 THEN 5
		WHEN @lifetimeRounds BETWEEN 15 AND 16 THEN 6
		WHEN @lifetimeRounds = 17 THEN 8
		WHEN @lifetimeRounds = 19 THEN 9
		ELSE 10
		END

	SELECT
	ROUND(([Score Per Day] - Rating) * 113.0 / Slope, 1) AS 'HandicapDifferential' INTO #HandicapDifferentials
	FROM
		(SELECT DISTINCT TOP 20
			ScoreDetails.Date,
			CAST(SUM(Score) OVER(PARTITION BY ScoreDetails.Date) AS DECIMAL) AS [Score Per Day]
		FROM
			ScoreDetails
		WHERE
			ScoreDetails.UserId = @userId
	) AS [Scores]
	INNER JOIN ScoreCard ON Scores.Date = ScoreCard.Date AND ScoreCard.UserId = @userId
	ORDER BY ScoreCard.Date DESC

	IF NOT EXISTS(SELECT * FROM HandicapReport INNER JOIN AspNetUsers ON UserId = Id WHERE Email = @email AND Year = DATEPART(YEAR, GETDATE()) AND Month = DATENAME(MONTH, GETDATE()))
	BEGIN
		INSERT INTO HandicapReport(UserId, LastUpdated, HandicapFactor, Average, BestOfTenAverage)
			SELECT
				@userId,
				GETDATE(),
				(SELECT ROUND(AVG(HandicapDifferential) * 0.96,1,1) FROM (SELECT TOP (@numberOfDifferentailsToUse) HandicapDifferential FROM #HandicapDifferentials ORDER BY HandicapDifferential ASC) [TopDifferentials]),
				(SELECT ROUND(AVG(HandicapDifferential * 1.0),1) FROM #HandicapDifferentials),
				(SELECT ROUND(AVG(HandicapDifferential * 1.0),1) FROM (SELECT TOP (@numberOfDifferentailsToUse) HandicapDifferential FROM #HandicapDifferentials ORDER BY HandicapDifferential ASC) [TopDifferentails])
	END
	ELSE
	BEGIN
		DECLARE @lastUpdated AS DATETIME = (SELECT TOP 1 LastUpdated FROM HandicapReport WHERE UserId = @userId ORDER BY LastUpdated DESC)
		UPDATE HandicapReport
			SET LastUpdated = GETDATE(),
			HandicapFactor = (SELECT ROUND(AVG(HandicapDifferential * 0.96),1,1) FROM (SELECT TOP (@numberOfDifferentailsToUse) HandicapDifferential FROM #HandicapDifferentials ORDER BY HandicapDifferential ASC) [TopDifferentials]),
			Average = (SELECT ROUND(AVG(HandicapDifferential * 1.0),1) FROM #HandicapDifferentials),
			BestOfTenAverage = (SELECT ROUND(AVG(HandicapDifferential * 1.0),1) FROM (SELECT TOP (@numberOfDifferentailsToUse) HandicapDifferential FROM #HandicapDifferentials ORDER BY HandicapDifferential ASC) [TopDifferentails])
		WHERE
			UserId = @userId AND LastUpdated = @lastUpdated
	END

	IF @@ERROR <> 0 
	RETURN 1

	RETURN 0
GO


DECLARE @holeByHoleScores AS HoleByHoleScores
DECLARE @message AS VARCHAR(123)

--INSERT INTO @holeByHoleScores VALUES(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4),(4)

--EXEC RecordScores 'Club BAIST', 70.6, 128, 'April 29, 2020 17:00', 'shareholder1@test.com', @holeByHoleScores, @message OUT

--SELECT * FROM AspNetUsers

EXEC UpdateHandicapReport 'shareholder1@test.com', @message OUT


SELECT SUM(Score) FROM ScoreDetails WHERE Date = '18-Feb-20 09:52:57' AND UserId = (SELECT TOP 1 UserId FROM AspNetUsers WHERE Email = 'shareholder1@test.com')

declare @p6 dbo.HoleByHoleScores
insert into @p6 values(N'5')
insert into @p6 values(N'7')
insert into @p6 values(N'6')
insert into @p6 values(N'7')
insert into @p6 values(N'4')
insert into @p6 values(N'5')
insert into @p6 values(N'6')
insert into @p6 values(N'3')
insert into @p6 values(N'4')
insert into @p6 values(N'5')
insert into @p6 values(N'7')
insert into @p6 values(N'6')
insert into @p6 values(N'4')
insert into @p6 values(N'5')
insert into @p6 values(N'7')
insert into @p6 values(N'5')
insert into @p6 values(N'6')
insert into @p6 values(N'4')

declare @p7 nvarchar(1)
set @p7=NULL
exec RecordScores @course=N'Club BAIST',@rating=70.599999999999994,@slope=128,@date='2020-02-18 09:52:57.517',@email=N'shareholder1@test.com',@holeByHoleScores=@p6,@message=@p7 output

ALTER TABLE ScoreDetails DROP CONSTRAINT FK_ScoreDetails_DateId ALTER TABLE ScoreDetails ADD CONSTRAINT FK_ScoreDetails_DateId FOREIGN KEY (Date,UserId) REFERENCES ScoreCard(Date,UserId) ON DELETE CASCADE DELETE ScoreCard WHERE Date IN('18-Feb-20 09:52:57.517') AND UserId IN(SELECT UserId FROM AspNetUsers WHERE Email IN('shareholder1@test.com')) ALTER TABLE ScoreDetails ADD CONSTRAINT FK_ScoreDetails_DateId FOREIGN KEY (Date,UserId) REFERENCES ScoreCard(Date,UserId)

SELECT * FROM ScoreDetails WHERE CONVERT(VARCHAR(24),Date,120) = '2020-02-22 00:00:00'

SELECT CONVERT(VARCHAR(24),Date,120) FROM ScoreDetails