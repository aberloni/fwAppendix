﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Linq;

namespace fwp.screens
{
	using fwp.scenes;

	public class ScreensManager
	{
		static public bool isVerbose => ScreensVerbosity.verbose;

		/// <summary>
		/// list of all opened screens
		/// </summary>
		static List<ScreenObject> screens = new List<ScreenObject>();

		//usual screen names
		public enum ScreenNameGenerics
		{
			home, // home menu
			ingame, // ingame interface (ui)
			pause, // pause screen
			result, // end of round screen, result of round
			loading
		};

		/// <summary>
		/// called during ScreenObject AWAKE
		/// </summary>
		static public void subScreen(ScreenObject so)
		{
			if (screens.Contains(so)) return;

			screens.Add(so);

			if (isVerbose)
				Debug.Log(so.name + "       is now subscribed to screens");
		}

		/// <summary>
		/// destroy
		/// </summary>
		static public void unsubScreen(ScreenObject so)
		{
			if (!screens.Contains(so)) return;

			screens.Remove(so);

			if (isVerbose)
				Debug.Log(so.name + "       is now removed from screens (screen destroy)");
		}

		/// <summary>
		/// not opti
		/// </summary>
		static protected void fetchScreens()
		{
			if (screens == null) screens = new List<ScreenObject>();
			screens.Clear();
			screens.AddRange(fwp.appendix.qh.gcs<ScreenObject>());
		}

		static public bool hasOpenScreenOfType(ScreenObject.ScreenType type)
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
			return screens.Select(x => x).Where(x => x.isVisible()).FirstOrDefault();
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
				if (screens[i].isVisible())
				{
					return screens[i];
				}
			}

			return null;
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

		static public void unloadScreen(string nm)
		{
			ScreenObject so = getScreen(nm);
			if (so != null)
			{
				Debug.Log("unloading screen | asked name : " + nm);
				so.unload();
			}
		}

		/// <summary>
		/// deprecated
		/// </summary>
		static bool checkCompatibility(string nm)
		{
			string[] nms = System.Enum.GetNames(typeof(ScreenObject.ScreenType));
			for (int i = 0; i < nms.Length; i++)
			{
				if (nm.StartsWith(nms[i])) return true;
			}

			Debug.LogWarning("given screen " + nm + " is not compatible with screen logic ; must start with type in name");

			return false;
		}

		/// <summary>
		/// if present return
		/// return null is needs loading (use callback)
		/// </summary>
		static public void load(string nm, Action<ScreenObject> onCompletion = null)
		{
			ScreenObject so = getScreen(nm);
			if (so != null)
			{
				if (isVerbose)
					Debug.Log($"{getStamp()} | open:<b>{nm}</b> | already present");

				onCompletion?.Invoke(so);
				return;
			}

			if (isVerbose)
				Debug.Log($"{getStamp()} | open:<b>{nm}</b> | not already present, load");

			loadMissingScreen(nm, (tar) =>
			{
				so = tar;
				onCompletion?.Invoke(so);
			});
		}

		static public void open(System.Enum enu, Action<ScreenObject> onComplete = null) => open(enu.ToString(), onComplete);
		static public void open(string nm, Action<ScreenObject> onComplete) => open(nm, string.Empty, onComplete);

		/// <summary>
		/// will load AND open()
		/// </summary>
		static public void open(string nm, string filterName = "", Action<ScreenObject> onLoadCompleted = null)
		{
			load(nm, (tar) =>
			{
				onLoadCompleted?.Invoke(tar);
				changeScreenPresence(nm, true, filterName);
			});
		}

