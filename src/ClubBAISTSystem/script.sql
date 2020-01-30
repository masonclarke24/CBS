USE CBS
GO

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

IF EXISTS(SELECT * FROM SYS.TYPES WHERE [name] LIKE 'GolferList')
	DROP TYPE GolferList
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
	MembershipType VARCHAR(15) NOT NULL,
	Occupation VARCHAR(25) NOT NULL,
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
	ShareholderCertification BIT NOT NULL
)
GO

CREATE TYPE GolferList AS TABLE
(
	UserId NVARCHAR(450) NOT NULL
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
