# Nibiru Unity SDK Instructions

Instructions for how to create new Unity virtual reality experiences for Nibiru VR headsets (or port existing ones).

<p align="center">
  <img alt="Nibiru" width="500px" src="/docs/assets/Nibiru.svg">
</p>

## Nibiru Headsets

#### Support & Revenue share

WEARVR.com, the world's largest independent VR app store, has partnered with Nibiru to provide development kits and assistance with promotion, technical support and advice to help get your content into Nibiru's global marketplace (including China) - at no cost to you. You get the same high revenue share.

| Region | Developer Revenue Share |
| :---: | :----: |
| China | 60% |
| Outside China | 70% |

#### Specifications

View the [full headset specifications](https://www.wearvr.com/developer-center/devices/nibiru).

#### Requesting a development kit

You can [request a Nibiru headset](/docs/nibiru-development-kit.md) to help get your VR experiences Nibiru-compatible.

## Prerequisites

The minimum supported version of Unity is V5.4.0f3.

## Overview

You can easily create a new Unity VR app, but the fastest way to get up and running on Nibiru is to convert an existing Google Cardboard, Google Daydream or Samsung Gear VR experience.

* [Installing and configuring the Nibiru VR Unity SDK](/docs/nibiru-vr-unity-sdk-installation.md)
* [Camera & input module setup](/docs/nibiru-vr-camera-setup.md)
* [Headset buttons](/docs/nibiru-buttons.md)
* [Enabling USB debugging](/docs/nibiru-developer-mode-usb-debugging.md)
* [Building to the device](/docs/building-to-nibiru.md)

Optional:

* [Working with the current user](/docs/nibiru-sdk-user-management)
* [Selling your app & payment SDK](/docs/nibiru-payment-sdk.md)
* [Performance optimization](/docs/optimizing-nibiru-experiences.md)

There is an [example project](examples/NibiruUnityVRSDKExample/Readme.md) to use as a reference as you follow this guide.

## Uploading and selling your experiences

When you are ready, it's time to release your Nibiru VR experiences to the global and Chinese Nibiru stores.

If you are <a href="https://users.wearvr.com/developers/devices/nibiru/test-builds" target="_blank">submitting a test build</a>, you need to [disable the payment SDK](/docs/disabling-payment-sdk.md) so that it may be tested.

If you want to <a href="https://users.wearvr.com/apps" target="_blank">submit the release version of your app</a>, you need to make sure the [payment SDK is enabled](/docs/disabling-payment-sdk.md#enabling-the-payment-sdk).

## Copyright & Trademarks

These instructions and example project are maintained by WEARVR LLC, the largest independent virtual reality app store. WEARVR is interested in connecting VR content creators and consumers, globally. We love working with the VR community and would be delighted to hear from you at `devs@wearvr.com`.

You can find more information about WEARVR at www.wearvr.com

The Nibiru trademark, Nibiru operating system, Nibiru virtual reality headsets and Nibiru VR Unity SDK are all owned by [Nibiru Technology Co., Ltd](http://www.inibiru.com/en/vr.html).
