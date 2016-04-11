using System;
using System.Collections.Generic;
using System.Linq;

namespace kontur
{
    class Card
    {
        public int Rank
        {
            get;
        }

        public string Color
        {
            get;
        }

        public string CalledColor;
        public int CalledRank;

        public const int defaultRank = -1;
        public const string defaultColor = "";
        public List<int> availableRanks
        {
            get;
        }
        public List<string> availableColors
        {
            get;
        }

        public Card(int rank, string color)
        {
            this.Rank = rank;
            this.Color = color;
            availableRanks = new List<int>(new int[] { 1, 2, 3, 4, 5 });
            availableColors = new List<string>(new string[] { "Red", "Blue", "Green", "Yellow", "White" });
            CalledColor = "";
            CalledRank = -1;
        }


        public void DeleteUnavailableRank(int rank)
        {
            availableRanks.Remove(rank);
            if (availableRanks.Count() == 1)
                AddRealRank();
        }

        public void DeleteUnavailableColor(string color)
        {
            availableColors.Remove(color);
            if (availableColors.Count() == 1)
                AddRealColor();
        }

        private void AddRealRank()
        {
            CalledRank = availableRanks[0];
        }

        private void AddRealColor()
        {
            CalledColor = availableColors[0];
        }
    }

    class Deck
    {
        private Queue<Card> cards = new Queue<Card>();
        public void AddCard(Card card)
        {
            cards.Enqueue(card);
        }

        public Card GiveTopCardToPlayer()
        {
            return cards.Dequeue();
        }

        public int GetCurrentDeckSize()
        {
            return cards.Count();
        }
    }

    class Player
    {
        private List<Card> hand;
        private const int maxHandSize = 5;
        Deck deck;
        public Player(Deck deck1)
        {
            hand = new List<Card>();
            deck = deck1;
            for (int i = 0; i < maxHandSize; ++i)
                TakeCard(deck.GiveTopCardToPlayer());
        }

        public void TakeCard(Card card)
        {
            hand.Add(card);
        }

        public Card PlayCard(int position)
        {
            var card = hand[position];
            hand.RemoveAt(position);
            TakeCard(deck.GiveTopCardToPlayer());
            return card;
        }

        public void LearnCardRank(int rank, int position)
        {
            hand[position].CalledRank = rank;
        }

        public void LearnCardColor(string color, int position)
        {
            hand[position].CalledColor = color;
        }

        public void DropCard(int position)
        {
            hand.RemoveAt(position);
            TakeCard(deck.GiveTopCardToPlayer());
        }

        public bool AreCalledCardsCharacteristicsTrue()
        {
            foreach (var card in hand)
            {
                if (card.CalledRank != -1 && card.Rank != card.CalledRank)
                    return false;
                if (card.CalledColor != "" && card.Color != card.CalledColor)
                    return false;
            }
            return true;
        }

        public void DeleteUnavailableColor(string color)
        {
            foreach (var card in hand)
            {
                if (card.CalledColor != color)
                    card.DeleteUnavailableColor(color);
            }
        }

        public void DeleteUnavailableRank(int rank)
        {
            foreach (var card in hand)
            {
                if (card.CalledRank != rank)
                    card.DeleteUnavailableRank(rank);
            }
        }

        public int GetSameColorCardsCount(string color)
        {
            return hand.Where(card => card.Color == color).Count();
        }

        public int GetSameRankCardsCount(int rank)
        {
            return hand.Where(card => card.Rank == rank).Count();
        }
    }

    class Table
    {
        private List<Card> cards = new List<Card>();

        private const int AmountOfColors = 5;
        private const int MaxCardRank = 5;
        public const int MaxTableSize = AmountOfColors * MaxCardRank;

        public void AddCardToTable(Card card)
        {
            cards.Add(card);
        }
        public bool CanCardBePlaced(Card card)
        {
            var sameColorCards = cards.Where(x => x.Color == card.Color).OrderBy(x => x.Rank);
            if (sameColorCards.Count() == 0)
            {
                if (card.Rank == 1)
                    return true;
            }
            else if (sameColorCards.Last().Rank == card.Rank - 1)
                return true;
            else
                return false;
            return false;
        }

