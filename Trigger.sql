CREATE OR ALTER TRIGGER TR_Insert_Update_Delete_ParentAndChildrenWhenChildrendIsExpiredAndIsUsedTrue
ON RefreshTokens
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	DECLARE @ParentId NVARCHAR(21);

	DECLARE @RowCount INT;
	
	DECLARE @Tokens TABLE(
		ParentId NVARCHAR(21)
	)
	-- Lấy những ParentID mà có con bị hết hạn và còn dùng được -> người dùng không đăng xuất
	INSERT INTO @Tokens(ParentId) SELECT ParentId FROM RefreshTokens WHERE IsExpiredAt < GETDATE() AND ParentId IS NOT NULL AND IsUsed = 1;

	SELECT @RowCount = COUNT(*) FROM @Tokens;

	SELECT TOP 1 @ParentId = ParentId FROM @Tokens;


	WHILE @RowCount > 0
	BEGIN

		DELETE FROM RefreshTokens WHERE ParentId = @ParentId;
		DELETE FROM RefreshTokens WHERE Id = @ParentId
		DELETE FROM @Tokens WHERE ParentId = @ParentId;
		

		SET @RowCount = @RowCount - 1;
		-- Duyệt qua lần cuối -> Xóa Cha

		SELECT TOP 1 @ParentId = ParentId FROM @Tokens;
	END
END

CREATE OR ALTER TRIGGER TR_Insert_Update_Delete_RefreshTokenIsExpiredAndIsUsedTrue
ON RefreshTokens
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	DECLARE @Id NVARCHAR(21);

	DECLARE @RowCount INT;
	
	DECLARE @Tokens TABLE(
		Id NVARCHAR(21)
	)
	-- Lấy những ParentID mới đăng nhập lần đầu, chưa refresh và -> người dùng không đăng xuất
	INSERT INTO @Tokens(Id) SELECT Id FROM RefreshTokens WHERE IsExpiredAt < GETDATE() AND ParentId IS NULL AND IsUsed = 1;

	SELECT @RowCount = COUNT(*) FROM @Tokens;

	SELECT TOP 1 @Id = Id FROM @Tokens;


	WHILE @RowCount > 0
	BEGIN

		DELETE FROM RefreshTokens WHERE Id = @Id;

		DELETE FROM @Tokens WHERE Id = @Id;

		SET @RowCount = @RowCount - 1;

		SELECT TOP 1 @Id = Id FROM @Tokens;
	END
END