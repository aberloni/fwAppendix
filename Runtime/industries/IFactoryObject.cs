
namespace fwp.industries
{
    using facebook;

    /// <summary>
    /// make ref compatible with factories
    /// </summary>
    public interface iFactoryObject : IFacebook
    {

        /// <summary>
        /// the actual name of the object to instantiate
        /// to be able to compare signatures when extracting and recycling
        /// Resources/{facto}/{CandidateName}
        /// </summary>
        string GetCandidateName();

        /// <summary>
        /// when recycle all is called on parent factory
        /// only during event when factory is told to recycling everything
        /// </summary>
        void OnRecycledByFactory();

    }
}