If not exists (select * from sys.tables WHERE name = 'Version')
BEGIN
    CREATE TABLE dbo.Version (
        VersionNumber nvarchar(20) NOT NULL
    )
END
GO

If not exists (select * from sys.tables WHERE name = 'Scripts')
BEGIN
    CREATE TABLE dbo.Scripts (
        Id int IDENTITY PRIMARY KEY,
        ScriptId nvarchar(150) NOT NULL,
        Name nvarchar(300) NOT NULL,
        Description nvarchar(300) NOT NULL,
        LastChanged DateTime NOT NULL,
        Features int NOT NULL,
        CustomResultsRange nvarchar(max),
        Actions nvarchar(max),
        PollingFrequency bigint,
        PositionX int,
        PositionY int,
        PositionWidth int,
        PositionHeight int,
		CONSTRAINT UKEY_script UNIQUE (ScriptId)
    )
END
GO

If not exists (select * from sys.tables WHERE name = 'Widgets')
BEGIN
    CREATE TABLE dbo.Widgets (
        Id int IDENTITY NOT NULL PRIMARY KEY,
        ScriptId int NOT NULL
            FOREIGN KEY REFERENCES dbo.Scripts(Id),
        WidgetName nvarchar(150) NOT NULL,
        LastChanged DateTime NOT NULL,
        ConfigData nvarchar(max),
        ConfigUI nvarchar(max),
		CONSTRAINT UKEY_widget UNIQUE(ScriptId, WidgetName)
    )
END
GO

If not exists (select * from sys.tables WHERE name = 'Results')
BEGIN
    CREATE TABLE dbo.Results (
        Id int IDENTITY NOT NULL PRIMARY KEY,
        ScriptId int NOT NULL
            FOREIGN KEY REFERENCES dbo.Scripts(Id),
        Created DateTime NOT NULL,
        RawValue nvarchar(max),
		CONSTRAINT UKEY_results UNIQUE(ScriptId, Created)
    )
END
GO

CREATE PROCEDURE DeleteScript
@ScriptId nvarchar(150)
AS
BEGIN
    DELETE FROM dbo.Results WHERE ScriptId = '@ScriptId';
	DELETE FROM dbo.Widgets WHERE ScriptId = '@ScriptId';
	DELETE FROM dbo.Scripts WHERE ScriptId = '@ScriptId'
END
GO

CREATE PROCEDURE SaveScript
@ScriptId nvarchar(150),
@Name nvarchar(300),
@Description nvarchar(300),
@LastChanged datetime, 
@Features int, 
@CustomResultsRange nvarchar(max),
@Actions nvarchar(max), 
@PollingFrequency bigint,
@PositionX smallint, 
@PositionY smallint, 
@PositionWidth smallint, 
@PositionHeight smallint
AS
BEGIN
	IF NOT EXISTS (SELECT Id FROM dbo.Scripts WHERE ScriptId = @ScriptId)
		INSERT INTO dbo.Scripts
		(ScriptId, Name, Description, LastChanged, Features, CustomResultsRange, Actions, PollingFrequency, PositionX, PositionY, PositionWidth, PositionHeight)
		values (@ScriptId, @Name, @Description, @LastChanged, @Features, @CustomResultsRange, @Actions, @PollingFrequency, @PositionX, @PositionY, @PositionWidth, @PositionHeight)
	ELSE
		UPDATE dbo.Scripts
		SET ScriptId = @ScriptId,
		Name = @Name,
		Description = @Description,
		LastChanged = @LastChanged, 
		Features = @Features, 
		CustomResultsRange = @CustomResultsRange,
		Actions = @Actions, 
		PollingFrequency = @PollingFrequency,
		PositionX = @PositionX, 
		PositionY = @PositionY, 
		PositionWidth = @PositionWidth, 
		PositionHeight = @PositionHeight
END
GO

CREATE PROCEDURE DeleteWidget
@ScriptId nvarchar(150)
AS
BEGIN
	DELETE FROM Widgets WHERE ScriptId = @ScriptId
END
GO

CREATE PROCEDURE SaveWidget
@ScriptId nvarchar(150),
@WidgetName nvarchar(150),
@LastChanged datetime,
@ConfigData nvarchar(max),
@ConfigUI nvarchar(max)
AS
BEGIN
	IF(NOT EXISTS (SELECT Widgets.Id 
					FROM Widgets 
					INNER JOIN Scripts ON Widgets.ScriptId = Scripts.Id 
					WHERE Scripts.ScriptId = @ScriptId AND WidgetName = @WidgetName))
		INSERT INTO Widgets
		(ScriptId,WidgetName,LastChanged,ConfigData,ConfigUI)
		SELECT Scripts.Id, @WidgetName, @LastChanged, @ConfigData, @ConfigUI
        FROM Scripts 
        WHERE ScriptId = @ScriptId
	ELSE
		UPDATE Widgets
		set LastChanged = @LastChanged,
		ConfigData = @ConfigData,
		ConfigUI = @ConfigUI
        FROM Widgets 
        INNER JOIN Scripts ON Widgets.ScriptId = Scripts.Id
        WHERE Scripts.ScriptId = @ScriptId AND Widgets.WidgetName = @WidgetName
END

CREATE PROCEDURE DeleteResults
@ScriptId nvarchar(150),
@Created DateTime = NULL
AS
BEGIN
    DELETE FROM Results
    FROM Results
    INNER JOIN Scripts ON Scripts.Id = Results.ScriptId
    WHERE Scripts.ScriptId = @ScriptId AND 
    (
        (@Created IS NULL) OR
        (@Created = Results.Created)
    )
END

CREATE PROCEDURE SaveResult
@ScriptId nvarchar(150),
@Created DateTime,
@RawValue nvarchar(max)
AS
BEGIN
    IF not exists (SELECT r.ID FROM Results r INNER JOIN Scripts s on r.ScriptId = s.Id WHERE r.Created = @Created AND s.ScriptId = @ScriptId)
		INSERT INTO Results
		(ScriptId, Created, RawValue)
		SELECT s.Id, @Created, @RawValue
		FROM Scripts s
		WHERE s.ScriptId = @ScriptId
	ELSE
		Update Results
		Set RawValue = @RawValue
		from Results 
		INNER JOIN Scripts ON Results.ScriptId = Scripts.Id
		WHERE Scripts.ScriptId = @ScriptId AND Results.Created = @Created
END