// Copyright 2016 Nibiru Inc. All rights reserved.
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
using UnityEditor;
using UnityEditor.Callbacks;
using Nvr.Internal;
/// A custom editor for properties on the NvrViewer script.  This appears in the
/// Inspector window of a NvrViewer object.  Its purpose is to allow changing the
/// `NvrViewer.Instance` object's properties from their default values.
[CustomEditor(typeof(NvrViewer))]
public class NvrViewerEditor : Editor {
    GUIContent directRenderEnabledLabel = new GUIContent("DirectRender  Enabled",
   "Whether to draw directly to the output window (true), or " +
                "to an offscreen buffer first and then blit (false).  Image " +
                " Effects and Deferred Lighting may only work if set to false.");

    GUIContent distortionEnabledLabel = new GUIContent("Distortion  Enabled",
     "Sets whether Distortion is enabled.");

    GUIContent vrModeLabel = new GUIContent("VR Mode Enabled",
      "Sets whether VR mode is enabled.");

   GUIContent vrLockHeadTracker = new GUIContent("Lock HeadTracker",
         "Sets whether Lock HeadTracker In Android.");

    GUIContent editorSettingsLabel = new GUIContent("Unity Editor Emulation Settings",
      "Controls for the in-editor emulation of a Cardboard viewer.");

  GUIContent autoUntiltHeadLabel = new GUIContent("Auto Untilt Head",
      "When enabled, just release Ctrl to untilt the head.");

  GUIContent screenSizeLabel = new GUIContent("Screen Size",
      "The screen size to emulate.");

  GUIContent viewerTypeLabel = new GUIContent("Viewer Type",
      "The viewer type to emulate."); 

  GUIContent qualityLabel = new GUIContent("Texture Quality",
      "The texture quality in android."); 

    /// @cond HIDDEN
    public override void OnInspectorGUI() {
    GUI.changed = false;

    GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
    headingStyle.fontStyle = FontStyle.Bold;

    NvrViewer nvrViewer = (NvrViewer)target;

    EditorGUILayout.LabelField("General Settings", headingStyle);

    nvrViewer.VRModeEnabled = EditorGUILayout.Toggle(vrModeLabel, nvrViewer.VRModeEnabled);

    nvrViewer.LockHeadTracker = EditorGUILayout.Toggle(vrLockHeadTracker, nvrViewer.LockHeadTracker);

    nvrViewer.DistortionEnabled = EditorGUILayout.Toggle(distortionEnabledLabel, nvrViewer.DistortionEnabled);
 
    nvrViewer.TextureQuality = (TextureQuality)EditorGUILayout.EnumPopup(qualityLabel,  nvrViewer.TextureQuality);

    nvrViewer.DirectRender = EditorGUILayout.Toggle(directRenderEnabledLabel , nvrViewer.DirectRender);
    if (GUI.changed)
    {
        EditorUtility.SetDirty(nvrViewer);
    }

        //EditorGUILayout.LabelField(editorSettingsLabel, headingStyle);
        //gvrViewer.autoUntiltHead =
        //    EditorGUILayout.Toggle(autoUntiltHeadLabel, gvrViewer.autoUntiltHead);
        //gvrViewer.ScreenSize = (NvrProfile.ScreenSizes)
        //    EditorGUILayout.EnumPopup(screenSizeLabel, gvrViewer.ScreenSize);
        //gvrViewer.ViewerType = (NvrProfile.ViewerTypes)
        //    EditorGUILayout.EnumPopup(viewerTypeLabel, gvrViewer.ViewerType);
 
    }
}
