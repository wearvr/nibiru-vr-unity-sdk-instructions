//  Copyright 2016 Nibiru. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System;
using System.Collections.Generic;

using Nvr.Internal;
using System.Collections;

/// The NvrViewer object communicates with the head-mounted display.
/// Is is repsonsible for:
/// -  Querying the device for viewing parameters
/// -  Retrieving the latest head tracking data
/// -  Providing the rendered scene to the device for distortion correction (optional)
///
/// There should only be one of these in a scene.  An instance will be generated automatically
/// by this script at runtime, or you can add one via the Editor if you wish to customize
/// its starting properties.
[AddComponentMenu("NVR/NvrViewer")]
public class NvrViewer : MonoBehaviour {
     // base 2.0.5
    public const string NVR_SDK_VERSION = "2.0.5_20170517";
    // dtr or not 
    public static bool USE_DTR = true;

    private static int _texture_count = 6;
    // 绘制前事件标识
    public static int kPreRenderEvent = 1;

    // 头部角度限制范围
    private float[] headEulerAnglesRange = null;

    /// The singleton instance of the NvrViewer class.
    public static NvrViewer Instance {
    get {
#if UNITY_EDITOR
      USE_DTR = false;
      if (instance == null && !Application.isPlaying) {
        Debug.Log("Create NvrViewer Instance !");
        instance = UnityEngine.Object.FindObjectOfType<NvrViewer>();
      }
#endif
      if (instance == null) {
        Debug.LogError("No NvrViewer instance found.  Ensure one exists in the scene, or call "
            + "NvrViewer.Create() at startup to generate one.\n"
            + "If one does exist but hasn't called Awake() yet, "
            + "then this error is due to order-of-initialization.\n"
            + "In that case, consider moving "
            + "your first reference to NvrViewer.Instance to a later point in time.\n"
            + "If exiting the scene, this indicates that the NvrViewer object has already "
            + "been destroyed.");
      }
      return instance;
    }
  }
  private static NvrViewer instance = null;
  public NvrEye[] eyes = new NvrEye[2];

    /// Generate a NvrViewer instance.  Takes no action if one already exists.
    public static void Create() {
    if (instance == null && UnityEngine.Object.FindObjectOfType<NvrViewer>() == null) {
      Debug.Log("Creating NvrViewer object");
      var go = new GameObject("NvrViewer", typeof(NvrViewer));
      go.transform.localPosition = Vector3.zero;
      // sdk will be set by Awake().
    }
  }

  /// The StereoController instance attached to the main camera, or null if there is none.
  /// @note Cached for performance.
  public static NvrStereoController Controller {
    get {
      Camera camera = Camera.main;
      // Cache for performance, if possible.
      if (camera != currentMainCamera || currentController == null) {
        currentMainCamera = camera;
        currentController = camera.GetComponent<NvrStereoController>();
      }
      return currentController;
    }
  }
  private static Camera currentMainCamera;
  private static NvrStereoController currentController;


    /// Whether to draw directly to the output window (_true_), or to an offscreen buffer
    /// first and then blit (_false_). If you wish to use Deferred Rendering or any
    /// Image Effects in stereo, turn this option off.  A common symptom that indicates
    /// you should do so is when one of the eyes is spread across the entire screen.
    [SerializeField]
    private bool isDirectRender = true;
   
    public bool DirectRender {
        get
        {
            return isDirectRender;
        }
        set {
            if(value != isDirectRender)
            {
                isDirectRender = value;
            }
        }
    }

    /// Determine whether the scene renders in stereo or mono.
    /// _True_ means to render in stereo, and _false_ means to render in mono.
    public bool VRModeEnabled {
    get {
      return vrModeEnabled;
    }
    set {
      if (value != vrModeEnabled && device != null) {
        device.SetVRModeEnabled(value);
      }
      vrModeEnabled = value;
    }
  }

    [SerializeField]
    private bool vrModeEnabled = true;

    // 纹理质量
    [SerializeField]
    public TextureQuality textureQuality = TextureQuality.Good;
    public TextureQuality TextureQuality
    {
        get
        {
            return textureQuality;
        }
        set
        {
            if (value != textureQuality)
            {
                textureQuality = value;
            }
        }
    }

    [SerializeField]
    private bool requestLock = false;
    /// <summary>
    ///  在Unity渲染层面，固定头部姿态
    /// </summary>
    public bool LockHeadTracker
    {
        get
        {
            return requestLock;
        }
        set
        {
            if (value != requestLock)
            {
                requestLock = value;
            }
        }
    }

    /// <summary>
    ///  SDK底层锁定头部姿态
    /// </summary>
    public void RequestLock()
    {
        if (device != null)
        {
            device.NLockTracker();
        }
    }

    /// <summary>
    /// SDK底层解锁头部姿态
    /// </summary>
    public void RequestUnLock()
    {
        if (device != null)
        {
            device.NUnLockTracker();
        }
    }

    public bool DistortionEnabled
    {
        get
        {
            return NvrGlobal.distortionEnabled;
        }
        set
        {
            if (value != NvrGlobal.distortionEnabled)
            {
                NvrGlobal.distortionEnabled = value;
            } 
        }
    }

