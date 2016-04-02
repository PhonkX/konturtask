using System;
using System.Collections.Generic;
using System.Linq;

namespace kontur
{
    class Card
    {
        public int rank
        {
            get;
        }

        public string color
        {
            get;
        }

        public string calledColor;
        public int calledRank;

        public List<int> availableRanks
        {
            get;
        }
        public List<string> availableColors
        {
            get;
        }

        public Card(int rank1, string color1)
        {
            rank = rank1;
            color = color1;
            availableRanks = new List<int>(new int[] { 1, 2, 3, 4, 5 });
            availableColors = new List<string>(new string[] { "Red", "Blue", "Green", "Yellow", "White" });
            calledColor = "";
            calledRank = -1;
        }


        public void DeleteUnavailableRank(int rank)
        {
            availableRanks.Remove(rank);
            if (availableRanks.Count() == 1)
                TryAddRealRank();
        }

        public void DeleteUnavailableColor(string color)
        {
            availableColors.Remove(color);
            if (availableColors.Count() == 1)
                TryAddRealColor();
        }

        private void TryAddRealRank()
        {
            calledRank = availableRanks[0];
        }

        private void TryAddRealColor()
        {
            calledColor = availableColors[0];
        }
    }

    class Deck
    {
        private Queue<Card> cards = new Queue<Card>();
        public void AddCard(Card card)//cut the name of method //done
        {
            cards.Enqueue(card);
        }

        public Card GiveTopCardToPlayer()
        {
            return cards.Dequeue();
        }

        public int CurrentDeckSize()
        {
            return cards.Count();
        }
    }

    class Player
    {
        private List<Card> hand ;
        private const int maxHandSize = 5;
        Deck deck;
        public Player() { }
        public Player(Deck deck1)
        {
            hand = new List<Card>();
            deck = deck1;
            for (int i = 0; i < maxHandSize; ++i)
                TakeCardFromDeck(deck.GiveTopCardToPlayer());
        }

        public void TakeCardFromDeck(Card card)//Its not neccesarry to tell from where you take the card //may be from other player?
        {
            hand.Add(card);
        }

        public Card PlayCard(int position)
        {
            var card = hand[position];
            hand.RemoveAt(position);
            TakeCardFromDeck(deck.GiveTopCardToPlayer());
            return card;
        }

        public void LearnCardRank(int rank, int position)
        {
            hand[position].calledRank = rank;
        }

        public void LearnCardColor(string color, int position)
        {
            hand[position].calledColor = color;
        }
        public void DropCard(int position)
        {
            hand.RemoveAt(position);
            TakeCardFromDeck(deck.GiveTopCardToPlayer());
        }
        public bool DoesOthePlayerTellTruthAboutCard(int cardPosition)//too long
        {
            var card = hand[cardPosition];
            if (card.calledRank != -1 && card.rank != card.calledRank)
                return false;
            if (card.calledColor != "" && card.color != card.calledColor)
                return false;
            return true;
        }

        public void DeleteUnavailableColor(string color)
        {
            foreach (var card in hand)
            {
                if (card.calledColor != color)
                    card.DeleteUnavailableColor(color);
            }
        }

        public void DeleteUnavailableRank(int rank)//is it neccesary to thell where you delete rank? Why don't just use DeleteUnavailableRank instead?  
        {//done
            foreach (var card in hand)
            {
                if (card.calledRank != rank)
                    card.DeleteUnavailableRank(rank);
            }
        }

        public int GetSameColorCardsCount(string color)
        {
            return hand.Where(card => card.color == color).Count();
        }
        public int GetSameRankCardsCount(int rank)
        {
            return hand.Where(card => card.rank == rank).Count();
        }
    }

    class Table
    {
        private List<Card> cards = new List<Card>();

        public const int MaxTableSize = 25;//wrong, try amountOfColors*maxCost //2 more variables? may be

        public int GetCardCount()
        {
            return cards.Count;
        }

        public void AddCardToTable(Card card)
        {
            cards.Add(card);
        }
        public bool CanCardBePlaced(Card card)//wtf //надо! =D
        {
            var sameColorCards = cards.Where(x => x.color == card.color).OrderBy(x => x.rank);
            if (sameColorCards.Count() == 0)
            {
                if (card.rank == 1)
                    return true;
            }
            else if (sameColorCards.Last().rank == card.rank - 1)
                    return true;
                else
                    return false;
            return false;
        }

        public int GetSameColorCardsCount(string color)
        {
            return cards.Where(card => card.color == color).Count();

        }
        public int GetCardsWithPreviousRankCount(int rank)
        {
           return cards.Where(card => card.rank == rank).Count();
        }

        public int GetPlayedCardsCount()//why dont use Get
        {
            return cards.Count;
        }
    }

    class Game
    {
        Player[] players;// = new Player[2];
        Table table = new Table();
        Deck deck = new Deck();
        int turnCount;
        int riskyTurnsCount;// = 0;
        bool gameIsOver;// = false;

        public Game()
        {

        }

        public Game(string startCommand)
        {
            players = new Player[2];
            turnCount = 0;
            var cards = startCommand.Split(' ');          
            const int cardStartPosition = 5; //в строке "Start new game with new deck ..." 5 слов =)  
            for (int i = cardStartPosition; i < cards.Length; ++i)
            {
                deck.AddCard(CardParser.GetCardFromAbbreviation(cards[i]));
            }
            players[0] = new Player(deck);//wtf, why you use 'deck' from class definition //taking card
            players[1] = new Player(deck);
            riskyTurnsCount = 0;
        }

