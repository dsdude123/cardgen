using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cardgenFunction.Models
{
    public class PokemonCard
    {
        public string name;
        public uint HP;
        public string species;
        //public Energy cardType - fire water,etc
        //public Attack[] attacks[2]
        //public Energy weaknessType
        //public string weaknessAmount - string so we can use both X,+,-
        //public Energy resistanceType
        public string resistanceAmount;
        public int retreatCost;
        public string illustrator;
        public string flavorText;

        public uint length; // inches

        public uint weight; // pounds

        public uint cardNumber = 1; // card number in set

        public uint cardMaxNumber = 1; //number of card in set

        //public Rarity rarity;
        //public CardStage stage
        public bool IsEX = false; // Pokemon EX cards
    }
    
}