    private bool gazeShow = false;
    /// <summary>
    ///  GazeTag.Show， GazeTag.Hide 后面的param传 "" 即可
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="param"></param>
    public void GazeApi(GazeTag tag, String param)
    {
        if (device != null)
        { 
            bool rslt = device.GazeApi(tag, param);
            if(tag == GazeTag.Show)
            {
                bool useDFT = NvrViewer.USE_DTR && !NvrGlobal.supportDtr;
                gazeShow = useDFT ? true : rslt;
            }
            else if(tag == GazeTag.Hide)
            {
                gazeShow = false;
            }
        }
    }

    private bool forceUseReticle = false;

    public void ForceUseReticle(bool forced)
    {
        forceUseReticle = forced;
        if (forceUseReticle && GazeInputModule.gazePointer == null)
        {
            NvrReticle nvrReticle = Camera.main.GetComponentInChildren<NvrReticle>();
            GazeInputModule.gazePointer = nvrReticle;
        }else if (!forceUseReticle && GazeInputModule.gazePointer != null)
        {
            GazeInputModule.gazePointer = null;
        }
    }

    public bool ReticleShow()
    {
        if (forceUseReticle)
        {
            return true;
        }
        // dtr=false,dft=??
        return NvrGlobal.supportDtr ? false : gazeShow;
    }
    


#if UNITY_EDITOR
    /// Restores level head tilt in when playing in the Unity Editor after you
    /// release the Ctrl key.
    public bool autoUntiltHead = true;

  /// @cond
  /// Use unity remote as the input source.
  public bool UseUnityRemoteInput = false;
  /// @endcond

  /// The screen size to emulate when testing in the Unity Editor.
  public NvrProfile.ScreenSizes ScreenSize {
    get {
      return screenSize;
    }
    set {
      if (value != screenSize) {
        screenSize = value;
        if (device != null) {
          device.UpdateScreenData();
        }
      }
    }
  }
  [SerializeField]
  private NvrProfile.ScreenSizes screenSize = NvrProfile.ScreenSizes.Nexus5;

  /// The viewer type to emulate when testing in the Unity Editor.
  public NvrProfile.ViewerTypes ViewerType {
    get {
      return viewerType;
    }
    set {
      if (value != viewerType) {
        viewerType = value;
        if (device != null) {
          device.UpdateScreenData();
        }
      }
    }
  }
  [SerializeField]
  private NvrProfile.ViewerTypes viewerType = NvrProfile.ViewerTypes.CardboardMay2015;
#endif

  // The VR device that will be providing input data.
  private static BaseVRDevice device;


  /// Scales the resolution of the #StereoScreen.  Set to less than 1.0 to increase
  /// rendering speed while decreasing sharpness, or greater than 1.0 to do the
  /// opposite.
  public float StereoScreenScale {
    get {
      return stereoScreenScale;
    }
    set {
      value = Mathf.Clamp(value, 0.1f, 10.0f);  // Sanity.
      if (stereoScreenScale != value) {
        stereoScreenScale = value;
      }
    }
  }
  [SerializeField]
  private float stereoScreenScale = 1;

    /// The texture that Unity renders the scene to.  After the frame has been rendered,
    /// this texture is drawn to the screen with a lens distortion correction effect.
    /// The texture size is based on the size of the screen, the lens distortion
    /// parameters, and the #StereoScreenScale factor.
    public RenderTexture GetStereoScreen(int eye)
    {
        // Don't need it except for distortion correction.
        if (!vrModeEnabled)
        {
            return null;
        }
        if (eyeStereoScreens[0] == null)
        {
            // 初始化6个纹理
            InitEyeStereoScreens();
            // Create on demand.
            // device.CreateStereoScreen();  // Note: uses set{}
        }

        if (Application.isEditor ||  (NvrViewer.USE_DTR && !NvrGlobal.supportDtr)) {
            // DFT or Editor
            return eyeStereoScreens[0];
        }

        // 获取对应索引的纹理
        return eyeStereoScreens[eye+_current_texture_index];
    }

    // 初始创建6个纹理，左右各3个 【左右左右左右】
    public RenderTexture[] eyeStereoScreens = new RenderTexture[_texture_count];

    // 初始化
    private void InitEyeStereoScreens()
    {
        bool useDFT = NvrViewer.USE_DTR && !NvrGlobal.supportDtr;
        if (!USE_DTR || useDFT)
        {
            // 编辑器模式 or 不支持DTR的DFT模式 只生成1个纹理
            RenderTexture rendetTexture = device.CreateStereoScreen();
            rendetTexture.Create();
            int tid= (int)rendetTexture.GetNativeTexturePtr();
            for (int i = 0; i < _texture_count; i++)
            {
                eyeStereoScreens[i] = rendetTexture;
                _texture_ids[i] = tid;
            }
        }
        else
        {
            for (int i = 0; i < _texture_count; i++)
            {
                eyeStereoScreens[i] = device.CreateStereoScreen();
                eyeStereoScreens[i].Create();
                _texture_ids[i] = (int)eyeStereoScreens[i].GetNativeTexturePtr();
            }
        } 
    }

