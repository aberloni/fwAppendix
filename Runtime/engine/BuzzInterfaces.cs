using UnityEngine;

namespace fwp.buzz
{

    /// <summary>
    /// buzz:bee
    /// locking if subbed
    /// </summary>
    public interface iBee
    {
        /// <summary>
        /// showed on screen while this bee is subbed
        /// </summary>
        public string stringifyBeeState();
    }

    /// <summary>
    /// + feedback why locking
    /// </summary>
    public interface iBeeDyn : iBee
    {
        /// <summary>
        /// method to modify bee state
        /// </summary>
        public void setBuzz(string msg);

        public void clearBuzz(); // reset bee state
    }
}