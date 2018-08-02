using UnityEngine;
using UnityEditor;


public class NvrMenu
{ 

    [MenuItem("NibiruVR/Developer Center", false, 100)]
    private static void OpenUnityGuide()
    {
        Application.OpenURL("http://dev.inibiru.com/download.jsp");
    } 

    [MenuItem("NibiruVR/About Nibiru VR", false, 200)]
    private static void OpenAbout()
    {
        EditorUtility.DisplayDialog("Nibiru VR SDK for Unity",
            "Version: " + NvrViewer.NVR_SDK_VERSION + "\n\n"
            + "QQ Group: 128275865. \n"
            + "Email: support@nibiruplayer.com. \n\n"
            + "Copyright: ©2017 Nibiru Inc. All rights reserved.\n",
            "OK");
    }
}
