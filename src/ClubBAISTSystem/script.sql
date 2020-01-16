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
	[Time] TIME CONSTRAINT FK_TeeTimes_Time FOREIGN KEY REFERENCES PermissableTeeTimes([Time]),
	Phone VARCHAR(14) NOT NULL,
	NumberOfCarts TINYINT NOT NULL,
	CONSTRAINT PK_TeeTimes_DateTime PRIMARY KEY ([Date],[Time])
)
GO

CREATE TABLE TeeTimeGolfers
(
	[Date] DATE NOT NULL,
	[Time] Time NOT NULL,
	MemberNumber NVARCHAR(450) NOT NULL CONSTRAINT FK_TeeTimeGolfers_MemberNumber FOREIGN KEY REFERENCES AspNetUsers(Id),
	CONSTRAINT FK_TeeTimeGolfers_DateTime FOREIGN KEY ([Date],[Time]) REFERENCES TeeTimes([Date],[Time]),
	CONSTRAINT PK_TeeTimeGolfers_DateTimeMemNumber PRIMARY KEY ([Date],[Time],MemberNumber)
)
GO

CREATE TABLE StandingTeeTimeRequests
(
	ID INT IDENTITY PRIMARY KEY,
	StartDate DATE,
	EndDate DATE,
	RequestedTime TIME CONSTRAINT FK_STTR_RequestedTime FOREIGN KEY REFERENCES PermissableTeeTimes([Time]),
	[DayOfWeek] AS DATENAME(DW, StartDate),
	CONSTRAINT CHK_STTR_StartDate CHECK (DATEPART(DW, StartDate) = DATEPART(DW, EndDate)),
	CONSTRAINT CHK_STTR_EndDate CHECK (DATEDIFF(DAY, StartDate, EndDate) > 0)
)
GO

CREATE TABLE StandingTeeTimeGolfers
(
	ID INT CONSTRAINT FK_STTGolfers_ID FOREIGN KEY REFERENCES StandingTeeTimeRequests(ID),
	MemberNumber NVARCHAR(450) CONSTRAINT FK_STTGolfers_MemberNumber FOREIGN KEY REFERENCES AspNetUsers(Id),
	PRIMARY KEY (ID,MemberNumber)
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

CREATE TYPE GolferList AS TABLE
(
	MemberNumber NVARCHAR(450) NOT NULL
)
GO


CREATE PROCEDURE FindDailyTeeSheet(@date DATE)
AS
	SELECT
		LEFT(CONVERT(NVARCHAR,PermissableTeeTimes.Time, 24),5) [Time],
		TeeTimes.NumberOfCarts,
		TeeTimes.Phone,
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE TeeTimeGolfers.MemberNumber = AspNetUsers.Id) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN TeeTimes ON
		PermissableTeeTimes.Time = TeeTimes.Time LEFT OUTER JOIN
		TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Date = @date AND TeeTimeGolfers.Time = TeeTimes.Time
GO

CREATE PROCEDURE [dbo].[GetPermittedTeeTimes](@memberNumber NVARCHAR(450), @dayOfWeek INT)
AS
	SELECT
		[Time]
	FROM
		TeeTimesForMembershipLevel
	WHERE
		MembershipLevel = (SELECT TOP 1 MembershipLevel FROM AspNetUsers WHERE Id = @memberNumber) AND [DayOfWeek] = @dayOfWeek
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
	--Verify that the selected tee time does not conflict with any member's membership level

	SELECT DISTINCT
		MembershipLevels.MembershipLevel INTO #membership_levels
	FROM
		MembershipLevels INNER JOIN AspNetUsers ON MembershipLevels.MembershipLevel = AspNetUsers.MembershipLevel
	WHERE
		AspNetUsers.Id IN(SELECT * FROM @golfers)

	DECLARE cursor_membershipLevels CURSOR  
	FOR SELECT
		MembershipLevel
	FROM
		#membership_levels

	DECLARE @membershipLevel VARCHAR(9)
	DECLARE @dayOfWeek INT = DATEPART(weekday, @date)

	OPEN cursor_membershipLevels

	FETCH NEXT FROM cursor_membershipLevels INTO @membershipLevel
	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS(SELECT * FROM TeeTimesForMembershipLevel WHERE MembershipLevel = @membershipLevel AND DayOfWeek = @dayOfWeek AND [Time] = @time)
			BEGIN
				DECLARE @conflictingMembers VARCHAR(512)
					SELECT @conflictingMembers = COALESCE(@conflictingMembers + ', ', '') + MemberName
					FROM
						AspNetUsers
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


