# Nibiru Payment SDK

The Nibiru Payment SDK provides the ability to integrate with Nibiru’s payment and user management infrastructure.

This is a required step if you wish to sell your VR experience on the Nibiru platform.

The payment SDK works offline, so users do not need an internet connection to play your VR experience.

## Paid apps

You should have already [integrated the Nibiru VR SDK](/docs/nibiru-vr-unity-sdk-installation.md) into your project and set up [user management](/docs/nibiru-sdk-user-management.md) to sign the current user in before you attempting these instructions.

If you wish to sell your VR content on Nibiru, then you need to call the `NPVRAndroid.enablePayForD()` method before `NPVRAndroid.onStart()`:

```cs
# if !NIBIRU_PAYMENT_SDK_DISABLED
void Start () {
	NPVRAndroid.init ();

	// Add the line below
	NPVRAndroid.enablePayForD ();

	NPVRAndroid.enableLog (true);
	NPVRAndroid.onStart ();
}
#endif
```

Once enabled, the Nibiru Payment SDK will close your app immediately unless it is launched on a Nibiru headset where the current user has paid for the experience.

## Next: Optimization

See [Performance optimization](/docs/optimizing-nibiru-experiences.md).
