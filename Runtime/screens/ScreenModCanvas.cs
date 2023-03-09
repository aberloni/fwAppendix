using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace fwp.screens
{
    public class ScreenModCanvas : ScreenMod
    {

        protected Canvas[] _canvas;
        protected Canvas _mainCanvas;

        protected RectTransform _rt;

        public ScreenModCanvas(ScreenObject screen) : base(screen)
        {
            _canvas = screen.GetComponentsInChildren<Canvas>();
            Debug.Assert(_canvas.Length > 0, "no canvas for screen ui ?");

            _mainCanvas = _canvas[0];

            _rt = screen.GetComponent<RectTransform>();

        }

        /// <summary>
        /// force canvas to use a specific camera
        /// </summary>
        public void setupForUiCamera()
        {
            Camera uiCam = fwp.appendix.AppendixUtils.gc<Camera>("camera-ui");
            if (_mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                _mainCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                _mainCanvas.worldCamera = uiCam;
            }
            
        }

        public bool hasCanvas()
        {
            return _mainCanvas != null;
        }

        public Canvas getDefaultCanvas()
        {
            return _mainCanvas;
        }

        public Canvas getCanvasByName(string nm)
        {
            for (int i = 0; i < _canvas.Length; i++)
            {
                if (_canvas[i].name.Contains(nm)) return _canvas[i];
            }
            Debug.LogWarning("~ScreenObject~ getCanvas() no canvas named <b>" + nm + "</b>");
            return null;
        }

        public void setCanvasVisibility(string nm, bool flag)
        {
            for (int i = 0; i < _canvas.Length; i++)
            {
                if (_canvas[i].name.Contains(nm))
                {
                    //Debug.Log("  L found canvas '"+nm+"' => visibility to "+flag);
                    _canvas[i].enabled = flag;
                }
            }
        }

        public bool toggleVisible(bool flag)
        {
            if (_canvas == null) Debug.LogError("no canvas ? for " + owner.name, owner.gameObject);

            //Debug.Log("toggle screen " + name + " visibility to " + flag + " | " + _canvas.Length + " canvas");

            //Debug.Log(name + " visibility ? " + flag+" for "+_canvas.Length+" canvas");

            //show all canvas of screen
            for (int i = 0; i < _canvas.Length; i++)
            {
                //Debug.Log(name + "  " + _canvas[i].name);
                if (_canvas[i].enabled != flag)
                {
                    //Debug.Log("  L canvas " + _canvas[i].name + " toggle to " + flag);
                    _canvas[i].enabled = flag;
                }
            }

            //Debug.Log(name+" , "+flag, gameObject);

            return flag;
        }

        public bool isVisible()
        {
            for (int i = 0; i < _canvas.Length; i++)
            {
                if (_canvas[i].enabled) return true;
            }
            return false;
        }

        static public Canvas getCanvas(string screenName, string canvasName)
        {
            ScreenObject screen = (ScreenObject)ScreensManager.getScreen(screenName);

            Debug.Assert(screen != null, screenName + " is not ui related");

            return screen.canvas.getCanvasByName(canvasName);
        }

    }
}