		/// <summary>
		/// will close or open the screen
		/// </summary>
		static void changeScreenPresence(string scName, bool state, string hidesContainsFilter = "")
		{
			if (!Application.isPlaying) fetchScreens();

			//Debug.Log("opening " + scName + " (filter ? " + filter + ")");

			ScreenObject selected = getScreen(scName);
			if (selected == null)
			{
				Debug.LogWarning($"changeScreenVisibleState:{scName} : this ScreenObject doesn't exist ?");
				return;
			}

			bool hideOthers = selected.tags.HasFlag(ScreenObject.ScreenTags.hideOtherLayerOnShow);

			//Debug.Log(selected.name + " visibilty to " + state+" (filter ? "+containsFilter+" | dont hide other ? "+selected.dontHideOtherOnShow+" => hide others ? "+hideOthers+")");

			//on opening a specific screen we close all other non sticky screens
			if (hideOthers && state)
			{
				for (int i = 0; i < screens.Count; i++)
				{
					if (screens[i] == selected) continue;

					// must filter
					if (!string.IsNullOrEmpty(hidesContainsFilter))
					{
						// part of filter : do nothing
						if (screens[i].name.Contains(hidesContainsFilter)) continue;
					}

					screens[i].close();
					//Debug.Log("  L "+screens[i].name + " hidden");
				}

			}

			if (state)
			{
				selected.open();
			}
			else
			{
				selected.close(); // stickies won't hide
			}

		}

		static public void close(ScreenNameGenerics scName, string filter = "") => close(scName.ToString(), filter);

		/// <summary>
		/// </summary>
		/// <param name="nameEnd"></param>
		/// <param name="force">if screen is sticky</param>
		static public void close(string nameEnd, string filter = "")
		{
			changeScreenPresence(nameEnd, false, filter);
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

				screens[i].close();
			}
		}

		/// <summary>
		/// leader will be the only screen visible
		/// only works for overlays
		/// </summary>
		static public void setStandby(ScreenObject leader)
		{
			for (int i = 0; i < screens.Count; i++)
			{
				if (screens[i].type == ScreenObject.ScreenType.overlay)
				{
					screens[i].setStandby(leader);
				}
			}
		}

		static protected void loadMissingScreen(string screenName, Action<ScreenObject> onComplete)
		{
			// don't, let the context choose to show it or not
			//ScreenLoading.showLoadingScreen();

			// first search if already exists
			ScreenObject so = getScreen(screenName);
			if (so != null)
			{
				onComplete(so);
				return;
			}

			if (isVerbose)
				Debug.Log("loadMissingScreen | screen to open : <b>" + screenName + "</b>");

			SceneLoader.queryScene(screenName, (assoc) =>
			{
				so = getScreen(screenName);
				if (so == null)
				{
					Debug.LogError(getStamp() + " | end of screen loading (name given : " + screenName + ") but no <ScreenObject> returned");
				}
				onComplete(so);
			});

		}

		static string getStamp()
		{
			return "~~ScreensManager~~>";
		}

		/// <summary>
		/// return if loaded/present
		/// </summary>
		static public ScreenObject getScreen(Type screenType, string nameContains = "")
		{
			foreach (var s in screens)
			{
				if (s.IsScreen(screenType, nameContains)) return s;
			}
			return null;
		}

		static public T getScreen<T>(string nameContains = "") where T : ScreenObject => getScreen(typeof(T), nameContains) as T;
		static public ScreenObject getScreen(string nameContains = "") => getScreen(null, nameContains);
		/// <summary>
		/// from screens[]
		/// ( generate new List )
		/// </summary>
		static public T[] getScreens<T>() where T : ScreenObject
		{
			List<T> ret = new();
			foreach (var s in screens)
			{
				if (s == null) continue;
				if (s is T sc) ret.Add(sc);
			}
			return ret.ToArray();
		}
		static public ScreenObject[] getScreens(Type screenType)
		{
			List<ScreenObject> ret = new();
			foreach (var s in screens)
			{
				if (s == null) continue;
				if (s.IsScreen(screenType)) ret.Add(s);
			}
			return ret.ToArray();
		}
		static public ScreenObject[] getScreens() => getScreens<ScreenObject>();
	}

}
