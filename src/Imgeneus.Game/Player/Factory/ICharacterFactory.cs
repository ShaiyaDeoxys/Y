using System.Threading.Tasks;

namespace Imgeneus.World.Game.Player
{
    public interface ICharacterFactory
    {
        /// <summary>
        /// Creates player instance from db character id.
        /// </summary>
        public Task<Character> CreateCharacter(int userId, uint id);
    }
}
