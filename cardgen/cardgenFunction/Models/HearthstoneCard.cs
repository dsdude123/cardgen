using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cardgenFunction.Models
{
    public class HearthstoneCard
    {
        public string cardName;
        public string tribe;
        public CardType cardType = CardType.Minion;
        public CardClass cardClass = CardClass.Neutral;
        public string cost;
        public string attack;
        public string health;
        public CardRarity rarity = CardRarity.Free;
        public string text;
        //public bool isGold = false;
        public string artwork; //base64

        public enum CardType
        {
            Minion,
            Spell,
            Weapon,
            Power,
            Portrait,
            Hero
        };

        public enum CardClass
        {
            Neutral,
            Warrior,
            Shaman,
            Rogue,
            Paladin,
            Hunter,
            Druid,
            Warlock,
            Mage,
            Priest,
            DeathKnight,
            Monk
        };

        public enum CardRarity
        {
            Free,
            Common,
            Rare,
            Epic,
            Legendary
        };

    }

}
