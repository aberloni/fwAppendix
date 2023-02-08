using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

namespace fwp.screens
{
    using fwp.scenes;

    public class ScreensManager
    {

        static protected List<ScreenObject> screens = new List<ScreenObject>();

        public enum ScreenType
        {
            undefined,
            menu,
            overlay, // ingame overlays
        }

        /// <summary>
        /// cumulative states for screens
        /// </summary>
        [System.Flags]
        public enum ScreenTags
        {
            none = 0,
            pauseIngameUpdate = 1, // screen that pauses gameplay
            blockIngameInput = 2 // screen that lock inputs
        };

        //usual screen names
        public enum ScreenNameGenerics
        {
            home, // home menu
            ingame, // ingame interface (ui)
            pause, // pause screen
            result, // end of round screen, result of round
            loading
        };

        static public void subScreen(ScreenObject so)
        {
            if (screens.Contains(so)) return;

            screens.Add(so);
            Debug.Log(so.name + "       is now subscribed to screens");
        }

        static public void unsubScreen(ScreenObject so)
        {
            if (!screens.Contains(so)) return;

            screens.Remove(so);
            Debug.Log(so.name + "       is now removed from screens (screen destroy)");
        }

        static protected void fetchScreens()
        {
            if (screens == null) screens = new List<ScreenObject>();
            screens.Clear();
            screens.AddRange(GameObject.FindObjectsOfType<ScreenObject>());
        }

        static public bool hasOpenScreenOfType(ScreenType type)
        {
            ScreenObject so = getOpenedScreen();
            if (so == null) return false;
            return so.type == type;
        }

        /// <summary>
        /// returns NON-STICKY visible screen
        /// </summary>
        /// <returns></returns>
        static public ScreenObject getOpenedScreen()
        {
            if (screens == null) return null;
            return screens.Select(x => x).Where(x => !x.sticky && x.isVisible()).FirstOrDefault();
        }

        /// <summary>
        /// returns NON-STICKY visible screen
        /// </summary>
        /// <returns></returns>
        static public ScreenObject getFirstOpenedScreen()
        {
            //fetchScreens();

            for (int i = 0; i < screens.Count; i++)
            {
                if (!screens[i].sticky && screens[i].isVisible())
                {
                    return screens[i];
                }
            }

            return null;
            //return screens.Select(x => x).Where(x => !x.sticky && x.isVisible()).FirstOrDefault();
        }

        static public List<ScreenObject> getLoadedScreens()
        {
            return screens;
        }