    // 释放所有纹理
    private void RealeaseEyeStereoScreens()
    {
        for (int i = 0; i < _texture_count; i++)
        {   
            if(eyeStereoScreens[i] != null)
            {
               eyeStereoScreens[i].Release();
               eyeStereoScreens[i] = null;
                _texture_ids[i] = 0;
            }
        }
    }  

  /// Describes the current device, including phone screen.
  public NvrProfile Profile {
    get {
      return device.Profile;
    }
  }

  /// Distinguish the stereo eyes.
  public enum Eye {
    Left,   /// The left eye
    Right,  /// The right eye
    Center  /// The "center" eye (unused)
  }

  /// When retrieving the #Projection and #Viewport properties, specifies
  /// whether you want the values as seen through the viewer's lenses (`Distorted`) or
  /// as if no lenses were present (`Undistorted`).
  public enum Distortion {
    Distorted,   /// Viewing through the lenses
    Undistorted  /// No lenses
  }

  /// The transformation of head from origin in the tracking system.
  public Pose3D HeadPose {
    get {
      return device.GetHeadPose();
    }
  }

  /// The transformation from head to eye.
  public Pose3D EyePose(Eye eye) {
    return device.GetEyePose(eye);
  }

  /// The projection matrix for a given eye.
  /// This matrix is an off-axis perspective projection with near and far
  /// clipping planes of 1m and 1000m, respectively.  The NvrEye script
  /// takes care of adjusting the matrix for its particular camera.
  public Matrix4x4 Projection(Eye eye, Distortion distortion = Distortion.Distorted) {
    return device.GetProjection(eye, distortion);
  }

  /// The screen space viewport that the camera for the specified eye should render into.
  /// In the _Distorted_ case, this will be either the left or right half of the `StereoScreen`
  /// render texture.  In the _Undistorted_ case, it refers to the actual rectangle on the
  /// screen that the eye can see.
  public Rect Viewport(Eye eye, Distortion distortion = Distortion.Distorted) {
    return device.GetViewport(eye, distortion);
  }

  /// The distance range from the viewer in user-space meters where objects may be viewed
  /// comfortably in stereo.  If the center of interest falls outside this range, the stereo
  /// eye separation should be adjusted to keep the onscreen disparity within the limits set
  /// by this range.  StereoController will handle this if the _checkStereoComfort_ is
  /// enabled.
  public Vector2 ComfortableViewingRange {
    get {
      return defaultComfortableViewingRange;
    }
  }
  private readonly Vector2 defaultComfortableViewingRange = new Vector2(0.4f, 100000.0f);

  /// @cond
  // Optional.  Set to a URI obtained from the Google Cardboard profile generator at
  //   https://www.google.com/get/cardboard/viewerprofilegenerator/
  // Example: Cardboard I/O 2015 viewer profile
  //public Uri DefaultDeviceProfile = new Uri("http://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0J-SA9JQHegj0qEAAAcEIAAHBCAABwQgAAcEJYADUpXA89OghX8as-YrENP1AAYAM");
  public Uri DefaultDeviceProfile = null;
  /// @endcond

  private void InitDevice() {
    if (device != null) {
      device.Destroy();
    }
    // 根据当前运行场景获取对应的设备对象
    device = BaseVRDevice.GetDevice();
    device.Init();

    device.SetVRModeEnabled(vrModeEnabled);
    // 更新界面数据
    device.UpdateScreenData();

    GazeApi(GazeTag.Show, "");
    GazeApi(GazeTag.Set_Size, ((int)GazeSize.Original).ToString());
   }

    /// @note Each scene load causes an OnDestroy of the current SDK, followed
    /// by and Awake of a new one.  That should not cause the underlying native
    /// code to hiccup.  Exception: developer may call Application.DontDestroyOnLoad
    /// on the SDK if they want it to survive across scene loads.
    void Awake()
    {
        if (instance == null)
        {
            instance = this; 
             
            // we sync in the TimeWarp, so we don't want unity syncing elsewhere
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = false;
            Input.gyro.enabled = false;
            Application.targetFrameRate = 60;
            // Disable screen dimming
            Screen.sleepTimeout = SleepTimeout.NeverSleep;  
        }
        if (instance != this)
        {
            Debug.LogError("There must be only one NvrViewer object in a scene.");
            UnityEngine.Object.DestroyImmediate(this);
            return;
        }

        InitDevice();
        AddPrePostRenderStages();

        device.AndroidLog("Welcome to use Unity NVR SDK , current SDK VERSION is " + NVR_SDK_VERSION + ", j " + NvrGlobal.jarVersion + ", s " + NvrGlobal.soVersion +", u " + Application.unityVersion);
    }

    void Start()
    {
        AddStereoControllerToCameras();

        if (eyeStereoScreens[0] == null)
        {
            // 初始化6个纹理
            InitEyeStereoScreens();
        }
    }

    public void AndroidLog(string msg)
    {
        if(device != null)
        {
            device.AndroidLog(msg);
        }else
        {
            Debug.Log(msg);
        }
    }

    public void UpdateDeviceState()
    {
        if(device != null)
        device.UpdateState();
    }

