using System;
using System.Collections;
using UnityEngine;
using KSP;

namespace MapViewPlus
{
	[KSPAddon (KSPAddon.Startup.MainMenu, true)]
	public class MapViewPlus : MonoBehaviour
	{
		public static ApplicationLauncherButton MVPButton = null;

		public bool showGUI;
		public Rect windowRect;
		public System.Collections.Generic.List<MapObject> objects;

		PlanetariumCamera pCamera;

		public void Awake(){
			GameObject.DontDestroyOnLoad (this.gameObject);
			showGUI = false;
			pCamera = PlanetariumCamera.fetch;
			windowRect = new Rect (20, 20, 200, 500);
			objects = pCamera.targets;

			#if DEBUG
			Debug.Log ("[MVP] Registering Callback");
			#endif

			Debug.Log ("[MVP] Called awake");

			//Prepare GUI and ApplicationLauncherButton
			if (MVPButton == null) {

				Debug.Log ("[MVP] Passed null check");
				GameEvents.onGUIApplicationLauncherReady.Add(AddAppButton);
				GameEvents.onGUIApplicationLauncherDestroyed.Add(RemoveAppButton);
			}
		}

		public void Start ()
		{
			MapView.OnEnterMapView += SetUpMapView;
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

				Debug.Log ("[MVP] Checking button address " + MVPButton);
				Debug.Log ("[MVP] Created a button");
			}
		}

		public void RemoveAppButton() {
			//ApplicationLauncher.Instance.RemoveApplication (MVPButton);
			Debug.Log ("[MVP] Just called to remove button");
		}

		void OnGUI(){
			if (this.showGUI) {
				Debug.Log ("Trying to draw :(");
				windowRect = GUILayout.Window (03755, windowRect, this.SelectionWindow, "Select Focus");
			}
		
		}

		void SelectionWindow (int windowID) {
			GUILayout.BeginVertical ();
			foreach (MapObject mapObject in this.objects)
				if (GUILayout.Button (mapObject.name))
					pCamera.SetTarget (mapObject);
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public void SetUpMapView(){
			#if DEBUG
			Debug.Log ("[MVP] Auto navball toggle");
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

		void OnAppLaunchTrue ()
		{
			Debug.Log ("[MVP] Called onTrue but no GUI?");
			this.showGUI = true;
			Debug.Log ("[MVP] Should have GUI, check bool: " + this.showGUI + "; check position of rect: " + windowRect.position.ToString());
		}

		void OnAppLauncherFalse ()
		{
			Debug.Log ("[MVP] Called onFalse but no GUI?");
			this.showGUI = false;
		}

		void OnAppLauncherNull () {}
	}
}

