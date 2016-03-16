using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Text.RegularExpressions;

namespace kontur
{
    class Card
    {
        public string[] possibleColors = { "Red", "Blue", "Green", "Yellow", "White" };
        public int[] possibleRanks = { 1, 2, 3, 4, 5 };
        public int rank
        {
            get;
        }
        // public string rank;
        /* public string Rank
         {
             get { return rank;  }
         }*/
        public string color
        {
            get;
        }
        public string calledColor = ""; //public?
        public int calledRank = -1;

        private List<string> unavailableColors = new List<string>();

        private List<int> unavailableRanks = new List<int>();

        public Card(int rank1, string color1)
        {
            rank = rank1;
            color = color1;
        }

        public void AddUnavailableColor(string color)
        {
            if (!unavailableColors.Contains(color))
                unavailableColors.Add(color);
        }

        public void AddUnavailableRank(int rank)
        {
            if(!unavailableRanks.Contains(rank))
                unavailableRanks.Add(rank);
        }

        public void TryAddRealColor()
        {
            if (calledColor == "" && unavailableColors.Count() == 4)
            {
                foreach (var color in possibleColors)
                {
                    if (!unavailableColors.Contains(color))
                    {
                        calledColor = color;
                        break;
                    }
                }
            }
        }

        public void TryAddRealRank()
        {
            if (calledRank == -1 && unavailableRanks.Count() == 4)
            {
                foreach (var card in possibleRanks)
                {
                    if (!unavailableRanks.Contains(rank))
                    {
                        calledRank = rank;
                        break;
                    }
                }
            }
        }

        /*public bool IsColorUnvailable(string color)
        {
            return unavailableColors.Contains(color);
        }

        public bool IsRankUnavailable(int rank)
        {
            return unavailableRanks.Contains(rank);
        }*/
        public List<string> GetAvailableColors()
        {
            List<string> availableColors = new List<string>();
            foreach (var color in possibleColors)
                if (!unavailableColors.Contains(color))
                    availableColors.Add(color);
            return availableColors;
        }
    }

    class Deck
    {
        private Queue<Card> cards = new Queue<Card>();
        public void AddCardToDeck(Card card)
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
        private List<Card> hand = new List<Card>();
        private int maxHandSize = 5;
        public Player() { }
       // Deck deck = new Deck();
        public Player(Deck deck)
        {
            //deck = deck1;
            for (int i = 0; i < maxHandSize; ++i)
                TakeCardFromDeck(deck.GiveTopCardToPlayer());
        }
       // private List<Card> calledCards;
        public void TakeCardFromDeck(Card card) //private? GetCard?
        {
            hand.Add(card);
        }
        public Card PlayCard(int position, Deck deck) //или put to?
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
        public void DropCard(int position, Deck deck)
        {
            hand.RemoveAt(position);
            TakeCardFromDeck(deck.GiveTopCardToPlayer());
        }
        public bool DoesOthePlayerTellTruthAboutCard(int cardPosition)
        {
            var card = hand[cardPosition];
            if (card.calledRank != -1 && card.rank != card.calledRank)
                return false;
            if (card.calledColor != "" && card.color != card.calledColor)
                return false;
            return true;
        }


        public void AddUnavailableColorToCards(string color)
        {
            foreach (var card in hand)
            {
                if (card.calledColor != color)
                    card.AddUnavailableColor(color);
            }
        }

        public void AddUnavailableRankToCards(int rank)
        {
            foreach (var card in hand)
            {
                if (card.calledRank != rank)
                    card.AddUnavailableRank(rank);
            }
        }

        public void TryKnowRealCardColors()
        {
            foreach (var card in hand)
                card.TryAddRealColor();
        }

        public void TryKnowRealCardRanks()
        {
            foreach (var card in hand)
                card.TryAddRealRank();
        }
        /* public string TellAllCardInHand()
         {
             string result = "";
             foreach(var card in hand)
                 result += card
         }*/
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

        public const int MaxTableSize = 25;

        public int GetCardCount()
        {
            return cards.Count;
        }

        public void AddCardToTable(Card card)
        {
            cards.Add(card);
        }
        public bool CanCardBePlaced(Card card) //нет ли так дублирования кода?
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
            return false; //Почему он считает, что не везде есть возврат?
        }
        /*public List<Card> GetAllCardsWithTheSameColor(string color)
        {
            return cards.Where(card => card.color == color).ToList();
        }
        public List<Card> GetAllCardsWithPreviousRank(int rank)
        {
            return cards.Where(card => card.rank == rank).ToList();
        }*/
        public int GetSameColorCardsCount(string color)
        {
            return cards.Where(card => card.color == color).Count();
            /*int count = 0;
            foreach (var card in cards)
                if (card.color == color)
                    count++;
            return count;*/
        }
        public int GetCardsWithPreviousRankCount(int rank)
        {
           return cards.Where(card => card.rank == rank).Count();
           /* int count = 0;
            foreach (var card in cards)
                if (card.rank == rank)
                    count++;
            return count;*/
        }

