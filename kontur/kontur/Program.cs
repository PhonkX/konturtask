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
        public Player() { }
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

        public bool AreCalledCardCharacteristicsTrue(int cardPosition)//too long
        {
            var card = hand[cardPosition];
            if (card.CalledRank != -1 && card.Rank != card.CalledRank)
                return false;
            if (card.CalledColor != "" && card.Color != card.CalledColor)
                return false;
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
        public bool CanCardBePlaced(Card card)//wtf //надо! =D
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

        public int GetSameColorCardsCount(string color)
        {
            return cards.Where(card => card.Color == color).Count();

        }
        public int GetCardsWithPreviousRankCount(int rank)
        {
            return cards.Where(card => card.Rank == rank).Count();
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

        public Game()
        {

        }

        public Game(string startCommand)
        {
            players = new Player[2];
            turnCount = 0;
            var cards = startCommand.Split(' ');    // назвать по другому
            const int cardsStartPositionInCommand = 5; //"Start new game with new deck <card>" - 5 words before cards  
            for (int i = cardsStartPositionInCommand; i < cards.Length; ++i)
            {
                deck.AddCard(CardParser.GetCardFromAbbreviation(cards[i]));
            }
            players[0] = new Player(deck);//wtf, why you use 'deck' from class definition //dependency injection, yo!
            players[1] = new Player(deck);
            riskyTurnsCount = 0;
        }

        //private void MakeTurn(string command)
        //private int MakeTurn(string command)
        private string MakeTurn(string command)
        {
            string turnStatus = "Good turn"; //"Good turn", if turn is correct, 1, "Bad turn", if turn is incorrect
            var currentPlayer = players[turnCount % 2];
            var nextPlayer = players[(turnCount + 1) % 2];
            turnCount++;
           // var commandElements = command.Split(' ');//strange naming

            if (command.Contains("Tell"))
            {
                if (command.Contains("rank"))
                {
                    /*const int rankPositionInCommand = 2;//too short, can't understand what it does
                    const int positionWhereCardsStartInCommand = 5; // Tell rank <...> for cards <...>
                    int rank = int.Parse(commandElements[rankPositionInCommand]);*/
                    var rankAndCardsPositionsInHand = CardParser.GetRankAndCardsPositions(command);
                    var rank = rankAndCardsPositionsInHand.Item1;
                    var positions = rankAndCardsPositionsInHand.Item2;
                    turnStatus = TellRankCommand(rank, positions, nextPlayer);
                }
                if (command.Contains("color"))
                {
                    var colorAndCardsPositionsInHand = CardParser.GetColorAndCardsPositions(command);
                    var color = colorAndCardsPositionsInHand.Item1;
                    var positions = colorAndCardsPositionsInHand.Item2; //TODO: подумать над названиями переменных!
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

        // public void TellRankCommand(string[] commandElements, Player targetPlayer)
        //public int TellRankCommand(string[] commandElements, Player targetPlayer)//передавать ранг и карты вместо строки
        private string TellRankCommand(int rank, List<int> cardsPositionsInHand, Player targetPlayer)
        {
            /*const int rankPositionInCommand = 2;//too short, can't understand what it does
            const int positionWhereCardsStartInCommand = 5; // Tell rank <...> for cards <...>
            int rank = int.Parse(commandElements[rankPositionInCommand]);*/
            int cardsInCommandCount = cardsPositionsInHand.Count();
            if (targetPlayer.GetSameRankCardsCount(rank) != cardsInCommandCount)
            {
                endGame();
                return "Bad turn";
            }
            for (int i = 0; i < cardsInCommandCount; ++i)
            {
                targetPlayer.LearnCardRank(rank, cardsPositionsInHand[i]);   
                if (!targetPlayer.AreCalledCardCharacteristicsTrue(cardsPositionsInHand[i]))
                {
                    endGame();
                    //break;
                    return "Bad turn";
                }

            }
            targetPlayer.DeleteUnavailableRank(rank);
            return "Good turn";
        }

        //public void TellColorCommand(string[] commandElements, Player targetPlayer)//we can call player as currentplayer //smth done
        //public int TellColorCommand(string[] commandElements, Player targetPlayer)
        private string TellColorCommand(string color, List<int> cardsPositionsInHand, Player targetPlayer)
        {
            /*const int colorPositionInCommand = 2; //Tell color <Color> for cards <...>
            const int positionWhereCardsStartInCommand = 5;
            var color = commandElements[colorPositionInCommand];*/
            int cardsInCommandCount = cardsPositionsInHand.Count();
            if (targetPlayer.GetSameColorCardsCount(color) != cardsInCommandCount)
            {
                endGame();
                return "Bad turn";
            }
            for (int i = 0; i < cardsInCommandCount; ++i)
            {
                targetPlayer.LearnCardColor(color, cardsPositionsInHand[i]);
                if (!targetPlayer.AreCalledCardCharacteristicsTrue(cardsPositionsInHand[i]))
                {
                    endGame();
                   // break;
                    return "Bad turn";
                }

            }
            targetPlayer.DeleteUnavailableColor(color);
            return "Good turn";
        }

        //public void PlayCommand(string[] commandElements, Player player)
        //public int PlayCommand(string[] commandElements, Player player)
        private string PlayCommand(int position, Player player)
        {
            //var cardPositionInCommand = 2;
            var card = player.PlayCard(position);
            if (table.CanCardBePlaced(card))
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

        // public void DropCommand(string[] commandElements, Player player)
        public void DropCommand(int position, Player player)
        {
            player.DropCard(position);
        }

        public void GameProcess(string command)
        {
            if (!gameIsOver)
            {
                if (MakeTurn(command) == "Good turn")//может, пусть лучше возвращает строки //сделано
                {
                    if (table.GetPlayedCardsCount() == Table.MaxTableSize || deck.GetCurrentDeckSize() == 0)
                    {
                        endGame();
                        return;
                    }
                }
            }

        }

     /*   private bool IsPlayCorrect(Card card)//подумать про то, чтобы убрать метод
        {
            return table.CanCardBePlaced(card); // зачем 2 метода для одной задачи
        }*/

        private bool IsTurnRisky(Card card)
        {
            bool isTurnRisky = true;
            if (card.CalledRank == 1 && table.GetPlayedCardsCount() == 0)
                isTurnRisky = false;
            if (card.CalledRank != -1)
            {
                if (card.CalledColor != "")
                    isTurnRisky = false;
                else
                {
                    if (table.GetCardsWithPreviousRankCount(card.Rank) == 5) //спросить, как правильно с точки зрения грамматики
                        //подумать про то, как обозначить константы
                    {
                        isTurnRisky = false;
                    }
                    else
                    {
                        bool canWeAssumeCard = true;
                        foreach (var availableColor in card.availableColors)
                        {
                            if (!table.CanCardBePlaced(new Card(card.Rank, availableColor)))
                            {
                                canWeAssumeCard = false;
                                break; //зачем делать брэйк, если найден вариант карты, который не может быть сыгран? Разобраться!
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
        private const int colorPositionInAbbreviation = 0; //Card: CR, C - first letter of color, R - rank
        private const int rankPositionInAbbreviation = 1;  //for example: R1
        private const int colorPositionInTellCommand = 2; //Tell color/rank <color/rank> for card <cards>
        private const int rankPositionInTellCommand = 2;    
        private const int positionWhereCardStartsInPlayAndDropCommand = 2;
        private const int positionWhereCardsStartInTellCommand = 5;

        static public Card GetCardFromAbbreviation(string cardAbbreviation)
        {
            string color = "";
            switch (cardAbbreviation[colorPositionInAbbreviation])
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
            return new Card(int.Parse(cardAbbreviation[rankPositionInAbbreviation].ToString()), color); //подумать про именование констант
        }

        static public Tuple<int, List<int>> GetRankAndCardsPositions(string command)
        {
            List<int> positions = new List<int>();
            var wordsInCommand = command.Split(' ');
            int rank = int.Parse(wordsInCommand[rankPositionInTellCommand]);
            for (int i = positionWhereCardsStartInTellCommand; i < wordsInCommand.Count(); ++i)
            {
                positions.Add(int.Parse(wordsInCommand[i]));
            }
            return Tuple.Create(rank, positions);
        }

        static public Tuple<string, List<int>> GetColorAndCardsPositions(string command)
        {
            List<int> positions = new List<int>();
            var wordsInCommand = command.Split(' ');
            string color = wordsInCommand[colorPositionInTellCommand];
            for (int i = positionWhereCardsStartInTellCommand; i < wordsInCommand.Count(); ++i)
            {
                positions.Add(int.Parse(wordsInCommand[i]));
            }
            return Tuple.Create(color, positions);
        }

        static public int GetCardPosition(string command)
        {
            return int.Parse(command.Split(' ')[positionWhereCardStartsInPlayAndDropCommand]);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();//убрать пустые конструкторы //не получится
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