using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJack
{
    class Card
    {
        private string source;
        private int score;

        public Card(string source , int score)
        {
            this.source = source;
            this.score = score;
        }

        public string Source
        {
            get => source;
            set => source = value;
        }

        public int Score
        {
            get => score;
            set => score = value;
        }

    }
}
