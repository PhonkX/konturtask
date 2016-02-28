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
        public string rank
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
        public string calledColor; //public?
        public string calledRank;
        public Card(string rank1, string color1)
        {
            rank = rank;
            color = color1;
        }
    }

    class Deck
    {
        private Queue<Card> Cards;
        public void AddCardToDeck(Card card)
        {
            Cards.Enqueue(card);
        }
        public Card GiveTopCardToPlayer()
        {
            return Cards.Dequeue();
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
        public Card PutCardOnDeck(int position) //или put to?
        {
            return hand[position];
        }
        public void LearnCardRank(string rank, int position)
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
        /*public bool CanCardBePlaced(Card card)
        {
            
        }*/
    }

    class Game
    {
        Player[] players = { new Player(), new Player() };
        Table table = new Table();
        Deck deck = new Deck();
        int turnCount;
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
                        players[turnCount % 2].LearnCardRank(commandElements[rankPosition], int.Parse(commandElements[i]));
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
                        players[turnCount % 2].LearnCardColor(commandElements[colorPositionInCommand], int.Parse(commandElements[i]));
                    }
                }
            }

            if (command.Contains("Drop"))
            {
                var cardPosition = 2; //Drop card ...
                players[turnCount % 2].DropCard(int.Parse(commandElements[cardPosition])); 
            }

            CheckTurnForCorrectnessAndRisk(); //сюда ли?

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

        private void CheckTurnForCorrectnessAndRisk()
        {
            
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
                    color = "red";
                    break;
                case 'Y':
                    color = "yellow";
                    break;
                case 'B':
                    color = "blue";
                    break;
                case 'G':
                    color = "green";
                    break;
                case 'W':
                    color = "white";
                    break;
                default: break;
                 
            }
            return new Card(color, cardAbbreviation[1].ToString());
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
