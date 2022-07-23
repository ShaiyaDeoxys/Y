using System.Collections.Generic;

namespace Imgeneus.Game.Crafting
{
    public class CraftInfo
    {
        public byte Type { get; set; }
        public byte TypeId { get; set; }

        public List<Recipe> Recipes { get; set; }
    }

    public class Recipe
    {
        public byte Type { get; set; }
        public byte TypeId { get; set; }
        public byte Count { get; set; }

        public float Rate { get; set; }

        public List<Ingredient> Ingredients { get; set; }
    }

    public class Ingredient
    {
        public byte Type { get; set; }
        public byte TypeId { get; set; }
        public byte Count { get; set; }
    }
}