using Imgeneus.Database.Entities;

namespace Imgeneus.World.Game.Friends
{
    public class Friend
    {
        /// <summary>
        /// Character id.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Friend name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Friend job.
        /// </summary>
        public CharacterProfession Job { get; private set; }

        /// <summary>
        /// Friend is online?
        /// </summary>
        public bool IsOnline { get; set; }

        public Friend(uint id, string name, CharacterProfession job, bool isOnline = false)
        {
            Id = id;
            Name = name;
            Job = job;
            IsOnline = isOnline;
        }
    }
}