        public int GetPlayedCardsCount()
        {
            return cards.Count;
        }
    }

    class Game
    {
        Player[] players;
        Table table = new Table();
        Deck deck = new Deck();
        int turnCount;
        int riskyTurnsCount;
        bool gameIsOver;
        private const int maxColorCount = 5;

        public Game(string startCommand)
        {
            players = new Player[2];
            turnCount = 0;
            var wordInCommand = startCommand.Split(' ');
            const int cardsStartPositionInCommand = 5; //"Start new game with new deck <card>" - 5 words before cards  
            for (int i = cardsStartPositionInCommand; i < wordInCommand.Length; ++i)
            {
                deck.AddCard(CardParser.GetCardFromAbbreviation(wordInCommand[i]));
            }
            players[0] = new Player(deck);
            players[1] = new Player(deck);
            riskyTurnsCount = 0;
        }

        private string MakeTurn(string command)
        {
            string turnStatus = "Good turn"; //"Good turn", if turn is correct, 1, "Bad turn", if turn is incorrect
            var currentPlayer = players[turnCount % 2];
            var nextPlayer = players[(turnCount + 1) % 2];
            turnCount++;
            if (command.Contains("Tell"))
            {
                if (command.Contains("rank"))
                {
                    var rankAndCardsPositionsInHand = CardParser.GetRankAndCardsPositions(command);
                    var rank = rankAndCardsPositionsInHand.Item1;
                    var positions = rankAndCardsPositionsInHand.Item2;
                    turnStatus = TellRankCommand(rank, positions, nextPlayer);
                }
                if (command.Contains("color"))
                {
                    var colorAndCardsPositionsInHand = CardParser.GetColorAndCardsPositions(command);
                    var color = colorAndCardsPositionsInHand.Item1;
                    var positions = colorAndCardsPositionsInHand.Item2;
                    turnStatus = TellColorCommand(color, positions, nextPlayer);
                }
            }

            if (command.Contains("Play"))
            {
                var cardPositionInHand = CardParser.GetCardPosition(command);
                turnStatus = PlayCommand(cardPositionInHand, currentPlayer);
            }

            if (command.Contains("Drop"))
            {
                var cardPositionInHand = CardParser.GetCardPosition(command);
                DropCommand(cardPositionInHand, currentPlayer);
            }
            return turnStatus;
        }

        private string TellRankCommand(int rank, List<int> cardsPositionsInHand, Player targetPlayer)
        {
            int cardsInCommandCount = cardsPositionsInHand.Count();
            for (int i = 0; i < cardsInCommandCount; ++i)
            {
                targetPlayer.LearnCardRank(rank, cardsPositionsInHand[i]);
            }
            if (!IsPlayCorrect(targetPlayer, cardsInCommandCount, rank))
            {
                endGame();
                return "Bad turn";
            }
            targetPlayer.DeleteUnavailableRank(rank);
            return "Good turn";
        }

        private string TellColorCommand(string color, List<int> cardsPositionsInHand, Player targetPlayer)
        {
            int cardsInCommandCount = cardsPositionsInHand.Count();
            for (int i = 0; i < cardsInCommandCount; ++i)
            {
                targetPlayer.LearnCardColor(color, cardsPositionsInHand[i]);
            }
            if (!IsPlayCorrect(targetPlayer, cardsInCommandCount, color))
            {
                endGame();
                return "Bad turn";
            }
            targetPlayer.DeleteUnavailableColor(color);
            return "Good turn";
        }

        private string PlayCommand(int position, Player player)
        {
            var card = player.PlayCard(position);
            if (IsPlayCorrect(card))
            {
                if (IsTurnRisky(card))
                    riskyTurnsCount++;
                table.AddCardToTable(card);
                return "Good turn";
            }
            else
            {
                endGame();
                return "Bad turn";
            }
        }

