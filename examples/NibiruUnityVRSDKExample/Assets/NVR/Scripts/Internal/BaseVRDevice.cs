// Copyright 2016 Nibiru. All rights reserved.
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

#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#endif
#endif

using UnityEngine;
using System.Collections.Generic;
using System;

/// @cond
namespace Nvr.Internal {
  // Represents a vr device that this plugin interacts with.
  public abstract class BaseVRDevice {
    private static BaseVRDevice device = null;
         

    protected BaseVRDevice() {
      Profile = NvrProfile.Default.Clone();
    }

    public NvrProfile Profile { get; protected set; }

    public abstract void Init();

    public abstract void SetVRModeEnabled(bool enabled); 
         
    public virtual void AndroidLog(string msg) { }

    public virtual bool SupportsNativeDistortionCorrection(List<string> diagnostics) {
      return true;
    }
     
    public long nibiruVRServiceId;

    public virtual RenderTexture CreateStereoScreen() {
      float scale = NvrViewer.Instance.StereoScreenScale;

      int width = (int)recommendedTextureSize[0];
      int height = (int)recommendedTextureSize[1];
      width = width == 0 ? Screen.width : width;
      height = height == 0 ? Screen.height : height;

     bool useDFT = NvrViewer.USE_DTR && !NvrGlobal.supportDtr;
     float DFT_TextureScale = 0.8f;
     if (useDFT)
     {
            TextureQuality textureQuality = NvrViewer.Instance.TextureQuality;
            if (textureQuality == TextureQuality.Best)
            {
                DFT_TextureScale = 1f;
            }
            else if (textureQuality == TextureQuality.Good)
            {
                DFT_TextureScale = 0.8f;
            }
            else if (textureQuality == TextureQuality.Simple)
            {
                DFT_TextureScale = 0.6666666666666666f;
            }
            width = (int)(width * DFT_TextureScale);
            height = (int)(height * DFT_TextureScale);
      }

      NvrViewer.Instance.AndroidLog("Creating ss tex "
        + width + " x " + height + "." +"sInfo : ["+Screen.width+","+Screen.height+ "].DFT_TexScal="+ DFT_TextureScale 
        + ",TexQuality=" + NvrViewer.Instance.TextureQuality.ToString());

      var rt = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
      rt.anisoLevel = 0;
      rt.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, 1);
      rt.Create();
      return rt;
    }
         
   public virtual long CreateNibiruVRService()
    {
            return 0;
    }
   
    public virtual void SetCameraNearFar(float near ,float far)
   {
        
    }

    public virtual void SetDisplayQuality(int level)
    {
    }
          
    public virtual  IntPtr NGetRenderEventFunc() { return IntPtr.Zero; }

    public virtual int GetTimewarpViewNumber()
    {
        return 0;
    }


    public Pose3D GetHeadPose() {
      return this.headPose;
    }
    protected MutablePose3D headPose = new MutablePose3D();

    public Pose3D GetEyePose(NvrViewer.Eye eye) {
      switch(eye) {
        case NvrViewer.Eye.Left:
          return leftEyePose;
        case NvrViewer.Eye.Right:
          return rightEyePose;
        default:
          return null;
      }
    }
    protected MutablePose3D leftEyePose = new MutablePose3D();
    protected MutablePose3D rightEyePose = new MutablePose3D();

    public Matrix4x4 GetProjection(NvrViewer.Eye eye,
                                   NvrViewer.Distortion distortion = NvrViewer.Distortion.Distorted) {
      switch(eye) {
        case NvrViewer.Eye.Left:
          return distortion == NvrViewer.Distortion.Distorted ?
              leftEyeDistortedProjection : leftEyeUndistortedProjection;
        case NvrViewer.Eye.Right:
          return distortion == NvrViewer.Distortion.Distorted ?
              rightEyeDistortedProjection : rightEyeUndistortedProjection;
        default:
          return Matrix4x4.identity;
      }
    }
    protected Matrix4x4 leftEyeDistortedProjection;
    protected Matrix4x4 rightEyeDistortedProjection;
    protected Matrix4x4 leftEyeUndistortedProjection;
    protected Matrix4x4 rightEyeUndistortedProjection;

    public Rect GetViewport(NvrViewer.Eye eye,
                            NvrViewer.Distortion distortion = NvrViewer.Distortion.Distorted) {
      switch(eye) {
        case NvrViewer.Eye.Left:
          return distortion == NvrViewer.Distortion.Distorted ?
              leftEyeDistortedViewport : leftEyeUndistortedViewport;
        case NvrViewer.Eye.Right:
          return distortion == NvrViewer.Distortion.Distorted ?
              rightEyeDistortedViewport : rightEyeUndistortedViewport;
        default:
          return new Rect();
      }
    }
    protected Rect leftEyeDistortedViewport;
    protected Rect rightEyeDistortedViewport;
    protected Rect leftEyeUndistortedViewport;
    protected Rect rightEyeUndistortedViewport;

