namespace VoidSurvivor.Core
{
    /// <summary>
    /// Optional lifecycle hook for pooled MonoBehaviour objects (M7.1).
    /// Implement on objects managed by <see cref="ObjectPool{T}"/> to run
    /// reset logic when the object is spawned from / released back to the pool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called when the object is taken from the pool (after SetActive(true)).</summary>
        void OnSpawn();

        /// <summary>Called when the object is returned to the pool (before SetActive(false)).</summary>
        void OnDespawn();
    }
}
