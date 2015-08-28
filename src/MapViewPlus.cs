using System;
using System.Collections;
using UnityEngine;
using KSP;

namespace MapViewPlus
{
	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class MapViewPlus : MonoBehaviour
	{
		public static ApplicationLauncherButton MVPButton = null;

		public bool showGUI;
		public Rect windowRect;
		public Vector2 scrollPos;
		public System.Collections.Generic.List<MapObject> objects;

		PlanetariumCamera pCamera;

		public void Awake(){

			showGUI = false;
			pCamera = PlanetariumCamera.fetch;
			windowRect = new Rect (20, 40, 200, 500);
			scrollPos = new Vector2 (0, 0);
			objects = pCamera.targets;
			#if DEBUG
			Debug.Log ("[MVP] Registering Callback");
			Debug.Log ("[MVP] Called awake");
			Debug.Log("[MVP] listing the targets and indices");
			for(int i = 0; i < objects.Count; i++)
				Debug.Log("[MVP] " + objects[i].name + " has index " + i);
			#endif

			//Prepare GUI and ApplicationLauncherButton
			if (MVPButton == null) {
				#if DEBUG
				Debug.Log ("[MVP] Passed null check");
				#endif
			}
		}

		public void Start ()
		{
			AddAppButton ();
			MapView.OnEnterMapView += SetUpMapView;
			MapView.OnExitMapView += DestroyAddon;
		}

		public void OnDestroy(){
			RemoveAppButton ();
		
			#if DEBUG
			Debug.Log("[MVP] Calling OnDestroy");
			#endif
		}


		public void AddAppButton ()
		{
			if (ApplicationLauncher.Ready && MVPButton == null) {
				#if DEBUG
				Debug.Log ("[MVP] AppLauncher was ready, attempting button");
				#endif

				//Create button and store for later deletion
				MVPButton = ApplicationLauncher.Instance.AddModApplication (this.OnAppLaunchTrue, 
					this.OnAppLauncherFalse, 
					this.OnAppLauncherNull, 
					this.OnAppLauncherNull, 
					this.OnAppLauncherNull,
					this.OnAppLauncherNull,
					ApplicationLauncher.AppScenes.MAPVIEW, 
					(Texture)GameDatabase.Instance.GetTexture ("MVP/Texture/StockIcon", false));

				#if DEBUG
				Debug.Log ("[MVP] Checking button address " + MVPButton);
				Debug.Log ("[MVP] Created a button");
				#endif
			}
		}

		public void RemoveAppButton() {
			ApplicationLauncher.Instance.RemoveModApplication (MVPButton);
			MapViewPlus.MVPButton = null;
			#if DEBUG
			Debug.Log ("[MVP] Just called to remove button");
			#endif
		}

		void OnGUI(){
			if (this.showGUI) {
				if (HighLogic.LoadedScene == GameScenes.FLIGHT && MapView.MapIsEnabled)
					windowRect = GUILayout.Window (03755, windowRect, this.SelectionWindow, "Select Focus");
				else {
					this.showGUI = false;
				}
			}
		
		}

		void SelectionWindow (int windowID) {
			scrollPos = GUILayout.BeginScrollView (scrollPos,
				GUILayout.ExpandWidth (false),
				GUILayout.MaxHeight (GameSettings.SCREEN_RESOLUTION_HEIGHT * 0.75f)
			);
			int vesselI = -1;
			for(int manueverI = this.objects.Count - 1; manueverI >= 0; manueverI--) {
				MapObject aMapObject = this.objects [manueverI];

				if (aMapObject.type == MapObject.MapObjectType.VESSEL) {
					vesselI = manueverI;
					if(vesselI != this.objects.Count - 1) {
						for (int i = vesselI + 1; i < this.objects.Count; i++)
							if (GUILayout.Button (this.objects[i].name))
								pCamera.SetTarget (this.objects[i]);
					}

					break;
				}
					
			}

			for (int celestialI = 0; celestialI < vesselI; celestialI++) {
				MapObject aMapObject = this.objects [celestialI];
				if (aMapObject.type == MapObject.MapObjectType.MANEUVERNODE)
					break;

				if (GUILayout.Button (aMapObject.name))
					pCamera.SetTarget (aMapObject);
			}

			if (this.objects[vesselI].type == MapObject.MapObjectType.VESSEL && GUILayout.Button ("Return to Ship"))
				pCamera.SetTarget (this.objects [vesselI]);

			GUILayout.EndScrollView();
			GUI.DragWindow ();
		}

		public void SetUpMapView(){
			#if DEBUG
			Debug.Log ("[MVP] Auto navball toggle");
			for(int i = 0; i < objects.Count; i++)
				Debug.Log("[MVP] " + objects[i].name + " has index " + i);
			#endif

			//Auto expand the navball
			try {
				FlightUIModeController.Instance.navBall.Expand ();
				MapView.fetch.MapCollapse_navBall.Expand ();
			}

			catch (Exception e) {
				#if DEBUG
				Debug.Log ("NavBall unaccessible");
				Debug.Log (e.ToString ());
				#endif
			}
		}

		public void DestroyAddon() {
			GameObject.Destroy (this);
		}

		void OnAppLaunchTrue ()
		{
			this.showGUI = true;
		}

		void OnAppLauncherFalse ()
		{
			this.showGUI = false;
		}

		void OnAppLauncherNull () {}
	}
}

