using UnityEngine;

namespace fwp.scenes.examples
{

    public class CustomProfil : fwp.scenes.SceneProfil
    {

        public CustomProfil(string categoryUid) : base(categoryUid)
        {
            sortByPattern(
                new string[] { "logic", "debug" },
                new int[] { 2, 1 });

            Debug.Log("<b>" + Category + "</b>");
            for (int i = 0; i < layers.Count; i++)
            {
                Debug.Log(" --> " + layers[i].Name);
            }

        }

    }

}