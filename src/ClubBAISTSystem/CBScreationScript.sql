use master

--CREATE DATABASE CBS
--go

USE CBS

--SELECT * FROM SYS.tables

--DROP DATABASE CBS
--ALTER TABLE AspNetUsers
--	DROP CONSTRAINT FK__AspNetUse__Membe__07420643

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'StandingTeeTimeGolfers')
	DROP TABLE StandingTeeTimeGolfers
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'TeeTimeGolfers')
	DROP TABLE TeeTimeGolfers
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'TeeTimesForMembershipLevel')
	DROP TABLE TeeTimesForMembershipLevel
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'Members')
	DROP TABLE Members
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'TeeTimes')
	DROP TABLE TeeTimes
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'MembershipLevels')
	DROP TABLE MembershipLevels
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'StandingTeeTimeRequests')
	DROP TABLE StandingTeeTimeRequests
GO

IF EXISTS(SELECT * FROM sys.tables WHERE name LIKE 'PermissableTeeTimes')
	DROP TABLE PermissableTeeTimes
GO

CREATE TABLE PermissableTeeTimes
(
	[Time] TIME PRIMARY KEY
)
GO

--select * from sys.tables


ALTER TABLE TeeTimes
	ADD FOREIGN KEY ([Time]) REFERENCES PermissableTeeTimes([Time])


CREATE TABLE TeeTimes
(
	[Date] DATE NOT NULL,
	[Time] TIME,
	Phone VARCHAR(14) NOT NULL,
	NumberOfCarts TINYINT NULL,
	PRIMARY KEY([Date],[Time]),
	FOREIGN KEY ([Time]) REFERENCES PermissableTeeTimes([Time]) 
)
GO

CREATE TABLE MembershipLevels
(
	MembershipLevel CHAR(6) PRIMARY KEY,
)
GO

--sp_help TeeTimesForMembershipLevel
CREATE TABLE TeeTimesForMembershipLevel
(
	[Time] Time,
	[DayOfWeek] CHAR(9),
	MembershipLevel CHAR(6) NOT NULL,
	PRIMARY KEY([Time],[DayOfWeek],MembershipLevel),
	FOREIGN KEY (MembershipLevel) REFERENCES MembershipLevels(MembershipLevel),
	FOREIGN KEY ([Time]) REFERENCES PermissableTeeTimes([Time])
)

CREATE TABLE Members
(
	MemberNumber CHAR(10) PRIMARY KEY,
	MemberName VARCHAR(40) NOT NULL,
	MembershipLevel CHAR(6) NOT NULL,
	IsShareholder TINYINT CHECK (IsShareholder IN (0,1)) NOT NULL,
	FOREIGN KEY(MembershipLevel) REFERENCES MembershipLevels(MembershipLevel)
)
GO


CREATE TABLE TeeTimeGolfers
(
	[Date] DATE NOT NULL,
	[Time] TIME,
	MemberNumber CHAR(10) NOT NULL,
	FOREIGN KEY([Date],[Time]) REFERENCES TeeTimes([Date],[Time]),
	FOREIGN KEY(MemberNumber) REFERENCES Members(MemberNumber)
)
GO

CREATE TABLE StandingTeeTimeRequests
(
	ID INT IDENTITY PRIMARY KEY,
	StartDate DATE NOT NULL,
	EndDate DATE NOT NULL,
	RequestedTime TIME,
	[DayOfWeek] AS DATENAME(WEEKDAY, StartDate),
	CONSTRAINT ck_WeekdayMatch CHECK(DATEPART(WEEKDAY, StartDate) = DATEPART(WEEKDAY, EndDate)),
	UNIQUE(StartDate,EndDate,RequestedTime),
	FOREIGN KEY (RequestedTime) REFERENCES PermissableTeeTimes([Time])
)
GO

CREATE TABLE StandingTeeTimeGolfers
(
	ID INT NOT NULL,
	MemberNumber CHAR(10) NOT NULL,
	PRIMARY KEY(ID,MemberNumber),
	FOREIGN KEY(ID) REFERENCES StandingTeeTimeRequests(ID),
	FOREIGN KEY(MemberNumber) REFERENCES Members(MemberNumber)
)
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE name LIKE 'FindDailyTeeSheet')
	DROP PROCEDURE FindDailyTeeSheet
GO

CREATE PROCEDURE FindDailyTeeSheet(@date DATE)
AS
	SELECT
		LEFT(CONVERT(NVARCHAR,PermissableTeeTimes.Time, 24),5) [Time],
		TeeTimes.NumberOfCarts,
		TeeTimes.Phone,
		(SELECT TOP 1 MemberName FROM Members WHERE TeeTimeGolfers.MemberNumber = Members.MemberNumber) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN TeeTimes ON
		PermissableTeeTimes.Time = TeeTimes.Time LEFT OUTER JOIN
		TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Date = @date
GO


IF EXISTS(SELECT * FROM SYSOBJECTS WHERE name LIKE 'ReserveTeeTime')
	DROP PROCEDURE ReserveTeeTime
GO

CREATE TYPE dbo.GolferList AS TABLE
(
	MemberNumber VARCHAR(40) 
)
GO

