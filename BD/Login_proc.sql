USE [Magic]
GO
/****** Object:  StoredProcedure [dbo].[usp_login]    Script Date: 04/06/2017 18:51:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[usp_login] (@user VARCHAR(255), @pass VARCHAR(max), @r BIT OUTPUT) 
AS
BEGIN
	DECLARE @passw VARCHAR(max) = HASHBYTES('SHA2_512', @pass)
	IF dbo.isRegistered(@user) = 0
	BEGIN
		EXEC register @user, @passw
		SET @r = 1;
		RETURN;
	END
	
	DECLARE @passw2 VARCHAR(max);
	SELECT @passw2 = password FROM [User] WHERE email = @user
	IF @passw = @passw2
	BEGIN
		SET @r = 1;
		RETURN;
	END
	SET @r = 0;
	RETURN;
END