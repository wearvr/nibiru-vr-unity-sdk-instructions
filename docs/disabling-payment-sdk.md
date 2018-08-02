# Disabling the Payment SDK

Before [submitting a test build](https://users.wearvr.com/developers/devices/nibiru/test-builds), you need to disable the payment SDK so that it may be tested.

Open the Unity **Edit** menu.

Select **Project Settings** and then **Player**.

In the Inspector, under **Other Settings**, enter *NIBIRU_PAYMENT_SDK_DISABLED* in the **Scripting Define Symbols*** field.

<p align="center">
  <img alt="Define symbol"  width="500px" src="assets/DefineSymbol.png">
</p>

## Enabling the Payment SDK

If you are submitting a [release build](https://users.wearvr.com/apps), then you need to make sure *NIBIRU_PAYMENT_SDK_DISABLED* does **NOT** appear in the **Scripting Define Symbols*** field.
