using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Imgeneus.World.Game.AdditionalInfo
{
    public class AdditionalInfoManager : IAdditionalInfoManager
    {
        private readonly ILogger<AdditionalInfoManager> _logger;
        private readonly ICharacterConfiguration _characterConfig;
        private int _ownerId;

        public AdditionalInfoManager(ILogger<AdditionalInfoManager> logger, ICharacterConfiguration characterConfiguration)
        {
            _logger = logger;
            _characterConfig = characterConfiguration;

#if DEBUG
            _logger.LogDebug("AdditionalInfoManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~AdditionalInfoManager()
        {
            _logger.LogDebug("AdditionalInfoManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId, Race race, CharacterProfession profession, byte hair, byte face, byte height, Gender gender)
        {
            _ownerId = ownerId;

            Race = race;
            Class = profession;
            Hair = hair;
            Face = face;
            Height = height;
            Gender = gender;
        }

        #endregion

        public Race Race { get; set; }
        public CharacterProfession Class { get; set; }
        public byte Hair { get; set; }
        public byte Face { get; set; }
        public byte Height { get; set; }
        public Gender Gender { get; set; }

        public CharacterStatEnum GetPrimaryStat()
        {
            var defaultStat = _characterConfig.DefaultStats.First(s => s.Job == Class);

            switch (defaultStat.MainStat)
            {
                case 0:
                    return CharacterStatEnum.Strength;

                case 1:
                    return CharacterStatEnum.Dexterity;

                case 2:
                    return CharacterStatEnum.Reaction;

                case 3:
                    return CharacterStatEnum.Intelligence;

                case 4:
                    return CharacterStatEnum.Wisdom;

                case 5:
                    return CharacterStatEnum.Luck;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