        public int PlayedCardsCount()
        {
            return cards.Count;
        }
    }

    class Game
    {
        // Player[] players = { new Player(), new Player() };
        Player[] players = new Player[2];
        //Player currentPlayer = new Player();
        Table table = new Table();
        Deck deck = new Deck();
        int turnCount;
        int rickTurnsCount = 0;
        bool gameIsOver = false;

        public Game()
        {

        }

        public Game(string startCommand)
        {
            turnCount = 0;
            // Regex CardRankAndColorRegEx = new Regex("[RGBYW][1-5]");
            // var cards = CardRankAndColorRegEx.Matches(startCommand);
            var cards = startCommand.Split(' ');          
            const int cardStartPosition = 5; //в строке "Start new game with new deck ..." 5 слов =)  
            for (int i = cardStartPosition; i < cards.Length; ++i)
            {
                deck.AddCardToDeck(CardParser.GetCardFromAbbreviation(cards[i]));
            }
            players[0] = new Player(deck);
            players[1] = new Player(deck);
        }

        public void MakeTurn(string command) //private?
        {
            var currentPlayer = players[turnCount% 2];
            var nextPlayer = players[(turnCount + 1) % 2];
            turnCount++;
            var commandElements = command.Split(' ');
            if (command.Contains("Tell"))
            {
                if (command.Contains("rank"))
                {
                    const int rankPosition = 2;
                    const int rankStartPosition = 5; //посмотреть, откуда начинается // внезапно, тоже с 6-ой позиции
                    int rank = int.Parse(commandElements[rankPosition]);
                    for (int i = rankStartPosition; i < commandElements.Length; ++i)
                    {
                        nextPlayer.LearnCardRank(rank, int.Parse(commandElements[i]));
                        if (nextPlayer.GetSameRankCardsCount(rank) != commandElements.Length - rankStartPosition ||
                            !nextPlayer.DoesOthePlayerTellTruthAboutCard(int.Parse(commandElements[i])))
                        {
                            endGame();
                            break;
                        }
                        
                    }
                    nextPlayer.AddUnavailableRankToCards(rank);
                    nextPlayer.TryKnowRealCardRanks();
                }

                if (command.Contains("color"))
                {
                    //var colors = command.Split(' ');
                    const int colorPositionInCommand = 2; //подумать над названием
                    const int colorStartPosition = 5; //посмотреть, откуда начинается 
                    var color = commandElements[colorPositionInCommand];
                    for (int i = colorStartPosition; i < commandElements.Length; ++i)
                    {
                        //посмотреть, где объявляется цвет
                        nextPlayer.LearnCardColor(color, int.Parse(commandElements[i]));
                        if (nextPlayer.GetSameColorCardsCount(color) != commandElements.Length - colorStartPosition|| 
                            !nextPlayer.DoesOthePlayerTellTruthAboutCard(int.Parse(commandElements[i])))
                        {
                            endGame();
                            break;
                        }
                        
                    }
                    nextPlayer.AddUnavailableColorToCards(color);
                    nextPlayer.TryKnowRealCardColors();
                }
               

            }
            if (command.Contains("Play"))
            {
                var cardPositionInCommand = 2;
                var card = currentPlayer.PlayCard(int.Parse(commandElements[cardPositionInCommand]), deck);
                CheckPlayForCorrectnessAndRisk(card);
                table.AddCardToTable(card);
            }
            if (command.Contains("Drop"))
            {
                var cardPosition = 2; //Drop card ...
                currentPlayer.DropCard(int.Parse(commandElements[cardPosition]), deck); 
            }
            // CheckTurnForCorrectnessAndRisk(); //сюда ли?
           // WriteItwnformation();
            //TODO: проверить обновлённый розыгрыш карты
        }

        public void GameProcess(string command) //надо ли этот метод?
        {
            if (!gameIsOver)
            {
                MakeTurn(command);
                if (table.GetCardCount() == Table.MaxTableSize || deck.CurrentDeckSize() == 0)
                {
                    endGame();
                }
            }
                //WriteGameInformation();
        }

        /*private void CheckTurnForCorrectnessAndRisk()
        {
            
        }*/
        //когда дека пустая, игра тоже заканчивается

        private void CheckPlayForCorrectnessAndRisk(Card card)
        {
            if (!table.CanCardBePlaced(card))
            {
                endGame();
                return;
            }
            bool isTurnRisk = true;
            /*if (card.calledRank == -1 && table.GetCardsWithPreviousRankCount(card.rank) != 5 && card.rank != 1)
                isTurnRisk = true;
            if (card.calledColor == "" && table.GetSameColorCardsCount(card.color) != 0)
                isTurnRisk = true;*/
            if (card.calledRank == 1 && table.PlayedCardsCount() == 0)
                isTurnRisk = false;
            if (card.calledRank != -1)
            {
                if (card.calledColor != "")
                    isTurnRisk = false;
                else
                {
                    if (table.GetCardsWithPreviousRankCount(card.rank) == 5)
                    {
                        isTurnRisk = false;
                    }
                    else
                    {
                        bool canWeAssumeCard = true;
                        foreach (var availableColor in card.GetAvailableColors())
                        {
                            if (!table.CanCardBePlaced(new Card(card.rank, availableColor)))
                            {
                                canWeAssumeCard = false;
                                break;
                            }
                        }
                        isTurnRisk = !canWeAssumeCard;
                    }
                }
            }
            if (isTurnRisk)
                rickTurnsCount++;
                

            /*
                в каких ситуациях нет риска?
                мы знаем карту целиком
                мы знаем достоинство карты, а на столе есть карты всех цветов достоинством на одно ниже
                мы знаем цвет, а на столе нет карт такого цвета
            */

        }


        private void endGame()
        {
            if (!gameIsOver) //подумать, как обойтись без этой проверки!
            {
                gameIsOver = true;
                WriteGameInformation();
            }
        
        }
        private void WriteGameInformation()
        {
            Console.WriteLine("Turn: {0}, cards: {1}, with risk: {2}",
                              turnCount, table.PlayedCardsCount(), rickTurnsCount);
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
        static public string GetAbbreviationFromCard(Card card)
        {
            return card.color[0] + card.rank.ToString();
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(int.Parse("R1"[1].ToString()));
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