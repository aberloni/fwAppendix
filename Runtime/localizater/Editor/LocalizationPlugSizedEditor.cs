using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(LocalizationPlugSized))]
public class LocalizationPlugSizedEditor : Editor
{

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
    LocalizationPlugSized _target = (LocalizationPlugSized)target;

    if (GUILayout.Button("Resize"))
    {
      _target.resise();
    }

    if (_target.spriteBounds == null)
    {
      if (GUILayout.Button("Allign Rect To Transform"))
      {
        _target.sizeModif.position = _target.transform.position;
      }
    }

  }

  private void OnSceneGUI()
  {

    LocalizationPlugSized _target = (LocalizationPlugSized)target;

    if (_target.txt_mesh == null) return;

    Undo.RecordObject(target, "Rect change");
    
    Rect resizedRect = _target.sizeModif;

    if (_target.spriteBounds != null)
    {
      resizedRect.position += (Vector2)_target.spriteBounds.transform.position;
      if (_target.alignBottom) {
        Vector3 pos = resizedRect.position;
        pos.y -= _target.spriteBounds.bounds.size.y * 0.5f;
        resizedRect.position = pos;
      }

      resizedRect.size += (Vector2)_target.spriteBounds.bounds.size;
    }

    Rect bounds = RectUtils.ResizeRect(resizedRect);
    if (bounds != resizedRect)
    {
      if (_target.spriteBounds == null)
      {
        //bounds.position = Vector2.zero;
        //bounds.size -= (Vector2)_target.spriteBounds.bounds.size;
      }
      else
      {
        bounds.position -= (Vector2)_target.spriteBounds.transform.position;
        bounds.size -= (Vector2)_target.spriteBounds.bounds.size;
      }

      _target.sizeModif = bounds;
      _target.resise();
    }

  }

}
