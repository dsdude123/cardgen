using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cardgenFunction.Models
{
    public class CardGenerationRequest
    {
        public CardType type;
        public PokemonCard pokemonCard;
        public HearthstoneCard hearthstoneCard;

        public enum CardType
        {
            Pokemon,
            YuGiOh,
            Hearthstone
        };

    }


}