    public void UpdateEyeTexture()
    {
        // 更新左右眼目标纹理
        if (USE_DTR && NvrGlobal.supportDtr)
        {
            // 更换纹理索引
            SwapBuffers();

            NvrEye[] eyes = NvrViewer.Instance.eyes;
            for (int i = 0; i < 2; i++)
            {
                NvrEye eye = eyes[i];
                if (eye != null)
                {
                    eye.UpdateTargetTexture();
                }
            }

        }
    }

    void AddPrePostRenderStages() {
    var preRender = UnityEngine.Object.FindObjectOfType<NvrPreRender>();
    if (preRender == null) {
      var go = new GameObject("PreRender", typeof(NvrPreRender));
      go.SendMessage("Reset");
      go.transform.parent = transform;
    }
    var postRender = UnityEngine.Object.FindObjectOfType<NvrPostRender>();
    if (postRender == null) {
      var go = new GameObject("PostRender", typeof(NvrPostRender));
      go.SendMessage("Reset");
      go.transform.parent = transform;
    }
  }

  /// Whether the viewer's trigger was pulled. True for exactly one complete frame
  /// after each pull
  public bool Triggered { get; set; }

  /// Whether the viewer was tilted on its side. True for exactly one complete frame
  /// after each tilt.  Whether and how to respond to this event is up to the app.
  public bool Tilted { get; private set; }

  /// Whether the viewer profile has possibly changed.  This is meant to indicate
  /// that a new QR code has been scanned, although currently it is actually set any time the
  /// application is unpaused, whether it was due to a profile change or not.  True for one
  /// frame.
  public bool ProfileChanged { get; private set; }

  /// Whether the user has pressed the "VR Back Button", which on Android should be treated the
  /// same as the normal system Back Button, although you can respond to either however you want
  /// in your app.
  public bool BackButtonPressed { get; private set; }

  // Only call device.UpdateState() once per frame.
  private int updatedToFrame = 0;

  /// Reads the latest tracking data from the phone.  This must be
  /// called before accessing any of the poses and matrices above.
  ///
  /// Multiple invocations per frame are OK:  Subsequent calls merely yield the
  /// cached results of the first call.  To minimize latency, it should be first
  /// called later in the frame (for example, in `LateUpdate`) if possible.
  public void UpdateState() {
    if (updatedToFrame != Time.frameCount) {
      updatedToFrame = Time.frameCount;
      DispatchEvents();
      if (NvrViewer.Instance.NeedUpdateNearFar && device != null && device.nibiruVRServiceId != 0)
      {
         float far = NvrViewer.Instance.GetCameraFar();
         float mNear = 0.0305f;
         if (NvrGlobal.fovNear > -1) {
             mNear = NvrGlobal.fovNear;
         }
         device.SetCameraNearFar(mNear, far);
         NvrViewer.Instance.NeedUpdateNearFar = false;
       }

       }
    }

    List<INvrButtonListener> btnLiseners = null;
    List<INibiruJoystickListener> joystickListeners = null;
    private void DispatchEvents()
    {
        // Update flags first by copying from device and other inputs.
        if (device == null) return;
        Triggered = Input.GetMouseButtonDown(0);
        Tilted = device.tilted;
        ProfileChanged = device.profileChanged;
        BackButtonPressed = Input.GetKeyDown(KeyCode.Escape);
        // Reset device flags.
        device.tilted = false;
        device.profileChanged = false;
        device.backButtonPressed = false;

        if (Application.platform == RuntimePlatform.Android && Input.anyKeyDown)
        {
            bool triggerDirectKey = false;
            Event e = Event.current;
            if (e != null && e.isKey)
            {
                KeyCode currentKey = e.keyCode;
                Debug.Log("Current Key is : " + currentKey.ToString());
                if ((int)currentKey == 10 || currentKey == KeyCode.LeftArrow || currentKey == KeyCode.RightArrow || currentKey == KeyCode.UpArrow || currentKey == KeyCode.DownArrow || currentKey == KeyCode.Escape
                    || currentKey == KeyCode.JoystickButton0)
                {
                    if ((int)currentKey == 10 || currentKey == KeyCode.JoystickButton0)
                    {
                        // ok 键
                        Triggered = true;
                    }
                    triggerDirectKey = currentKey == KeyCode.LeftArrow || currentKey == KeyCode.RightArrow || currentKey == KeyCode.UpArrow || currentKey == KeyCode.DownArrow;
                    triggerKeyEvent(currentKey);
                }

                // 手柄  
                if (currentKey == KeyCode.LeftShift)
                {
                    triggerJoystickEvent(0, 0);
                }
                else if (currentKey == KeyCode.LeftAlt)
                {
                    triggerJoystickEvent(1, 0);
                }
                else if (currentKey == KeyCode.RightShift)
                {
                    triggerJoystickEvent(2, 0);
                }
                else if (currentKey == KeyCode.RightAlt)
                {
                    triggerJoystickEvent(3, 0);
                }
                else if (currentKey == KeyCode.Pause)
                {
                    triggerJoystickEvent(4, 0);
                }
                else if (currentKey == KeyCode.Return)
                {
                    triggerJoystickEvent(5, 0);
                }
                else if (currentKey == KeyCode.JoystickButton2)
                {
                    triggerJoystickEvent(6, 0);
                }
                else if (currentKey == KeyCode.JoystickButton3)
                {
                    triggerJoystickEvent(7, 0);
                }
                else if (currentKey == KeyCode.JoystickButton1)
                {
                    triggerJoystickEvent(8, 0);
                }
                else if (currentKey == KeyCode.JoystickButton0)
                {
                    triggerJoystickEvent(9, 0);
                }
            }
      
            //   
            // 手柄上下左右兼容处理
            float leftKeyHor = Input.GetAxis("5th axis");
            float leftKeyVer = Input.GetAxis("6th axis");
            // 左摇杆
            float leftStickHor = Input.GetAxis("joystick_Horizontal");
            float leftStickVer = Input.GetAxis("joystick_Vertical");
            // 右摇杆
            float rightStickHor = Input.GetAxis("3th axis");
            float rightStickVer = Input.GetAxis("4th axis");
            if (leftStickHor != 0)
            {
                triggerJoystickEvent(10, leftStickHor);
            }
            if (leftStickVer != 0)
            {
                triggerJoystickEvent(11, leftStickVer);
            }
            if (rightStickHor != 0)
            {
                triggerJoystickEvent(12, rightStickHor);
            }
            if (rightStickVer != 0)
            {
                triggerJoystickEvent(13, rightStickVer);
            }

            if (leftKeyHor == 1 && !triggerDirectKey)
            {
                // 左
                triggerKeyEvent(KeyCode.LeftArrow);
                triggerJoystickEvent(16, 0);
            }
            else if (leftKeyHor == -1 && !triggerDirectKey)
            {
                // 右
                triggerKeyEvent(KeyCode.RightArrow);
                triggerJoystickEvent(17, 0);
            }
            if (leftKeyVer == 1 && !triggerDirectKey)
            {
                // 上
                triggerKeyEvent(KeyCode.UpArrow);
                triggerJoystickEvent(14, 0);
            }
            else if (leftKeyVer == -1 && !triggerDirectKey)
            {
                // 下
                triggerKeyEvent(KeyCode.DownArrow);
                triggerJoystickEvent(15, 0);
            }
        }

        // up 事件
        if (Application.platform == RuntimePlatform.Android &&
            (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetKeyUp((KeyCode)10)))
        {
            triggerKeyEvent(KeyCode.JoystickButton0, true);
        }


    }

