CREATE OR ALTER PROC Proc_DeleteRefreshTokenExpired
AS
BEGIN
	DECLARE @ParentToken NVARCHAR(21);
	DECLARE @RowCount INT;
	DECLARE @Tokens TABLE 
	(
		ParentToken NVARCHAR(21)
	);

	INSERT INTO @Tokens(ParentToken)
    SELECT Token FROM RefreshTokens
    WHERE IsExpiredAt < GETDATE() AND ParentToken IS NULL;
	
	SELECT @RowCount = COUNT(*) FROM @Tokens;

	SELECT TOP 1 @ParentToken = ParentToken FROM @Tokens;

	WHILE @RowCount > 0
    BEGIN
		DELETE FROM RefreshTokens WHERE ParentToken = @ParentToken;
		DELETE FROM RefreshTokens WHERE Token = @ParentToken;

		DELETE FROM @Tokens WHERE ParentToken = @ParentToken;
		SET @RowCount = @RowCount - 1;

		SELECT TOP 1 @ParentToken = ParentToken FROM @Tokens;
	END
END