CREATE PROCEDURE [dbo].[ViewStandingTeeTimeRequests](@startDate date, @endDate date)
AS
	DECLARE @dayOfWeek VARCHAR(9) = DATENAME(WEEKDAY,@startDate)
	SELECT
		LEFT(CONVERT(NVARCHAR,[Time],24),5) [Requested Time],
		StandingTeeTimeRequests.StartDate [Start Date],
		StandingTeeTimeRequests.EndDate [End Date],
		[DayOfWeek] [Day of Week],
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE StandingTeeTimeGolfers.MemberNumber = AspNetUsers.Id) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN StandingTeeTimeRequests ON
		PermissableTeeTimes.Time = StandingTeeTimeRequests.RequestedTime AND (StandingTeeTimeRequests.StartDate BETWEEN @startDate AND @endDate) AND StandingTeeTimeRequests.DayOfWeek = @dayOfWeek
		LEFT OUTER JOIN StandingTeeTimeGolfers ON StandingTeeTimeRequests.ID = StandingTeeTimeGolfers.ID
GO



CREATE PROCEDURE FindReservedTeeTimes(@userID VARCHAR(450))
AS
	SELECT
		TeeTimes.[Date],
		TeeTimes.[Time]
	INTO
		#ReservedTeeTimes
	FROM 
		TeeTimes INNER JOIN TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Time = TeeTimeGolfers.Time
	WHERE 
		TeeTimeGolfers.MemberNumber = @userID

	SELECT
		TeeTimes.[Date],
		TeeTimes.[Time],
		NumberOfCarts,
		Phone,
		(SELECT TOP 1 MemberName FROM AspNetUsers WHERE TeeTimeGolfers.MemberNumber = AspNetUsers.Id) [Member Name]
	FROM
		TeeTimes INNER JOIN TeeTimeGolfers ON TeeTimes.Date = TeeTimeGolfers.Date AND TeeTimes.Time = TeeTimeGolfers.Time
	WHERE
		TeeTimes.[Date] IN(SELECT [Date] FROM #ReservedTeeTimes) AND TeeTimes.[Time] IN(SELECT [Time] FROM #ReservedTeeTimes)
GO

CREATE PROCEDURE CancelTeeTime(@date DATE, @time TIME, @userID VARCHAR(450))
AS
	BEGIN TRANSACTION
	
	DELETE TeeTimeGolfers WHERE [Date] = @date AND [Time] = @time AND MemberNumber = @userID
	DECLARE @rowCount INT = @@ROWCOUNT

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to remove golfer from tee time',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION
	BEGIN TRANSACTION

	IF NOT EXISTS(SELECT * FROM TeeTimeGolfers WHERE [Date] = @date AND [Time] = @time)
	BEGIN
		DELETE TeeTimes WHERE [Date] = @date AND [Time] = @time
	END

	IF @@ERROR <> 0
	BEGIN
		ROLLBACK TRANSACTION
		RAISERROR('Unable to cancel from tee time',16,1)
		RETURN 1
	END

	COMMIT TRANSACTION

	IF @rowCount <> 0
		RETURN 0
	RETURN 1
GO
--SELECT * FROM AspNetUsers

--EXEC FindDailyTeeSheet 'January 22, 2020'

--SELECT MemberName FROM AspNetUsers WHERE Id = '52f66411-7e4e-4773-916c-354da9a05ee7'

--UPDATE AspNetUsers SET MemberNumber = 2 WHERE Id = '9d13c967-8c80-460b-bb13-22d8666b3de7'
--EXEC FindReservedTeeTimes '9d13c967-8c80-460b-bb13-22d8666b3de7'
--GO

EXEC CancelTeeTime '1/22/2020', '7:00:00 AM', '51a35064-6572-455e-9ae6-ae58774e9f39'
SELECT @@ROWCOUNT

SELECT [Date], [Time], MemberNumber FROM TeeTimeGolfers WHERE [Date] = '1/22/2020' AND [Time] = '7:00:00 AM' AND MemberNumber = '51a35064-6572-455e-9ae6-ae58774e9f39'