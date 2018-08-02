# Nibiru Payment SDK

The Nibiru Payment SDK provides the ability to integrate with Nibiru’s payment and user management infrastructure.

This is a required step if you wish to sell your VR experience on the Nibiru platform.

The payment SDK works offline, so users do not require an internet connection to play your VR experience.

You should have already [integrated the Nibiru VR SDK](/docs/nibiru-vr-unity-sdk-installation.md) into your project and set up [user management](/docs/nibiru-sdk-user-management.md) to sign the current user in before you attempting these instructions.

## Paid apps

If you wish to sell your VR content on Nibiru, then you need to call the NPVRAndroid.enablePayForD() method before `NPVRAndroid.onStart()`:

```cs
void Start () {
	NPVRAndroid.init ();

	// Add the line below
	NPVRAndroid.enablePayForD ();

	NPVRAndroid.enableLog (true);
	NPVRAndroid.onStart ();
}
```

Once enabled, the Nibiru Payment SDK will close your app immediately unless it is launched on a Nibiru headset where the current user has paid for the experience.

## Next

If you are submitting a test build, you need to [disable the payment SDK](/docs/disabling-payment-sdk.md).

If you want to [release your app](/Readme.md), you need to make sure the [payment SDK is enabled](/docs/disabling-payment-sdk.md).
