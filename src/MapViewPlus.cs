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

		#region Unity Lifecycle
		public void Awake() {

			//Initialize variables for GUI
			showGUI = false;
			windowRect = new Rect (20, 40, 200, 500); //Under Timewarp/Clock
			scrollPos = new Vector2 (0, 0);

			//Get Planetarium's MapObject information (Vessel, bodies, MNodes)
			pCamera = PlanetariumCamera.fetch;
			objects = pCamera.targets;

			#if DEBUG
			//Log all start up info, listing MO's before entering MapView for comparison
			Debug.Log ("[MVP] Registering Callback");
			Debug.Log ("[MVP] Called awake");
			Debug.Log("[MVP] listing the targets and indices before Map View");
			for(int i = 0; i < objects.Count; i++)
				Debug.Log("[MVP] " + objects[i].name + " has index " + i);
			#endif
		}

		public void Start ()
		{
			//Make sure a button isn't hanging around when starting
			if (MVPButton == null) {
				#if DEBUG
				Debug.Log ("[MVP] Passed null check");
				#endif
			}

			//Registers new button when Addon starts up in Flight scene
			AddAppButton ();

			//Prepares auto target
			MapView.OnEnterMapView += SetUpMapView;
		}

		public void OnDestroy(){
			RemoveAppButton ();
			#if DEBUG
			Debug.Log("[MVP] Calling OnDestroy");
			#endif
		}
		#endregion

		#region Stock Toolbar
		public void AddAppButton ()
		{
			if (MVPButton == null) {
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
					(Texture)GameDatabase.Instance.GetTexture ("MapViewPlus/Texture/StockIcon", false));

				#if DEBUG
				Debug.Log ("[MVP] Checking button address " + MVPButton);
				Debug.Log ("[MVP] Created a button");
				#endif
			}
		}
		#endregion
			
		public void RemoveAppButton() {
			ApplicationLauncher.Instance.RemoveModApplication (MVPButton);
			MapViewPlus.MVPButton = null;
			#if DEBUG
			Debug.Log ("[MVP] Just called to remove button");
			#endif
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

		#region GUI
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
			//Feed back the scrollbar's position
			scrollPos = GUILayout.BeginScrollView (scrollPos,
				GUILayout.ExpandWidth (false),
				GUILayout.MaxHeight (GameSettings.SCREEN_RESOLUTION_HEIGHT * 0.75f)
			);

			//Find the active vessel, currently the divider
			int vesselI = -1;
			for(int manueverI = this.objects.Count - 1; manueverI >= 0; manueverI--) {
				MapObject aMapObject = this.objects [manueverI];

				if (aMapObject.type == MapObject.MapObjectType.VESSEL) {
					vesselI = manueverI;
					//Print everything after vessel (MNodes)
					if(vesselI != this.objects.Count - 1) {
						for (int i = vesselI + 1; i < this.objects.Count; i++)
							if (GUILayout.Button (this.objects[i].name))
								pCamera.SetTarget (this.objects[i]);
					}

					break;
				}
					
			}

			//Print everything before the divider (Celestial bodies)
			for (int celestialI = 0; celestialI < vesselI; celestialI++) {
				MapObject aMapObject = this.objects [celestialI];
				if (aMapObject.type == MapObject.MapObjectType.MANEUVERNODE)
					break;

				if (GUILayout.Button (aMapObject.name))
					pCamera.SetTarget (aMapObject);
			}

			//Print the vessel
			if (this.objects[vesselI].type == MapObject.MapObjectType.VESSEL && GUILayout.Button ("Return to Ship"))
				pCamera.SetTarget (this.objects [vesselI]);

			//End GUI code
			GUILayout.EndScrollView();
			GUI.DragWindow ();
		}

		public void SetUpMapView(){
			#if DEBUG
			//Print the MO's again in-case they've updated
			Debug.Log ("[MVP] Auto navball toggle");
			for(int i = 0; i < objects.Count; i++)
				Debug.Log("[MVP] " + objects[i].name + " has index " + i);
			#endif
				
			//Auto expand the navball and activate it
			if (!MapView.ManeuverModeActive) {

				FlightUIModeController.Instance.navBall.Expand ();
				MapView.fetch.MapCollapse_navBall.Expand ();
				MapView.fetch.gameObject.SendMessage ("ToggleManeuverMode", SendMessageOptions.RequireReceiver);

			}


		}

		#endregion
	}
}

