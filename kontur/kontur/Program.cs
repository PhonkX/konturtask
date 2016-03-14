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
        public Card(int rank1, string color1)
        {
            rank = rank;
            color = color1;
        }
    }

    class Deck
    {
        private Queue<Card> сards;
        public void AddCardToDeck(Card card)
        {
            сards.Enqueue(card);
        }
        public Card GiveTopCardToPlayer()
        {
            return сards.Dequeue();
        }
    }

    class Player
    {
        private List<Card> hand;
       // private List<Card> calledCards;
        public void TakeCardFromDesk(Card card) //private? GetCard?
        {
            hand.Add(card);
        }
        public Card PlayCard(int position) //или put to?
        {
            return hand[position];
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

       /* public string TellAllCardInHand()
        {
            string result = "";
            foreach(var card in hand)
                result += card
        }*/
    }

    class Table
    {
        private List<Card> cards;

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
        public int GetCardsWithTheSameColorCount(string color)
        {
            return cards.Where(card => card.color == color).Count();
        }
        public int GetCardsWithPreviousRankCount(int rank)
        {
            return cards.Where(card => card.rank == rank).Count();
        }

        public int PlayedCardsCount()
        {
            return cards.Count;
        }
    }

    class Game
    {
        Player[] players = { new Player(), new Player() };
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
            turnCount = 1;
            // Regex CardRankAndColorRegEx = new Regex("[RGBYW][1-5]");
            // var cards = CardRankAndColorRegEx.Matches(startCommand);
            var cards = startCommand.Split(' ');
            const int cardStartPosition = 5; //в строке "Start new game with new deck ..." 5 слов =)  
            for (int i = cardStartPosition; i < cards.Length; ++i)
            {
                deck.AddCardToDeck(CardParser.GetCardFromAbbreviation(cards[i]));
            }          
        }

        public void MakeTurn(string command) //private?
        {
            var currentPlayer = players[turnCount % 2];
            var commandElements = command.Split(' ');
            if (command.Contains("Tell"))
            {
                if (command.Contains("rank"))
                {
                    //var ranks = command.Split(' ');
                    const int rankPosition = 2;
                    const int rankStartPosition = 5; //посмотреть, откуда начинается // внезапно, тоже с 6-ой позиции
                    for (int i = rankStartPosition; i < commandElements.Length; ++i)
                    {
                        //посмотреть, где объявляется ранг
                        currentPlayer.LearnCardRank(int.Parse(commandElements[rankPosition]), int.Parse(commandElements[i]));
                        if (!currentPlayer.DoesOthePlayerTellTruthAboutCard(int.Parse(commandElements[i])))
                        {
                            gameIsOver = true;
                            break;
                        }
                    }
                }

                if (command.Contains("rank"))
                {
                    //var colors = command.Split(' ');
                    const int colorPositionInCommand = 2; //подумать над названием
                    const int colorStartPosition = 5; //посмотреть, откуда начинается 
                    for (int i = colorStartPosition; i < commandElements.Length; ++i)
                    {
                        //посмотреть, где объявляется цвет
                        currentPlayer.LearnCardColor(commandElements[colorPositionInCommand], int.Parse(commandElements[i]));
                        if (!currentPlayer.DoesOthePlayerTellTruthAboutCard(int.Parse(commandElements[i])))
                        {
                            gameIsOver = true;
                            break;
                        }
                    }
                }
                if (command.Contains("Play"))
                {
                    var cardPositionInCommand = 2;
                    var card = currentPlayer.PlayCard(cardPositionInCommand);
                    CheckPlayForCorrestnessAndRisk(card);
                    table.AddCardToTable(card);
                }
            }

            if (command.Contains("Drop"))
            {
                var cardPosition = 2; //Drop card ...
                currentPlayer.DropCard(int.Parse(commandElements[cardPosition])); 
            }

            // CheckTurnForCorrectnessAndRisk(); //сюда ли?
            WriteInformation();
        }

        public void GameProcess(string command) //надо ли этот метод?
        {
            if (!gameIsOver)
            {
                MakeTurn(command);
                if (table.GetCardCount() == Table.MaxTableSize)
                    gameIsOver = true;
            }

        }

        /*private void CheckTurnForCorrectnessAndRisk()
        {
            
        }*/

        private void CheckPlayForCorrestnessAndRisk(Card card)
        {
            if (!table.CanCardBePlaced(card))
            {
                gameIsOver = true;
                return;
            }
            bool isTurnRisk = false;
            if (card.calledRank == -1 && table.GetCardsWithPreviousRankCount(card.rank) != 5)
                isTurnRisk = true;
            if (card.calledColor == "" && table.GetCardsWithTheSameColorCount(card.color) != 0)
                isTurnRisk = true;
            if (isTurnRisk)
                rickTurnsCount++;
                

            /*
                в каких ситуациях нет риска?
                мы знаем карту целиком
                мы знаем достоинство карты, а на столе есть карты всех цветов достоинством на одно ниже
                мы знаем цвет, а на столе нет карт такого цвета
            */

        }

        private void WriteInformation()
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
            return new Card(cardAbbreviation[1], color);
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