    protected Vector2 recommendedTextureSize;
    protected int leftEyeOrientation;
    protected int rightEyeOrientation;

    public bool tilted;
    public bool profileChanged;
    public bool backButtonPressed;

    public abstract void UpdateState();

    public abstract void UpdateScreenData();

    public abstract void Recenter();

    public abstract void PostRender(RenderTexture stereoScreen);

    public virtual void OnPause(bool pause) {
      if (!pause) {
        UpdateScreenData();
      }
    }

    public virtual void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            UpdateScreenData();
        }
    }
    public virtual void EnterVRMode() { }

    public virtual void OnFocus(bool focus) {
      // Do nothing.
    }

    public virtual void OnApplicationQuit() {
      // Do nothing.
    }
    
    public virtual string GetStoragePath() { return null; }

    public virtual void ShowVideoPlayer(string path, int type2D3D, int mode, int decode) { }
    
    public virtual void DismissVideoPlayer() { }

    /// <summary>
    ///   1=系统分屏，0=应用分屏
    /// </summary>
    /// <param name="flag"></param>
    public virtual void NSetSystemVRMode(int flag) { }

    /// <summary>
    ///  锁定当前画面
    /// </summary>
    public virtual void NLockTracker() { }

    /// <summary>
    ///  解除锁定
    /// </summary>
    public virtual void NUnLockTracker() { }

    /// <summary> DTR 
    ///  (0=显示点，1=隐藏点，2=设置点距离，3=设置点大小，4=设置点颜色）
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="param"></param>
    public virtual bool GazeApi(GazeTag tag, String param) { return false; }


    public virtual void Destroy() {
      if (device == this) {
        device = null;
      }
    }

    // Helper functions. near=1,far=1000
    protected void ComputeEyesFromProfile(float near, float far) {
      // Compute left eye matrices from screen and device params
      Matrix4x4 leftEyeView = Matrix4x4.identity;
      leftEyeView[0, 3] = -Profile.viewer.lenses.separation / 2;
      leftEyePose.Set(leftEyeView);

      float[] rect = new float[4];
      Profile.GetLeftEyeVisibleTanAngles(rect);
      leftEyeDistortedProjection = MakeProjection(rect[0], rect[1], rect[2], rect[3], near, far);
      Profile.GetLeftEyeNoLensTanAngles(rect);
      leftEyeUndistortedProjection = MakeProjection(rect[0], rect[1], rect[2], rect[3], near, far);

      leftEyeUndistortedViewport = Profile.GetLeftEyeVisibleScreenRect(rect);
      leftEyeDistortedViewport = leftEyeUndistortedViewport;

      // Right eye matrices same as left ones but for some sign flippage.
      Matrix4x4 rightEyeView = leftEyeView;
      rightEyeView[0, 3] *= -1;
      rightEyePose.Set(rightEyeView);

      rightEyeDistortedProjection = leftEyeDistortedProjection;
      rightEyeDistortedProjection[0, 2] *= -1;
      rightEyeUndistortedProjection = leftEyeUndistortedProjection;
      rightEyeUndistortedProjection[0, 2] *= -1;

      rightEyeUndistortedViewport = leftEyeUndistortedViewport;
      rightEyeUndistortedViewport.x = 1 - rightEyeUndistortedViewport.xMax;
      rightEyeDistortedViewport = rightEyeUndistortedViewport;

      if(NvrViewer.USE_DTR)  return;

      float width = Screen.width * (leftEyeUndistortedViewport.width+rightEyeDistortedViewport.width);
      float height = Screen.height * Mathf.Max(leftEyeUndistortedViewport.height,
                                               rightEyeUndistortedViewport.height);
      recommendedTextureSize = new Vector2(width, height);
    }

    public static Matrix4x4 MakeProjection(float l, float t, float r, float b, float n, float f) {
      Matrix4x4 m = Matrix4x4.zero;
      m[0, 0] = 2 * n / (r - l);
      m[1, 1] = 2 * n / (t - b);
      m[0, 2] = (r + l) / (r - l);
      m[1, 2] = (t + b) / (t - b);
      m[2, 2] = (n + f) / (n - f);
      m[2, 3] = 2 * n * f / (n - f);
      m[3, 2] = -1;
      return m;
    }

    public static BaseVRDevice GetDevice() {
      if (device == null) {
#if UNITY_EDITOR
        device = new EditorDevice();
#elif ANDROID_DEVICE
        device = new AndroidDevice();
#elif IPHONE_DEVICE
        device = new iOSDevice();
#else
        throw new InvalidOperationException("Unsupported device.");
#endif
      }
      return device;
    }
  }
}
/// @endcond

