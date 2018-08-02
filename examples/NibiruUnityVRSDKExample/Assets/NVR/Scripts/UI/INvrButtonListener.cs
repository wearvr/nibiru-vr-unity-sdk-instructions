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

using UnityEngine;
using System.Collections;

/// This script provides an interface for VR Button Press
public interface INvrButtonListener
{ 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="isKeyUp">true 按键抬起 false 按键按下</param>
    void OnPressEnter(bool isKeyUp);
     
    void OnPressLeft();
     
    void OnPressRight();

    void OnPressUp();

    void OnPressDown();

    void OnPressBack();

    void OnPressVolumnUp();

    void OnPressVolumnDown();
}

