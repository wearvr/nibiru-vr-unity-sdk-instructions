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
using System;
using System.Collections;
using UnityEngine;

/// <summary>
///  截图
/// </summary>
class NvrScreenCapture : MonoBehaviour
{

    void Start()
    {
#if UNITY_EDITOR
        string mPath = Application.dataPath + "/ScreenShot.png";
#elif UNITY_ANDROID
        // Android/data/包名/files
 string mPath = Application.persistentDataPath+ "/ScreenShot.png";
#endif
         

        // CaptureByUnity(mPath);
        // StartCoroutine(CaptureByCamera(GetComponent<Camera>(), new Rect(0, 0, 1361, 339), mPath));
    }


    private void CaptureByUnity(string mFileName)
    {
        Application.CaptureScreenshot(mFileName, 0);
    }

    /// <summary>
    /// 根据一个Rect类型来截取指定范围的屏幕
    /// 左下角为(0,0)
    /// </summary>
    /// <param name="mRect">M rect.</param>
    /// <param name="mFileName">M file name.</param>
    private IEnumerator CaptureByRect(Rect mRect, string mFileName)
    {
        //等待渲染线程结束
        yield return new WaitForEndOfFrame();
        //初始化Texture2D
        Texture2D mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGB24, false);
        //读取屏幕像素信息并存储为纹理数据
        mTexture.ReadPixels(mRect, 0, 0);
        //应用
        mTexture.Apply();


        //将图片信息编码为字节信息
        byte[] bytes = mTexture.EncodeToPNG();
        //保存
        System.IO.File.WriteAllBytes(mFileName, bytes);

        //如果需要可以返回截图
        //return mTexture;
    }

    private IEnumerator CaptureByCamera(Camera mCamera, Rect mRect, string mFileName)
    {
        //等待渲染线程结束
        yield return new WaitForEndOfFrame();

        //初始化RenderTexture
        RenderTexture mRender = new RenderTexture((int)mRect.width, (int)mRect.height, 0);
        //设置相机的渲染目标
        mCamera.targetTexture = mRender;
        //开始渲染
        mCamera.Render();

        //激活渲染贴图读取信息
        RenderTexture.active = mRender;

        Texture2D mTexture = new Texture2D((int)mRect.width, (int)mRect.height, TextureFormat.RGB24, false);
        //读取屏幕像素信息并存储为纹理数据
        mTexture.ReadPixels(mRect, 0, 0);
        //应用
        mTexture.Apply();

        //释放相机，销毁渲染贴图
        mCamera.targetTexture = null;
        RenderTexture.active = null;
        GameObject.Destroy(mRender);

        //将图片信息编码为字节信息
        byte[] bytes = mTexture.EncodeToPNG();
        //保存
        System.IO.File.WriteAllBytes(mFileName, bytes);

        //如果需要可以返回截图
        //return mTexture;
    }
}

