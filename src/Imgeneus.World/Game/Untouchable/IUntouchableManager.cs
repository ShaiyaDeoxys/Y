namespace Imgeneus.World.Game.Untouchable
{
    public interface IUntouchableManager
    {
        void Init(int ownerId);

        /// <summary>
        /// When true, killable is untouchable and can not be killed.
        /// </summary>
        bool IsUntouchable { get; set; }
    }
}
