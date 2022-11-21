using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationTextMeshSized : MonoBehaviour
{
    public string id = "";

    public TextMesh txt_mesh;

    private void Start()
    {

        fetch();

        //LocalizationManager.manager.onLanguageChange += onLanguageChange;
    }

    protected void fetch()
    {

        if (txt_mesh == null)
        {
            txt_mesh = GetComponent<TextMesh>();
            if (txt_mesh == null) Debug.LogError("Missing textMesh on localizationPlug", this);
        }

    }


    virtual public void onLanguageChange(string newLang)
    {
        if (id == null)
        {
            Debug.LogError("no id for " + name, transform);
            return;
        }

        fetch();

        string content = LocalizationManager.getContent(id);

        content = content.Replace("\\n", "\n");

        if (txt_mesh != null)
        {
            txt_mesh.text = content;
        }
    }

    private void OnDestroy()
    {
        //if (LocalizationManager.manager != null) LocalizationManager.manager.onLanguageChange -= onLanguageChange;
    }

}