        public void DropCommand(int position, Player player)
        {
            player.DropCard(position);
        }

        public void GameProcess(string command)
        {
            if (!gameIsOver)
            {
                if (MakeTurn(command) == "Good turn")
                {
                    if (table.GetPlayedCardsCount() == Table.MaxTableSize || deck.GetCurrentDeckSize() == 0)
                    {
                        endGame();
                        return;
                    }
                }
            }
        }

        private bool IsPlayCorrect(Card card)
        {
            return table.CanCardBePlaced(card);
        }

        private bool IsPlayCorrect(Player player, int cardsInCommandCount, string color)
        {
            return (player.GetSameColorCardsCount(color) == cardsInCommandCount) &&
                    player.AreCalledCardsCharacteristicsTrue();
        }

        private bool IsPlayCorrect(Player player, int cardsInCommandCount, int rank)
        {
            return (player.GetSameRankCardsCount(rank) == cardsInCommandCount) &&
                   player.AreCalledCardsCharacteristicsTrue();
        }

        private bool IsTurnRisky(Card card)
        {
            bool isTurnRisky = true;
            if (card.CalledRank != Card.defaultRank)
            {
                if (card.CalledColor != Card.defaultColor)
                    isTurnRisky = false;
                else
                {
                    bool canWeAssumeCard = true;
                    foreach (var availableColor in card.availableColors)
                    {
                        if (!table.CanCardBePlaced(new Card(card.Rank, availableColor)))
                        {
                            canWeAssumeCard = false;
                            break;
                        }
                    }
                    isTurnRisky = !canWeAssumeCard;
                }
            }
            return isTurnRisky;
        }

        private void endGame()
        {
            gameIsOver = true;
            WriteGameInformation();
        }

        private void WriteGameInformation()
        {
            Console.WriteLine("Turn: {0}, cards: {1}, with risk: {2}",
                              turnCount, table.GetPlayedCardsCount(), riskyTurnsCount);
        }
    }

    class CardParser
    {
        private const int ColorPositionInAbbreviation = 0; //Card: CR, C - first letter of color, R - rank
        private const int RankPositionInAbbreviation = 1;  //for example: R1
        private const int ColorPositionInTellCommand = 2; //Tell color/rank <color/rank> for card <cards>
        private const int RankPositionInTellCommand = 2;
        private const int PositionWhereCardStartsInPlayAndDropCommand = 2;
        private const int PositionWhereCardsStartInTellCommand = 5;

        static public Card GetCardFromAbbreviation(string cardAbbreviation)
        {
            string color = "";
            switch (cardAbbreviation[ColorPositionInAbbreviation])
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
            return new Card(int.Parse(cardAbbreviation[RankPositionInAbbreviation].ToString()), color); //подумать про именование констант
        }

        static public Tuple<int, List<int>> GetRankAndCardsPositions(string command)
        {
            List<int> positions = new List<int>();
            var wordsInCommand = command.Split(' ');
            int rank = int.Parse(wordsInCommand[RankPositionInTellCommand]);
            for (int i = PositionWhereCardsStartInTellCommand; i < wordsInCommand.Count(); ++i)
            {
                positions.Add(int.Parse(wordsInCommand[i]));
            }
            return Tuple.Create(rank, positions);
        }

        static public Tuple<string, List<int>> GetColorAndCardsPositions(string command)
        {
            List<int> positions = new List<int>();
            var wordsInCommand = command.Split(' ');
            string color = wordsInCommand[ColorPositionInTellCommand];
            for (int i = PositionWhereCardsStartInTellCommand; i < wordsInCommand.Count(); ++i)
            {
                positions.Add(int.Parse(wordsInCommand[i]));
            }
            return Tuple.Create(color, positions);
        }

        static public int GetCardPosition(string command)
        {
            return int.Parse(command.Split(' ')[PositionWhereCardStartsInPlayAndDropCommand]);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Game game = null;
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