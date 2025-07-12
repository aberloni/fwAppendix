
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
		/// called when object is actually removed from actives listing by factory
		/// won't be called if object was not present in actives listing (ie : for duplicate calls)
		/// 
		/// to recycle an object properly : use factory routines and wait for this callback
		/// </summary>
		void OnRecycled();
	}
}