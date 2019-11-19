use CBS
GO

INSERT INTO MembershipLevels(MembershipLevel) VALUES('Gold')

INSERT INTO MembershipLevels(MembershipLevel) VALUES('Silver')

INSERT INTO MembershipLevels(MembershipLevel) VALUES('Bronze')

INSERT INTO Members VALUES ('1234567890','John Doe', 'Gold', 1), ('1029384756','Jane Doe', 'Silver', 0), ('1092837465','Mohammed Lee', 'Bronze', 0), ('483814079','Bronze Member', 'Bronze', 0)

DECLARE @golfers AS dbo.GolferList
INSERT INTO @golfers VALUES('1234567890'),('1029384756'),('1092837465'),('483814079')
DECLARE @message AS VARCHAR(1024)

EXEC ReserveTeeTime 'November 19, 2019', '15:37', 2, '7804569335', @golfers, @message out
SELECT @message

SELECT 
	PermissableTeeTimes.Time, 
	NumberOfCarts, 
	TeeTimes.Phone,
	Date
FROM 
	TeeTimes RIGHT OUTER JOIN PermissableTeeTimes ON 
	TeeTimes.Time = PermissableTeeTimes.Time AND PermissableTeeTimes.Time = 'November 19 2019'

SELECT
		[Time]
	FROM
		TeeTimesForMembershipLevel INNER JOIN Members ON
		TeeTimesForMembershipLevel.MembershipLevel = Members.MembershipLevel
	WHERE
		Members.MemberNumber = '1029384756' AND DayOfWeek = 'Weekend'
SELECT * FROM AspNetUsers
--SELECT @message

--EXEC FindDailyTeeSheet 'November 19 2019'

EXEC RequestStandingTeeTime 'May 20, 2020', 'June 24, 2020', '07:15', @message out, @golfers

--EXEC ViewStandingTeeTimeRequests 'Wednesday'
select @message

SELECT TOP 1 MemberName FROM Members WHERE Members.MemberNumber = TeeTimeGolfers.MemberNumber [Member Name]
SELECT * FROM TeeTimeGolfers
--DECLARE @current AS TIME = '07:00'
--WHILE DATEDIFF(HOUR, @current, '19:00') > 0
--BEGIN
--	IF DATEPART(MINUTE, @current) IN (0,7,15,22,30,37,45,52)
--		INSERT INTO PermissableTeeTimes VALUES(@current)
--	SET @current = DATEADD(MINUTE,1, @current)
--END

--SELECT * FROM PermissableTeeTimes
--SELECT * FROM TeeTimesForMembershipLevel WHERE MembershipLevel = 'Silver' AND DayOfWeek = 'Weekend'

INSERT INTO TeeTimesForMembershipLevel(Time, DayOfWeek, MembershipLevel)
	SELECT Time, 'Weekend', 'Gold' FROM PermissableTeeTimes

INSERT INTO TeeTimesForMembershipLevel(Time, DayOfWeek, MembershipLevel)
	SELECT Time, 'Weekday', 'Silver' FROM PermissableTeeTimes
	WHERE
		(DATEDIFF(MINUTE, Time, '15:00') > 0) OR (DATEDIFF(MINUTE, Time, '17:30') < 0)

INSERT INTO TeeTimesForMembershipLevel(Time, DayOfWeek, MembershipLevel)
	SELECT Time, 'Weekend', 'Silver' FROM PermissableTeeTimes
	WHERE
		DATEDIFF(MINUTE, Time, '11:00') < 0

INSERT INTO TeeTimesForMembershipLevel(Time, DayOfWeek, MembershipLevel)
	SELECT Time, 'Weekday', 'Bronze' FROM PermissableTeeTimes
	WHERE
		(DATEDIFF(MINUTE, Time, '15:00') > 0) OR (DATEDIFF(MINUTE, Time, '18:00') < 0)

INSERT INTO TeeTimesForMembershipLevel(Time, DayOfWeek, MembershipLevel)
	SELECT Time, 'Weekend', 'Bronze' FROM PermissableTeeTimes
	WHERE
		DATEDIFF(MINUTE, Time, '13:00') < 0


SELECT
		LEFT(CONVERT(NVARCHAR,PermissableTeeTimes.Time, 24),5) [Time],
		TeeTimes.NumberOfCarts,
		TeeTimes.Phone,
		(SELECT TOP 1 MemberName FROM Members WHERE Members.MemberNumber = TeeTimeGolfers.MemberNumber) [Member Name]
	FROM
		PermissableTeeTimes LEFT OUTER JOIN TeeTimes ON
		PermissableTeeTimes.Time = TeeTimes.Time LEFT OUTER JOIN Members ON
		TeeTimeGolfers.Date = TeeTimes.Date = 'November 19 2020'

--DECLARE @golfers AS dbo.GolferList
--INSERT INTO @golfers VALUES('1234567890'),('1029384756'),('1092837465'),('483814079')

--DECLARE @conflictingMembers VARCHAR(64)
	
--SELECT @conflictingMembers = COALESCE(@conflictingMembers + ', ', '') + MemberName
--					FROM
--						Members
--					WHERE MembershipLevel = 'Bronze' AND MemberNumber IN(SELECT * FROM @golfers)

--SELECT @conflictingMembers