    private void triggerKeyEvent(KeyCode currentKey)
    {
        triggerKeyEvent(currentKey, false);
    }

    private void triggerKeyEvent(KeyCode currentKey, bool isKeyUp)
    {
        if (btnLiseners == null || btnLiseners.Count == 0)
        {
            List<GameObject> allObject = GetAllObjectsInScene();
            btnLiseners = new List<INvrButtonListener>();
            foreach (GameObject obj in allObject)
            {
                Component[] comps = obj.GetComponents(typeof(INvrButtonListener));
                if (comps != null)
                {
                    INvrButtonListener[] listeners = new INvrButtonListener[comps.Length];

                    for (int p = 0; p < comps.Length; p++)
                    {
                        listeners[p] = (INvrButtonListener)comps[p];
                    }
                    // 获取所有挂载了INvrButtonListener的物体
                    notifyBtnPressed(listeners, currentKey, isKeyUp);
                    foreach (Component cp in comps)
                    {
                        btnLiseners.Add((INvrButtonListener)cp);
                    }
                }
            }
        }
        else
        {
            notifyBtnPressed(btnLiseners.ToArray(), currentKey, isKeyUp);
        }
    }

    private void notifyBtnPressed(INvrButtonListener[] comps, KeyCode currentKey, bool isKeyUp)
    {
        if (comps == null) return;
        for (int i = 0; i < comps.Length; i++)
        {
            INvrButtonListener btnListener = (INvrButtonListener) comps[i];
            if (btnListener == null) continue;
            if (currentKey == KeyCode.LeftArrow)
            {
                btnListener.OnPressLeft();
            }
            else if (currentKey == KeyCode.RightArrow)
            {
                btnListener.OnPressRight();
            }
            else if (currentKey == KeyCode.UpArrow)
            {
                btnListener.OnPressUp();
            }
            else if (currentKey == KeyCode.DownArrow)
            {
                btnListener.OnPressDown();
            }
            else if (currentKey == KeyCode.Escape)
            {
                btnListener.OnPressBack();
            }
            else if (currentKey == KeyCode.JoystickButton0 || (int)currentKey == 10)
            {
                btnListener.OnPressEnter(isKeyUp);
            }
            else if (currentKey == KeyCode.Joystick5Button18)
            {
                // 音量加
                btnListener.OnPressVolumnUp();
            }
            else if (currentKey == KeyCode.Joystick5Button19)
            {
                // 音量减
                btnListener.OnPressVolumnDown();
            }
        }

    }

    /// Presents the #StereoScreen to the device for distortion correction and display.
    /// @note This function is only used if #DistortionCorrection is set to _Native_,
    /// and it only has an effect if the device supports it.
    public void PostRender(RenderTexture stereoScreen) {
    if (stereoScreen != null && stereoScreen.IsCreated()) {
      device.PostRender(stereoScreen);
    }
  }