CREATE PROCEDURE ReserveTeeTime
@date DATE, @time TIME, @numberOfCarts INT, @phone VARCHAR(14), @golfers AS dbo.GolferList READONLY, @message VARCHAR(128) OUT
AS
SET NOCOUNT ON
BEGIN
	DECLARE @returnCode AS INT = 0

	--Verify that the selected tt time does not conflict with any member's membership level

	SELECT DISTINCT
		MembershipLevel INTO #membership_levels
	FROM
		MembershipLevels

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
				DECLARE @conflictingMembers VARCHAR(64)
					SELECT @conflictingMembers = COALESCE(@conflictingMembers + ', ', '') + MemberName
					FROM
					@golfers G CROSS APPLY(SELECT MemberName FROM Members INNER JOIN @golfers ON Members.MemberNumber = G.MemberNumber WHERE Members.MembershipLevel = @membershipLevel) MemberNumber

				SET @message = 'Selected tee time: ' + CAST(@time AS nvarchar(8)) + ' conflicts with membership level: ' + @membershipLevel + ' for members ' + @conflictingMembers
				RETURN 1
				CLOSE cursor_membershipLevels
				DEALLOCATE cursor_membership_levels
				BREAK
			END
	END

	CLOSE cursor_membershipLevels
	DEALLOCATE cursor_membershipLevels
	
	IF @returnCode = 1
		

	BEGIN TRANSACTION
	INSERT INTO TeeTimes(Date,Time,NumberOfCarts, Phone) VALUES(@date,@time,@numberOfCarts, @phone)

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to reserve tee time ' + CAST(@time AS nvarchar(8)) + ' for day ' + CAST(@date AS NVARCHAR(8))
		SET @returnCode = 1
		ROLLBACK TRANSACTION
		RETURN @returnCode
	END

	INSERT INTO TeeTimeGolfers(Date, Time, MemberNumber)
		SELECT
			@date,
			@time,
			MemberNumber
		FROM
			@golfers

	IF @@ERROR <> 0
	BEGIN
		SET @message = 'Unable to add golfers to tee time for time: ' + CAST(@time AS NVARCHAR(8)) + ' on day: ' + CAST(@date AS NVARCHAR(8))
		SET @returnCode = 1
		ROLLBACK TRANSACTION
		RETURN @returnCode
	END

	COMMIT TRANSACTION
	RETURN @returnCode
END
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [Name] LIKE 'GetPermittedTeeTimes')
	DROP PROCEDURE GetPermittedTeeTimes
GO

CREATE PROCEDURE GetPermittedTeeTimes(@memberNumber VARCHAR(10), @dayOfWeek CHAR(10))
AS
	SELECT
		[Time]
	FROM
		TeeTimesForMembershipLevel INNER JOIN Members ON
		TeeTimesForMembershipLevel.MembershipLevel = Members.MembershipLevel
	WHERE
		Members.MemberNumber = @memberNumber AND DayOfWeek = @dayOfWeek

GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [Name] LIKE 'ViewStandingTeeTimeRequests')
	DROP PROCEDURE ViewStandingTeeTimeRequests
GO

CREATE PROCEDURE ViewStandingTeeTimeRequests(@dayOfWeek VARCHAR(9))
AS
	SELECT
		LEFT(CONVERT(NVARCHAR,RequestedTime,24),5)
	FROM
		StandingTeeTimeRequests
	WHERE
		[DayOfWeek] = @dayOfWeek
		AND DATEDIFF(DAY, GETDATE() ,EndDate) >= 0
GO

IF EXISTS(SELECT * FROM SYSOBJECTS WHERE [Name] LIKE 'RequestStandingTeeTime')
	DROP PROCEDURE RequestStandingTeeTime
GO

CREATE PROCEDURE RequestStandingTeeTime(@startDate DATE, @endDate DATE, @requestedTime TIME, @message VARCHAR(64) out, @memberNumbers AS GolferList READONLY)
AS
	BEGIN TRANSACTION

	INSERT INTO StandingTeeTimeRequests(StartDate,EndDate,RequestedTime)
	VALUES(@startDate, @endDate,@requestedTime)

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		SET @message = 'Unable to create a standing tee time request for start date ' + CAST(@startDate AS NVARCHAR(15))
		RETURN 1
	END

	DECLARE @standingTeeTimeID as INT = @@IDENTITY

	INSERT INTO StandingTeeTimeGolfers(ID, MemberNumber)
		SELECT
			@standingTeeTimeID, MemberNumber
		FROM
			@memberNumbers
	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		SET @message = 'Unable to add golfers to standing tee time on start date ' + CAST(@startDate AS NVARCHAR(15))
		RETURN 1
	END

	COMMIT TRANSACTION
	RETURN 0
GO

IF EXISTS (SELECT * FROM SYSOBJECTS WHERE [Name] LIKE 'MemberExists')
	DROP PROCEDURE MemberExists
GO

CREATE PROCEDURE MemberExists(@memberNumber VARCHAR(10))
AS
	IF EXISTS(SELECT * FROM Members WHERE MemberNumber = @memberNumber)
		RETURN 0
	RETURN 1
GO


