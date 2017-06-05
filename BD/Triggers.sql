USE Magic;

GO

CREATE TRIGGER valid_amount_cards ON CardInDeck
AFTER UPDATE, INSERT
AS
	IF EXISTS(SELECT * FROM inserted JOIN Card ON inserted.card = Card.id AND rarity != 'Basic Land' AND amount > 4)
	BEGIN
		RAISERROR('Cannot have more than 4 of the same non basic lands on a deck',0,0);
		ROLLBACK TRAN;	
	END;
	ELSE IF EXISTS(SELECT * FROM inserted WHERE amount < 1)
		DELETE FROM CardInDeck WHERE card IN(SELECT card FROM inserted WHERE amount = 0);

GO

CREATE TRIGGER update_rating_at_update ON RatedBy
AFTER UPDATE, INSERT
AS
	DECLARE @deckID INT;
	SELECT @deckID = inserted.deck FROM inserted;
	UPDATE Deck SET rating = (SELECT avg(rating) from RatedBy where deck = @deckID) where id = @deckID;

GO

CREATE TRIGGER valid_bid ON OfferBid
AFTER UPDATE, INSERT
AS
	IF EXISTS(
		SELECT Offer, MinimumBid
		FROM (
			SELECT Bid, Offer
			FROM inserted) as ob
		JOIN (
			SELECT ID, MinimumBid
			FROM ListingBid) AS lb
		ON ob.Bid = lb.ID AND Offer < MinimumBid)
	BEGIN
		RAISERROR('Offer has to be greater or equal to MinimumBid',0,0);
		ROLLBACK TRAN;	
	END;

GO

CREATE TRIGGER unit_check ON CardInListing
AFTER UPDATE
AS
	BEGIN TRAN;
	IF EXISTS(SELECT * from Inserted where Units < 0)
	BEGIN
		RAISERROR('Can''t buy that many cards', 11, 0);
		ROLLBACK
	END
	
	IF EXISTS(SELECT * from INSERTED where Units = 0)
	BEGIN
		DECLARE @id INT ;
		SELECT @id = ID FROM INSERTED;
		DELETE FROM Listing WHERE ID = @id;
	END
	COMMIT

GO

CREATE TRIGGER balance_check ON [User] 
AFTER INSERT, UPDATE
AS 
	IF EXISTS(SELECT * FROM INSERTED WHERE balance < 0)
	BEGIN
		RAISERROR('Can''t have negative balance!', 11, 0);
		ROLLBACK;
	END

GO

CREATE TRIGGER preventEditionDeleteOrUpdate ON Edition
INSTEAD OF DELETE, UPDATE
AS
	RAISERROR('CANT UPDATE OR DELETE EXISTING EDITION',11,0);
	
GO

CREATE TRIGGER preventCardDeleteOrUpdate ON [Card]
INSTEAD OF DELETE, UPDATE
AS
	RAISERROR('CAN''T UPDATE OR DELETE EXISTING CARD',11,0);
	
GO

CREATE TRIGGER preventCardInListingHistory ON CardInListingHistory
INSTEAD OF DELETE, UPDATE
AS
	RAISERROR('CAN''T UPDATE OR DELETE EXISTING CARD',11,0);
GO