  /// Resets the tracker so that the user's current direction becomes forward.
  public void Recenter() {
    device.Recenter();
  }

    /// Add a StereoController to any camera that does not have a Render Texture (meaning it is
    /// rendering to the screen).
    public static void AddStereoControllerToCameras() {
    for (int i = 0; i < Camera.allCameras.Length; i++) {
      Camera camera = Camera.allCameras[i];
      if (camera.targetTexture == null &&
          camera.GetComponent<NvrStereoController>() == null &&
          camera.GetComponent<NvrEye>() == null &&
          camera.GetComponent<NvrPreRender>() == null &&
          camera.GetComponent<NvrPostRender>() == null) {
        camera.gameObject.AddComponent<NvrStereoController>();
      }
    }
  }

  void OnEnable() {
#if UNITY_EDITOR
    // This can happen if you edit code while the editor is in Play mode.
    if (device == null) {
      InitDevice();
    }
#endif
    device.OnPause(false);

    StartCoroutine("EndOfFrame");
    }

  void OnDisable() {
    device.OnPause(true);
    StopCoroutine("EndOfFrame");
  }

    void OnApplicationPause(bool pause)
    {
        Debug.Log("NvrViewer->OnApplicationPause," + pause + ", hasEnterVRMode="+ hasEnterVRMode);
        // 首次不执行
        if (hasEnterVRMode)
        {
            device.OnApplicationPause(pause);
        }
    }

    void OnApplicationFocus(bool focus) {
    Debug.Log("NvrViewer->OnApplicationFocus," + focus);
    device.OnFocus(focus);
  }

  void OnApplicationQuit() {
    device.OnApplicationQuit();
    StopAllCoroutines();
    Debug.Log("NvrViewer->OnApplicationQuit");
  }

  void OnDestroy() {
    if (btnLiseners != null)
    {
        btnLiseners.Clear();
    }
    btnLiseners = null;
    RealeaseEyeStereoScreens();
    VRModeEnabled = false;
    if (device != null) {
      device.Destroy();
    }
    if (instance == this) {
      instance = null;
    }
    Debug.Log("NvrViewer->OnDestroy");
  }

    private bool hasEnterVRMode = false;
    public void EnterVRMode()
    {
        if(device != null)
        {
            if (!hasEnterVRMode) {
                hasEnterVRMode = true;
                device.EnterVRMode();
                if (NvrGlobal.supportDtr && Camera.main != null)
                {
                    NvrReticle nvrReticle = Camera.main.GetComponentInChildren<NvrReticle>();
                    if(nvrReticle != null && GazeInputModule.gazePointer == (INvrGazePointer) nvrReticle) {
                        if(!forceUseReticle) { 
                            GazeInputModule.gazePointer = null;
                        }
                    }
                }
            }
        }
    }

    // 处理来自Android的调用 
    public void ResetHeadTrackerFromAndroid()
    {
        if (instance !=null && device != null)
        {
            device.Recenter();
        }
    }

    void OnVolumnUp()
    {
        triggerKeyEvent(KeyCode.Joystick5Button18);
    }

    void OnVolumnDown()
    {
        triggerKeyEvent(KeyCode.Joystick5Button19);
    }

    void OnActivityPause()
    {
        Debug.Log("OnActivityPause");
    }

    void OnActivityResume()
    {
        Debug.Log("OnActivityResume");
    }

    /// <summary>
    ///  系统分屏接口 1=系统分屏，0=应用分屏
    /// </summary>
    public void SetSystemVRMode(int flag) {
        device.NSetSystemVRMode(flag);
    }

    private int[] _texture_ids = new int[_texture_count];
    private int _current_texture_index, _next_texture_index;
    public bool SwapBuffers()
    {
        bool ret = true;
        for (int i = 0; i < _texture_count; i++)
        {
            if (!eyeStereoScreens[i].IsCreated())
            {
                eyeStereoScreens[i].Create(); 
				_texture_ids[i] = (int) eyeStereoScreens[i].GetNativeTexturePtr(); 
                ret = false;
            }
        }

        _current_texture_index = _next_texture_index;
        _next_texture_index = (_next_texture_index + 2) % _texture_count;
        return ret;
    }

    public int GetEyeTextureId(int eye) {
        return _texture_ids[_current_texture_index + (int)eye];
    }

    public int GetTimeWarpViewNum ()
    {
        return device.GetTimewarpViewNumber();
    }

    public List<GameObject> GetAllObjectsInScene()
    {
        GameObject[] pAllObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
        List<GameObject> pReturn = new List<GameObject>();
        foreach (GameObject pObject in pAllObjects)
        {
            if (pObject == null || !pObject.activeInHierarchy || pObject.hideFlags == HideFlags.NotEditable || pObject.hideFlags == HideFlags.HideAndDontSave)
            {
                continue;
            } 
            pReturn.Add(pObject);
        }
        return pReturn;
    }

    public Texture2D createTexture2D(RenderTexture renderTexture)
    {
        int width = renderTexture.width;
        int height = renderTexture.height;
        Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture2D.Apply();
        return texture2D;
    }