        /// <summary>
        /// si un screen visible contient "nm"
        /// </summary>
        static public bool isAScreenContainNameOpened(string nm)
        {
            List<ScreenObject> sos = getLoadedScreens();
            for (int i = 0; i < sos.Count; i++)
            {
                if (sos[i].isVisible())
                {
                    if (sos[i].name.Contains(nm)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// to return the screen if already open
        /// to call the screen use open() flow instead
        /// </summary>
        static public ScreenObject getScreen(ScreenNameGenerics nm)
        {
            return getScreen(nm.ToString());
        }
        static public ScreenObject getScreen(System.Enum enu) => getScreen(enu.ToString());
        static public ScreenObject getScreen(string nm)
        {
            if(screens.Count <= 0)
            {
                //Debug.LogWarning("asking for a screen " + nm + " but screen count is 0");
                return null;
            }

            ScreenObject so = screens.Select(x => x).Where(x => x.isScreenOfSceneName(nm)).FirstOrDefault();

            if (so == null)
            {
                Debug.LogWarning($"{getStamp()} getScreen({nm}) <color=red>no screen that END WITH that name</color> (screens count : {screens.Count})");

                for (int i = 0; i < screens.Count; i++)
                {
                    Debug.Log("  #"+i+","+screens[i]);
                }
            }

            return so;
        }

        static public void unloadScreen(string nm)
        {
            ScreenObject so = getScreen(nm);
            if (so != null)
            {
                Debug.Log("unloading screen | asked name : " + nm);
                so.unload();
            }
        }

        static bool checkCompatibility(string nm)
        {
            string[] nms = System.Enum.GetNames(typeof(ScreenType));
            for (int i = 0; i < nms.Length; i++)
            {
                if (nm.StartsWith(nms[i])) return true;
            }

            Debug.LogWarning("given screen " + nm + " is not compatible with screen logic ; must start with type in name");

            return false;
        }

        static public ScreenObject open(System.Enum enu, Action<ScreenObject> onComplete = null) => open(enu.ToString(), onComplete);
        static public ScreenObject open(string nm, Action<ScreenObject> onComplete) { return open(nm, string.Empty, onComplete); }

        /// <summary>
        /// best practice : should never call a screen by name but create a contextual enum
        /// this function won't return a screen that is not already loaded
        /// </summary>
        static public ScreenObject open(string nm, string filterName = "", Action<ScreenObject> onComplete = null)
        {
            Debug.Log($"{getStamp()} | opening screen of name : <b>{nm}</b> , filter ? {filterName}");

            if(!checkCompatibility(nm))
            {
                onComplete?.Invoke(null);
                return null;
            }

            ScreenObject so = getScreen(nm);

            if (so != null)
            {
                // show
                changeScreenVisibleState(nm, true, filterName);
                
                onComplete?.Invoke(so);

                return so;
            }

            //si le screen existe pas on essaye de le load
            loadMissingScreen(nm, delegate (ScreenObject loadedScreen)
            {
                //Debug.Log("  ... missing screen '" + nm + "' is now loaded, opening");

                loadedScreen.onScreenLoaded();
                //changeScreenVisibleState(nm, true, filterName);

                onComplete?.Invoke(loadedScreen);
            });

            return null;
        }

        static protected void changeScreenVisibleState(string scName, bool state, string containsFilter = "", bool force = false)
        {
            fetchScreens();

            //Debug.Log("opening " + scName + " (filter ? " + filter + ")");

            ScreenObject selected = getScreen(scName);
            if (selected == null)
            {
                Debug.LogWarning("trying to change visibility of screen " + scName + " but this ScreenObject doesn't exist");
                return;
            }

            bool hideOthers = !selected.dontHideOtherOnShow;

            //Debug.Log(selected.name + " visibilty to " + state+" (filter ? "+containsFilter+" | dont hide other ? "+selected.dontHideOtherOnShow+" => hide others ? "+hideOthers+")");

            //on opening a specific screen we close all other non sticky screens
            if (hideOthers && state)
            {
                for (int i = 0; i < screens.Count; i++)
                {
                    if (screens[i] == selected) continue;

                    //do nothing with filtered screen
                    if (containsFilter.Length > 0 && screens[i].name.Contains(containsFilter)) continue;

                    screens[i].hideInstant();
                    //Debug.Log("  L "+screens[i].name + " hidden");
                }

            }

            if (state) selected.showInstant();
            else
            {
                if (force) selected.forceHide();
                else selected.hideInstant(); // stickies won't hide
            }

        }

        static public void close(ScreenNameGenerics scName) { close(scName.ToString()); }
        static public void close(string scName) { close(scName, "", false); }
        static public void close(ScreenNameGenerics scName, bool force = false) { close(scName.ToString(), "", force); }
        static public void close(ScreenNameGenerics scName, string filter = "", bool force = false) { close(scName.ToString(), filter, force); }

        /// <summary>
        /// </summary>
        /// <param name="nameEnd"></param>
        /// <param name="force">if screen is sticky</param>
        static protected void close(string nameEnd, string filter = "", bool force = false)
        {
            changeScreenVisibleState(nameEnd, false, filter, force);
        }

        [ContextMenu("kill all")]
        public void killAll(string filterName = "")
        {
            fetchScreens();

            for (int i = 0; i < screens.Count; i++)
            {
                if (filterName.Length > 0)
                {
                    if (screens[i].name.EndsWith(filterName)) continue;
                }

                screens[i].hide();
            }
        }

        static public void setStandby(ScreenObject leader)
        {
            for (int i = 0; i < screens.Count; i++)
            {
                if(screens[i].type == ScreenType.overlay)
                {
                    screens[i].setStandby(leader);
                }
            }
        }

        static protected void loadMissingScreen(string screenName, Action<ScreenObject> onComplete)
        {
            ScreenLoading.showLoadingScreen();

            if(!checkCompatibility(screenName))
            {
                onComplete?.Invoke(null);
                return;
            }

            // first search if already exists
            ScreenObject so = getScreen(screenName);
            if (so != null)
            {
                onComplete(so);
                return;
            }

            Debug.Log("screen to open : <b>" + screenName + "</b> is not loaded");

            SceneLoader.queryScene(screenName, delegate (Scene sc)
            {
                so = getScreen(screenName);
                if (so == null)
                {
                    Debug.LogError(getStamp() + " | end of screen loading (name given : " + screenName + ") but no <ScreenObject> returned");
                }
                onComplete(so);
            });

        }

        /// <summary>
        /// just display, no state change
        /// </summary>
        /// <param name="state"></param>
        static public void callPauseScreen(bool state)
        {

            if (state) ScreensManager.open(ScreensManager.ScreenNameGenerics.pause);
            else ScreensManager.close(ScreensManager.ScreenNameGenerics.pause);

        }

        static string getStamp()
        {
            return "~~ScreensManager~~";
        }

    }

}
