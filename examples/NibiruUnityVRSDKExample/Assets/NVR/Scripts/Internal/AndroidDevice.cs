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
#if UNITY_ANDROID

using UnityEngine;

/// @cond
namespace Nvr.Internal
{
    public class AndroidDevice : NvrDevice
    {
        // 
        private const string ActivityListenerClass =
            "com.nibiru.lib.vr.unity.NibiruVRUnityService";

        // sdk-class
        private const string NibiruVRClass = "com.nibiru.lib.vr.NibiruVR";

        private static AndroidJavaObject activityListener, nibiruVR;

        public override void Init()
        {
            SetApplicationState();

            ConnectToActivity();
            base.Init();
        }

        protected override void ConnectToActivity()
        {
            base.ConnectToActivity();
            if (androidActivity != null && activityListener == null)
            {
                activityListener = Create(ActivityListenerClass);
            }
            if (androidActivity != null && nibiruVR == null)
            {
                nibiruVR = Create(NibiruVRClass);
            }
        }

        public override long CreateNibiruVRService()
        {
            string initParams = "";
            long pointer = 0;
            CallStaticMethod<string>(ref initParams, nibiruVR, "initNibiruVRServiceForUnity", androidActivity);

            string[] data = initParams.Split('_');
            pointer = long.Parse(data[0]);
            NvrGlobal.supportDtr = (int.Parse(data[1]) == 1 ? true : false);
            NvrGlobal.distortionEnabled = (int.Parse(data[2]) == 1 ? true : false);
            NvrGlobal.useNvrSo = (int.Parse(data[3]) == 1 ? true : false);

            string channelCode = "";
            CallStaticMethod<string>(ref channelCode, nibiruVR, "getChannelCode");
            NvrGlobal.channelCode = channelCode;

            // 系统支持
            int[] allVersion = new int[] { -1,-1};
            CallStaticMethod<int[]>(ref allVersion, nibiruVR, "getVersionForUnity");
            NvrGlobal.soVersion = allVersion[0];
            NvrGlobal.jarVersion = allVersion[1];
            NvrGlobal.platPerformanceLevel = allVersion[2];
            NvrGlobal.platformID = allVersion[3];

            Debug.Log("AndDev->Service : [pointer]=" + pointer + ", [dtrSpt] =" + NvrGlobal.supportDtr + ", [DistEnabled]=" +
            NvrGlobal.distortionEnabled+", [useNvrSo]="+ NvrGlobal.useNvrSo + ", [code]="+ channelCode + ", [jar]="+ NvrGlobal.jarVersion + ", [so]="+ NvrGlobal.soVersion
            +", [pid]="+NvrGlobal.platformID + ", [pl]=" + NvrGlobal.platPerformanceLevel);

            // 读取cardboard参数
            string cardboardParams = "";
            CallStaticMethod<string>(ref cardboardParams, nibiruVR, "getNibiruVRConfigFull");
            if (cardboardParams.Length > 0)
            {
                Debug.Log("cardboardParams is " + cardboardParams);
                string[] profileData = cardboardParams.Split('_');
                for (int i = 0; i < NvrGlobal.dftProfileParams.Length; i++)
                {
					if(i >= profileData.Length) break;

                    if (profileData[i] == null || profileData[i].Length == 0) continue;
              
                    NvrGlobal.dftProfileParams[i] = float.Parse(profileData[i]);
                }
            }
            else
            {
                Debug.Log("Nvr->AndroidDevice->getNibiruVRConfigFull Failed ! ");
            }

            return pointer;
        }

        public override void SetDisplayQuality(int level)
        {
            CallStaticMethod(nibiruVR, "setDisplayQualityForUnity", level);
        }

        public override bool GazeApi(GazeTag tag, string param)
        {
            bool show = false;
            CallStaticMethod<bool>(ref show, nibiruVR, "gazeApiForUnity", (int)tag, param);
            return show;
        }

        public override void SetVRModeEnabled(bool enabled)
        {
            // CallObjectMethod(activityListener, "setVRModeEnabled", enabled);
        }
        public override void AndroidLog(string msg)
        {
            CallStaticMethod(activityListener, "log", msg);
        }

        public override void OnApplicationPause(bool pause)
        {
            base.OnApplicationPause(pause);
            //CallObjectMethod(activityListener, "onPause", pause);

            if(!pause && androidActivity != null) { 
               RunOnUIThread(androidActivity,new AndroidJavaRunnable(runOnUiThread)); 
            }

        }

        void runOnUiThread()
        {
            //mActivity.getWindow().addFlags(128);
            //mActivity.getWindow().getDecorView().setSystemUiVisibility(5894);
            AndroidJavaObject androidWindow = androidActivity.Call<AndroidJavaObject>("getWindow");
            androidWindow.Call("addFlags", 128);
            AndroidJavaObject androidDecorView = androidWindow.Call<AndroidJavaObject>("getDecorView");
            androidDecorView.Call("setSystemUiVisibility", 5894);
        }

        private void SetApplicationState()
        {
            //if (activityListener == null) {
            //  using (var listenerClass = GetClass(ActivityListenerClass)) {
            //    CallStaticMethod(listenerClass, "setUnityApplicationState");
            //  }
            //}
        }

        /// <summary>
        /// 	 * @param path
        ///    * @param type23d  0=2d,1=3d
        ///    * @param mode  0=normal,1=360,2=180,3=fullmode
        ///    * @param decode 0=hardware,1=software
        /// </summary>
        public override void ShowVideoPlayer(string path,int type2D3D,int mode,int decode) {
            CallStaticMethod(nibiruVR, "showVideoPlayer", path,type2D3D,mode,decode);
        }

        public override void DismissVideoPlayer() {
            CallStaticMethod(nibiruVR, "dismissVideoPlayer");
        }

        public override string GetStoragePath() { return GetAndroidStoragePath(); }

        public override void SetCameraNearFar(float near, float far)
        {
            CallStaticMethod(nibiruVR, "setProjectionNearFarForUnity", near, far);
        }

    }
}
/// @endcond

#endif
