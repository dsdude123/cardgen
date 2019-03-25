using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cardgenFunction.Models
{
    public class PokemonCard
    {
    	public CardEra era;
        public string name;
        public uint HP;
        public string species; // <species> Pokemon - i.e. Pony Pokemon
        public Energy cardEnergy; //- fire water,etc
        public Attack[] attacks = new Attack[2];
        public Energy weaknessType;
        public string weaknessAmount; //- string so we can use both X,+,-
        public Energy resistanceType;
        public string resistanceAmount;
        public int retreatCost;
        public string illustrator;
        public string flavorText;

        public uint length = 0; // inches

        public uint weight = 0; // pounds

        public uint cardNumber = 1; // card number in set

        public uint cardMaxNumber = 1; //number of card in set

        public Rarity rarity = Rarity.Common;
        public CardStage stage = CardStage.Basic;
        public string evolvesFrom; // for evolutions
        //public bool IsEX = false; // Pokemon EX cards not yet supported
        
        public string evolvesFromArtwork; // base 64 byte[] images
        public string artwork;
        
        public enum Energy{
        	Colorless,
        	Fighting,
        	Fire,
        	Grass,
        	Lightning,
        	Psychic,
        	Water
        	/*
 			 * Types not yet suppoted 	
        	Fairy,
        	Dragon
        	 */
        };
        
        public enum CardEra{
        	Classic
        	/*
        	 * Series not yet supported
        	Gym,
        	Neo,
        	eCard,
        	EX,
        	DPPt,
        	HGSS,
        	BW,
        	XY,
        	SM
        	*/
        };
        
        public enum Rarity{
        	Common,
        	Uncommon,
        	Rare,
        	UltraRare
        };
        
        public enum CardStage{
        	Baby, // not supported in classic
        	Basic, 
        	Stage1,
        	Stage2
        };
        
        public class Attack{
        	public string attackName;
        	public string attackText;
        	public Energy[] energy = new Energy[4];
        	public AttackType attackType = AttackType.Normal;
        };
        
        public enum AttackType{
        	Normal,
        	PokemonPower, // for classic, just text no symbol, but on fire type we need to use a different color
        	PokemonBody // not supported on classic
        };
    }
    
}