    //string mFileName = Application.persistentDataPath + "/d" + i + "-ScreenShot.png";
    //Debug.Log(mFileName);
    //!System.IO.File.Exists(mFileName)

    //Texture2D mTexture = NvrViewer.Instance.createTexture2D(re);
    //Debug.Log(mFileName);
    ////将图片信息编码为字节信息
    //byte[] bytes = mTexture.EncodeToPNG();
    //// 保存
    //System.IO.File.WriteAllBytes(mFileName, bytes);


    IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (NvrViewer.USE_DTR && NvrGlobal.supportDtr)
            {
                NvrPluginEvent.IssueWithData(RenderEventType.TimeWarp, NvrViewer.Instance.GetTimeWarpViewNum());
            }
        }
    }


    private void triggerJoystickEvent(int index, float axisValue)
    {
        if (joystickListeners == null)
        {
            List<GameObject> allObject = GetAllObjectsInScene();
            joystickListeners = new List<INibiruJoystickListener>();
            foreach (GameObject obj in allObject)
            {
                Component[] joystickcomps = obj.GetComponents(typeof(INibiruJoystickListener));

                if (joystickcomps != null)
                {
                    INibiruJoystickListener[] listeners = new INibiruJoystickListener[joystickcomps.Length];

                    for (int p = 0; p < joystickcomps.Length; p++)
                    {
                        listeners[p] = (INibiruJoystickListener)joystickcomps[p];
                    }
                    // INibiruJoystickListener
                    notifyJoystickPressed(listeners, index, axisValue);
                    foreach (Component cp in joystickcomps)
                    {
                        joystickListeners.Add((INibiruJoystickListener)cp);
                    }
                }
            }
        }
        else
        {
            notifyJoystickPressed(joystickListeners.ToArray(), index, axisValue);
        }
    }

    private void notifyJoystickPressed(INibiruJoystickListener[] comps, int index, float axisValue)
    {
        if (comps == null) return;
        for (int i = 0; i < comps.Length; i++)
        {
            INibiruJoystickListener joystickListener = (INibiruJoystickListener)comps[i];
            if (joystickListener == null) continue;
            switch (index)
            {
                case 0:
                    // l1
                    joystickListener.OnPressL1();
                    break;
                case 1:
                    // l2
                    joystickListener.OnPressL2();
                    break;
                case 2:
                    // r1
                    joystickListener.OnPressR1();
                    break;
                case 3:
                    // r2
                    joystickListener.OnPressR2();
                    break;
                case 4:
                    // select
                    joystickListener.OnPressSelect();
                    break;
                case 5:
                    // start
                    joystickListener.OnPressStart();
                    break;
                case 6:
                    // x
                    joystickListener.OnPressX();
                    break;
                case 7:
                    // y
                    joystickListener.OnPressY();
                    break;
                case 8:
                    // a
                    joystickListener.OnPressA();
                    break;
                case 9:
                    // b
                    joystickListener.OnPressB();
                    break;
                case 10:
                    // leftstickx
                    joystickListener.OnLeftStickX(axisValue);
                    break;
                case 11:
                    // leftsticky
                    joystickListener.OnLeftStickY(axisValue);
                    break;
                case 12:
                    // rightstickx
                    joystickListener.OnRightStickX(axisValue);
                    break;
                case 13:
                    // rightsticky
                    joystickListener.OnRightStickY(axisValue);
                    break;
                case 14:
                    // dpad-up
                    joystickListener.OnPressDpadUp();
                    break;
                case 15:
                    // dpad-down
                    joystickListener.OnPressDpadDown();
                    break;
                case 16:
                    // dpad-left
                    joystickListener.OnPressDpadLeft();
                    break;
                case 17:
                    // dpad-right
                    joystickListener.OnPressDpadRight();
                    break;
            }

        }
    }


    private float mFar = -1;
    private bool needUpdateNearFar = false;
    public void UpateCameraFar(float far)
    {
        mFar = far;
        needUpdateNearFar = true;
        NvrGlobal.fovFar = far;
        if (Application.isEditor)
        {
            // 编辑器及时生效
            Camera.main.farClipPlane = far;
        }  
    }

    public float GetCameraFar() {
        return mFar;
    }

    public bool NeedUpdateNearFar {
        get
        {
            return needUpdateNearFar;
        }
        set
        {
            if (value != needUpdateNearFar)
            {
                needUpdateNearFar = value;
            }
        }
    }


    private float oldFov = -1;

    private Matrix4x4[] eyeOriginalProjection;

    /// <summary>
    ///  检查初始是否需要更新相机投影矩阵
    /// </summary>
    /// <param name="eye"></param>
    public void UpdateEyeCameraProjection(Eye eye)
    {
        if (oldFov != -1 && eye == Eye.Right)
        {
            UpdateCameraFov(oldFov);
        }

        if(!Application.isEditor && device != null && eye == Eye.Right)
        {

            if (mFar >0 )
            {
                float mNear = 0.0305f;
                if (NvrGlobal.fovNear > -1) {
                    mNear = NvrGlobal.fovNear;
                }
                // Debug.Log("new near : " + mNear + "," + NvrGlobal.fovNear+ ",new far : " + mFar + "," + NvrGlobal.fovFar);

                // 更新camera  near far
                float fovLeft = mNear * Mathf.Tan(-Profile.viewer.maxFOV.outer * Mathf.Deg2Rad);
                float fovTop = mNear * Mathf.Tan(Profile.viewer.maxFOV.upper * Mathf.Deg2Rad);
                float fovRight = mNear * Mathf.Tan(Profile.viewer.maxFOV.inner * Mathf.Deg2Rad);
                float fovBottom = mNear * Mathf.Tan(-Profile.viewer.maxFOV.lower * Mathf.Deg2Rad);

                //Debug.Log("fov : " +fovLeft+","+fovRight+","+fovTop+","+fovBottom);

                Matrix4x4 eyeProjection = BaseVRDevice.MakeProjection(fovLeft, fovTop, fovRight, fovBottom, mNear, mFar);
                for (int i = 0; i < 2; i++)
                {
                    NvrEye mEye = eyes[i];
                    if (mEye != null)
                    {
                        mEye.cam.projectionMatrix = eyeProjection;
                    }
                }

            } 
        }
    }

    public void ResetCameraFov() {
        for (int i = 0; i < 2; i++)
        {
            if (eyeOriginalProjection[i] == null) return;
            NvrEye eye = eyes[i];
            if (eye != null)
            {
                eye.cam.projectionMatrix = eyeOriginalProjection[i];
            }
        }
        oldFov = -1;
    }

    /// <summary>
    /// 
    ///  fov范围 [40~90]
    /// </summary>
    /// <param name="fov"></param>
    public void UpdateCameraFov(float fov)
    {
        if (fov > 90) fov = 90;
        if (fov < 5) fov = 5;
        // cache左右眼透视矩阵
        if (eyeOriginalProjection == null && eyes[0] != null && eyes[1] != null)
        {
            eyeOriginalProjection = new Matrix4x4[2];
            eyeOriginalProjection[0] = eyes[0].cam.projectionMatrix;
            eyeOriginalProjection[1] = eyes[1].cam.projectionMatrix;
        }
        oldFov = fov;
        float near = NvrGlobal.fovNear > 0 ? NvrGlobal.fovNear : 0.0305f;
        float far = NvrGlobal.fovFar > 0 ? NvrGlobal.fovFar : 2000;
        far = far > 100 ? far : 2000;
        float fovLeft = near * Mathf.Tan(-fov * Mathf.Deg2Rad);
        float fovTop = near * Mathf.Tan(fov * Mathf.Deg2Rad);
        float fovRight = near * Mathf.Tan(fov * Mathf.Deg2Rad);
        float fovBottom = near * Mathf.Tan(-fov * Mathf.Deg2Rad);
        Matrix4x4 eyeProjection = BaseVRDevice.MakeProjection(fovLeft, fovTop, fovRight, fovBottom, near, far);
        if (device != null)
        {
            for (int i = 0; i < 2; i++)
            {
                NvrEye eye = eyes[i];
                if (eye != null)
                {
                    eye.cam.projectionMatrix = eyeProjection;
                }
            }
        }
    }

    /// <summary>
    ///  水平方向头部转动限制在固定范围 【 中间角度为0，左侧为负值，右侧为正值 】
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    public void SetHorizontalAngleRange(float minRange,float maxRange)
    {
        if (headEulerAnglesRange == null) {
            headEulerAnglesRange = new float[] { 0,360,0,360};
        }
        headEulerAnglesRange[0] = minRange + 360;
        headEulerAnglesRange[1] = maxRange;
    }

    /// <summary>
    ///  垂直方向头部转动限制在固定范围 【 中间角度为0，上面为负值，下面为正值 】
    /// </summary>
    /// <param name="minRange"></param>
    /// <param name="maxRange"></param>
    public void SetVerticalAngleRange(float minRange, float maxRange)
    {
        if (headEulerAnglesRange == null)
        {
            headEulerAnglesRange = new float[] { 0, 360, 0, 360 };
        }
        headEulerAnglesRange[2] = minRange + 360;
        headEulerAnglesRange[3] = maxRange;
    }

    /// <summary>
    ///  移除头部转动的限制
    /// </summary>
    public void RemoveAngleLimit()
    {
        headEulerAnglesRange = null;
    }

    public float[] GetHeadEulerAnglesRange()
    {
        return headEulerAnglesRange;
    }

    /// <summary>
    /// 
    ///    打开视频播放器
    ///    
    /// </summary>
    /// <param name="path">视频源地址</param>
    /// <param name="type2D3D">0=2d,1=3d 单双屏</param>
    /// <param name="mode">0=normal,1=360,2=180,3=fullmode 视频类型</param>
    /// <param name="decode">0=hardware,1=software 解码方式</param>
    public void OpenVideoPlayer(string path, int type2D3D, int mode, int decode)
    {
        device.ShowVideoPlayer(path, type2D3D, mode, decode);
    }

    public void CloseVideoPlayer()
    {
        device.DismissVideoPlayer();
    } 

    /// <summary>
    ///  获取androidSD卡路径
    /// </summary>
    /// <returns>exp: /storage/emulated/0</returns>
    public string GetStoragePath()
    {
        return device.GetStoragePath();
    }

}
