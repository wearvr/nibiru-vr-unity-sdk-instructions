# Nibiru in-app purchases

## Set a callback

To interact with the Nibiru payment servers, you must first use `setUnityObjName()` to register a GameObject to handle payment events.

`setUnityObjName()` takes the name of the GameObject as its single string argument. It should be called before `NPVRAndroid.onStart()`.

```cs
void Start () {
	NPVRAndroid.setUnityObjName ('ObjectName');
	NPVRAndroid.onStart ();
}
```

Attach a Script to the GameObject named above; this will be the Script that you need to define all callback methods on.

## Displaying the payment UI

> All transactions using the Nibiru payment service are done in the virtual currency, N-Coins. 1 N-Coin is worth 1/7th of $1 (USD).

All of your app’s payment logic should be contained within an `onNPVRServiceResult()` method, which is called once a connection to the payment service has been completed.

This method must be defined on the Script attached to the GameObject registered with `NPVRAndroid.setUnityObjName()` in the previous step.

`onNPVRServiceResult()` is called with a single parameter: the string value 'true' when the connection was successful, or 'false' when the connection attempt failed.

#### Example

```cs
void onNPVRServiceResult(string isSuccessful){
	if (bool.Parse(isSuccessful))
	{
		// Connection success
	}
	else
	{
		// Connection failed
	}
}
```

Once the connection has been established, Nibiru’s payment UI can be displayed using the NPVRAndroid.payUI() method:

```cs
NPVRAndroid.payUI(string itemId, string itemName, double price)
```

##### Parameters

| Parameter | Description |
| :--- | :--- |
| itemId | A string containing the id of the item the payment is for. |
| itemName | A string containing the name of the item the payment is for. |
| price | A double representing the price the item is being sold for, in N-Coins. |

## Receiving the purchase result

Once a purchase request has been made, the `onNPVRPayResult()` method is called.

```cs
onNPVRPayResult(string result)
```

Where `result` is a string containing a comma-separated list of parameters that describe the result of the purchase request. They are, in order:

| Parameter | Description |
| :--- | :--- |
| status | A status code describing the result of the payment request. Refer to Payment Statuses below. |
| orderId | The purchase’s order number |
| productId | The product id set when opening the payment UI above |
| productName | The product name set when opening the payment UI above |
| price | The purchase price set when opening the payment UI above |

##### Example

```cs
void onNPVRPayResult(string result){
	string[] resultParams = result.Split (new string[]{","}, System.StringSplitOptions.None);

	int status = int.Parse (resultParams[0]);

	string orderId = resultParams[1];
	string productId = resultParams[2];
	string productName = resultParams[3];

	double price = double.Parse (resultParams[4]);
}
```

### Payment Statuses

| `NPVRAccount.PAYMENT_RES_*` | Value | Description |
| PAYMENT_RES_SUCC | 100 | Payment successful |
| PAYMENT_RES_CANCEL | 104 | Payment was cancelled by the user |
| PAYMENT_RES_NOASSIS | 119 | Payment failed because NibiruVR Assistant could not be found on the headset |
| PAYMENT_RES_NETWORK_FAILED | 105 | Payment failed due to a network failure |
| PAYMENT_RES_FAILED | 101 | Payment failed on the server |
| PAYMENT_RES_NO_SUCH_USER | 103 | Payment failed because user could not be found or validated |
| PAYMENT_RES_BALANCE_NOENOUGH | 102 | Payment failed due to the user having insufficient N-Coins to make the purchase |
| PAYMENT_RES_REPEAT | 107 | Payment failed because an identical one was already received |
| PAYMENT_RES_NO_CONSISTANT | 108 | Payment failed because it was not valid |
| PAYMENT_RES_UNKNOWN | -1 | Payment failed due to unknown error |

## Checking the payment service status is available

You can check if the payment service is available using the following method:

```
NPVRAndroid.isEnable();
```

## Next

If you are submitting a test build, you need to [disable the payment SDK](/docs/disabling-payment-sdk.md) so that it may be tested.

If you want to [release your app](/Readme.md), you need to make sure the [payment SDK is enabled](/docs/disabling-payment-sdk.md).
