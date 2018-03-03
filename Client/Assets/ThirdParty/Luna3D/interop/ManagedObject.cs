
namespace Luna3D
{
    /// <summary>
    /// Base class for objects that contain references to unmanaged resources.
    /// </summary>
    public abstract class ManagedObject : IManagedObject
    {
        private readonly AllocType mResourceType;

        /// <summary>
        /// The type of unmanaged resources held by the object.
        /// </summary>
        public AllocType ResourceType { get { return mResourceType; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceType">The type of unmanaged resource.</param>
        public ManagedObject(AllocType resourceType)
        {
            this.mResourceType = resourceType;
        }

   
        public abstract void RequestDisposal();

        public abstract bool IsDisposed { get; }
    }
}
