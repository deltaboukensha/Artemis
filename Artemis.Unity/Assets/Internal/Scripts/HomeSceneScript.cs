using UnityEngine;
using System.Collections;
using Vuforia;
using System;
using UnityEngine.SceneManagement;

public class HomeSceneScript : MonoBehaviour {
	private bool mVuforiaStarted = false;
	private bool mAutofocusEnabled = false;
	private bool mFlashTorchEnabled;
	private CameraDevice.CameraDirection mActiveDirection;

	// Use this for initialization
	void Start () {
		Debug.Log("Start");
		VuforiaAbstractBehaviour vuforia = FindObjectOfType<VuforiaAbstractBehaviour>();
		vuforia.RegisterVuforiaStartedCallback(OnVuforiaStarted);
		vuforia.RegisterOnPauseCallback(OnVuforiaPaused);

		Physics.gravity = Quaternion.AngleAxis(-45, Vector3.right) * new Vector3(0, -9.81f, 0);
		Debug.Log("Gravity: " + Physics.gravity);
	}

	void Update()
	{
        //if(Input.GetKeyDown(KeyCode.Escape))
        //{
        //    //SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % (SceneManager.sceneCount + 1));
        //}

        //      foreach(var touch in Input.touches)
        //      {
        //          var ray = Camera.main.ScreenPointToRay(touch.position);
        //          var hit = Physics.Raycast(ray);
        //      }

        //      if(Input.GetMouseButtonDown(0))
        //      {
        //          var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //          RaycastHit hitInfo;
        //          var hit = Physics.Raycast(ray, out hitInfo);

        //          if(hit)
        //          {
        //              Debug.Log(hitInfo);
        //              hitInfo.rigidbody.angularVelocity += new Vector3(0, 1);
        //          }

        //          Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, Time.deltaTime, false);
        //      }
    }

	private void OnVuforiaStarted()
	{
		Debug.Log("OnVuforiaStarted");
		mVuforiaStarted = true;
		SwitchAutofocus(true);
	}

	private void OnVuforiaPaused(bool paused)
	{
		Debug.Log("OnVuforiaPaused");

		bool appResumed = !paused;
		if(appResumed && mVuforiaStarted)
		{
			// Restore original focus mode when app is resumed
			if(mAutofocusEnabled)
				CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
			else
				CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);

			// Set the torch flag to false on resume (cause the flash torch is switched off by the OS automatically)
			mFlashTorchEnabled = false;
		}
	}

	// Update is called once per frame


	public bool IsFlashTorchEnabled()
	{
		return mFlashTorchEnabled;
	}

	public void SwitchFlashTorch(bool ON)
	{
		if(CameraDevice.Instance.SetFlashTorchMode(ON))
		{
			Debug.Log("Successfully turned flash " + ON);
			mFlashTorchEnabled = ON;
		}
		else
		{
			Debug.Log("Failed to set the flash torch " + ON);
			mFlashTorchEnabled = false;
		}
	}

	public bool IsAutofocusEnabled()
	{
		return mAutofocusEnabled;
	}

	public void SwitchAutofocus(bool ON)
	{
		if(ON)
		{
			if(CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO))
			{
				Debug.Log("Successfully enabled continuous autofocus.");
				mAutofocusEnabled = true;
			}
			else
			{
				// Fallback to normal focus mode
				Debug.Log("Failed to enable continuous autofocus, switching to normal focus mode");
				mAutofocusEnabled = false;
				CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
			}
		}
		else
		{
			Debug.Log("Disabling continuous autofocus (enabling normal focus mode).");
			mAutofocusEnabled = false;
			CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
		}
	}

	public void TriggerAutofocusEvent()
	{
		// Trigger an autofocus event
		CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);

		// Then restore original focus mode
		StartCoroutine(RestoreOriginalFocusMode());
	}

	public void SelectCamera(CameraDevice.CameraDirection camDir)
	{
		if(RestartCamera(camDir))
		{
			mActiveDirection = camDir;

			// Upon camera restart, flash is turned off
			mFlashTorchEnabled = false;
		}
	}

	public bool IsFrontCameraActive()
	{
		return (mActiveDirection == CameraDevice.CameraDirection.CAMERA_FRONT);
	}

	private IEnumerator RestoreOriginalFocusMode()
	{
		// Wait 1.5 seconds
		yield return new WaitForSeconds(1.5f);

		// Restore original focus mode
		if(mAutofocusEnabled)
			CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
		else
			CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
	}

	private bool RestartCamera(CameraDevice.CameraDirection direction)
	{
		ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
		if(tracker != null)
			tracker.Stop();

		CameraDevice.Instance.Stop();
		CameraDevice.Instance.Deinit();

		if(!CameraDevice.Instance.Init(direction))
		{
			Debug.Log("Failed to init camera for direction: " + direction.ToString());
			return false;
		}
		if(!CameraDevice.Instance.Start())
		{
			Debug.Log("Failed to start camera for direction: " + direction.ToString());
			return false;
		}

		if(tracker != null)
		{
			if(!tracker.Start())
			{
				Debug.Log("Failed to restart the Tracker.");
				return false;
			}
		}

		return true;
	}

    public void QuitApplication()
    {
        Debug.Log("QuitApplication");
        Application.Quit();
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }

    public void Target1Clicked()
    {
        Debug.Log("Target1Clicked");
    }
}
