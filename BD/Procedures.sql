USE Magic;

GO

CREATE PROC addCardToDeck(@cardId int, @deck int, @amount int, @sideboard BIT)
AS
	IF EXISTS(SELECT * FROM CardInDeck WHERE card = @cardId AND deck = @deck)
		UPDATE CardInDeck SET amount = amount + @amount WHERE deck = @deck AND card = @cardId AND isSideboard = @sideboard;
	ELSE
		INSERT INTO CardInDeck(deck,card,amount,isSideboard) VALUES (@deck, @cardId, @amount, @sideboard);

GO

CREATE PROC rate @user VARCHAR(255), @deckID INT, @rating FLOAT
AS
	IF EXISTS(SELECT * FROM RatedBy WHERE deck = @deckID AND [user] = @user)
		UPDATE RatedBy SET rating = @rating where (deck=@deckID and [user] = @user);
	ELSE
		INSERT INTO RatedBy VALUES (@deckID, @user, @rating);

GO

CREATE PROC register(@user VARCHAR(255), @password TEXT)
AS
	INSERT INTO [User] VALUES (@user, @password);

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

GO

CREATE PROC usp_sellingListingsSelect
AS
	SELECT * FROM Listing WHERE Sell = 1;


GO

CREATE PROC usp_buyOrSellCard (@cardinlisting INT, @amount INT, @user VARCHAR(255), @sell BIT)
AS
	BEGIN TRAN
		DECLARE @primaryuser VARCHAR(255), @isSell BIT;
		SELECT @primaryuser = [User], @isSell = Sell
		FROM (SELECT ID, [User], Sell FROM LISTING) AS l
		JOIN (SELECT listing FROM CardInListing WHERE ID = @cardinlisting) AS cid
		ON l.ID = cid.listing;
		IF (@isSell != @sell)
		BEGIN
			RAISERROR('You cannot buy/sell these cards', 11, 0);
			ROLLBACK TRAN;
		END
		INSERT INTO CardInListing (Listing, Card, Price_Per_Unit, Units, Condition) SELECT Listing, Card, Price_Per_Unit, @amount, Condition FROM CardInListing;
		DECLARE @max INT;
		SELECT @max = Units FROM CardInListing WHERE ID = @cardinlisting;
		IF (@amount > @max)
		BEGIN
			RAISERROR('Cannot buy more than what is sold', 11, 0);
			ROLLBACK TRAN;
		END
		
		UPDATE CardInListing SET Units = Units - @amount WHERE ID = @cardinlisting;
		INSERT INTO CardInListingHistory (ID, EndDate, SecondaryUser) VALUES (@cardinlisting, getdate(), @user);
	COMMIT TRAN;