        private void MakeTurn(string command)
        {
            var currentPlayer = players[turnCount % 2];
            var nextPlayer = players[(turnCount + 1) % 2];
            turnCount++;
            var commandElements = command.Split(' ');//strange naming

            if (command.Contains("Tell"))
            {
                if (command.Contains("rank"))
                {
                    TellRankCommand(commandElements, nextPlayer);
                }
                if (command.Contains("color"))
                {
                    TellColorCommand(commandElements, nextPlayer);
                }
            }

            if (command.Contains("Play"))
            {
                PlayCommand(commandElements, currentPlayer);
            }

            if (command.Contains("Drop"))
            {
                DropCommand(commandElements, currentPlayer);
            }
         }

        public void TellRankCommand(string[] commandElements, Player targetPlayer)
        {
            const int rankPosition = 2;//too short, can't understand what it does
            const int rankStartPosition = 5; // Tell rank <...> for cards <...>
            int rank = int.Parse(commandElements[rankPosition]);
            for (int i = rankStartPosition; i < commandElements.Length; ++i)
            {
                targetPlayer.LearnCardRank(rank, int.Parse(commandElements[i]));
                if (targetPlayer.GetSameRankCardsCount(rank) != commandElements.Length - rankStartPosition ||
                    !targetPlayer.DoesOthePlayerTellTruthAboutCard(int.Parse(commandElements[i])))
                {
                    endGame();
                    break;
                    //return;
                }

            }
            targetPlayer.DeleteUnavailableRank(rank);
        }

        public void TellColorCommand(string[] commandElements, Player targetPlayer)//we can call player as currentplayer //smth done
        {
            const int colorPositionInCommand = 2; //Tell color <Color> for cards <...>
            const int colorStartPosition = 5;
            var color = commandElements[colorPositionInCommand];
            for (int i = colorStartPosition; i < commandElements.Length; ++i)
            {
                targetPlayer.LearnCardColor(color, int.Parse(commandElements[i]));
                if (targetPlayer.GetSameColorCardsCount(color) != commandElements.Length - colorStartPosition ||
                    !targetPlayer.DoesOthePlayerTellTruthAboutCard(int.Parse(commandElements[i])))
                {
                    endGame();
                    break;
                   // return;
                }

            }
            targetPlayer.DeleteUnavailableColor(color);
        }

        public void PlayCommand(string[] commandElements, Player player)
        {
            var cardPositionInCommand = 2;
            var card = player.PlayCard(int.Parse(commandElements[cardPositionInCommand]));
            if (IsPlayCorrect(card))
            {
                if (IsTurnRisky(card))
                    riskyTurnsCount++;
                table.AddCardToTable(card);
            }
            else
            {
                endGame();
               // return;
            }
        }

        public void DropCommand(string[] commandElements, Player player)
        {
            var cardPosition = 2; //Drop card <number>
            player.DropCard(int.Parse(commandElements[cardPosition]));
        }

        public void GameProcess(string command)
        {
            if (!gameIsOver)
            {
                MakeTurn(command);
                if (table.GetCardCount() == Table.MaxTableSize || deck.CurrentDeckSize() == 0)
                {
                    endGame();
                    //return;
                }
            }
        
        }

        private bool IsPlayCorrect(Card card)
        {
            return table.CanCardBePlaced(card);
        }

        private bool IsTurnRisky(Card card)//ENGLISH MOTHERFUCKER, use risky instead //done
        {
            bool isTurnRisky = true;

            if (card.calledRank == 1 && table.GetPlayedCardsCount() == 0)
                isTurnRisky = false;
            if (card.calledRank != -1)
            {
                if (card.calledColor != "")
                    isTurnRisky = false;
                else
                {
                    if (table.GetCardsWithPreviousRankCount(card.rank) == 5)
                    {
                        isTurnRisky = false;
                    }
                    else
                    {
                        bool canWeAssumeCard = true;
                        foreach (var availableColor in card.availableColors)
                        {
                            if (!table.CanCardBePlaced(new Card(card.rank, availableColor)))
                            {
                                canWeAssumeCard = false;
                                break;
                            }
                        }
                        isTurnRisky = !canWeAssumeCard;
                    }
                }
            }
          
            return isTurnRisky;
        }
     
        private void endGame()
        {
            if (!gameIsOver)
            {
                gameIsOver = true;
                WriteGameInformation();
            }
        
        }
        private void WriteGameInformation()
        {
            Console.WriteLine("Turn: {0}, cards: {1}, with risk: {2}",
                              turnCount, table.GetPlayedCardsCount(), riskyTurnsCount);
        }
    }

    class CardParser
    {
        static public Card GetCardFromAbbreviation(string cardAbbreviation)
        {
            string color = "";
            switch (cardAbbreviation[0])
            {
                case 'R':
                    color = "Red";
                    break;
                case 'Y':
                    color = "Yellow";
                    break;
                case 'B':
                    color = "Blue";
                    break;
                case 'G':
                    color = "Green";
                    break;
                case 'W':
                    color = "White";
                    break;
                default: break;
                 
            }
            return new Card(int.Parse(cardAbbreviation[1].ToString()), color);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            string command;
            while ((command = Console.ReadLine()) != null)
            {
                if (command.Contains("Start"))
                {
                    game = new Game(command);
                }
                else game.GameProcess(command);
            }
        }
    }
}