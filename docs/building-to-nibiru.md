# Creating Unity builds for Nibiru headsets

## Build settings

Building directly to a Nibiru headset is the recommended method of running your app.

As the Nibiru operating system is an Android derivative, the build platform for your project must be set to **Android**.

This can be done via the **File › Build Settings** menu option.

Select **Android** and then click the **Switch Platform** button.

Change the **Texture Compression** option to **ETC2 (GLES 3.0)**.

<p align="center">
  <img alt="Switch platforms to Android" width="500px" src="assets/ChangeTextureCompressionImage.png">
</p>

## Building directly to a Nibiru headset

Connect your charged Nibiru device via USB to your development machine and then select the **File › Build & Run** menu option in Unity.

Your app should then be installed and launched on the device.

## Building an APK

Select the **File › Build Settings** menu option in Unity.

Click the **Build** button and select where you would like to save the APK on your file system.

## Next: Adding payments and user management

If you wish to sell your app on the Nibiru store, or access user details see [Working with the current user](/docs/nibiru-sdk-user-management.md).

If not, move on to  [optimizing your Nibiru experience](/docs/optimizing-nibiru-experiences.md).
