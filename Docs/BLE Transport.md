# BLE Transport

On the Arduino side, `BLECommands` creates a peripheral device (server) with a user-specified name, creates a service, and appends three characteristics to it:

- **Command** – used to send commands from the client to the device.
- **Response** – used to send command responses from the server to the client.
- **Listening** – used by the server to send notifications to the client.

Both client and server use predefined UUIDs for the service and characteristics:

|                   | UUID                                   |
|-------------------|----------------------------------------|
| Service           | `DB341FB3-8977-4C2D-AC6C-74540BD8B901` |
| Command Characteristic   | `DB341FB3-8977-4C2D-AC6C-74540BD8B902` |
| Response Characteristic  | `DB341FB3-8977-4C2D-AC6C-74540BD8B903` |
| Listening Characteristic | `DB341FB3-8977-4C2D-AC6C-74540BD8B904` |

> The Listening characteristic can be used by the server to send any information, such as current position or temperature. The client can start the listening procedure and receive all messages via events.  
> Introduce a final token in your protocol (e.g., `'END'`) so the client knows when to stop listening.

# Interaction
![](img/Interaction.png)

Maximum size of a command with arguments is 512 bytes. The length of responses to commands and outgoing messages is limited only by available memory; texts are encoded as UTF-8 strings.

To send large text, the [Arduino BleCommands library](https://github.com/andrey-val-rodin/BleCommands.Arduino) splits the message into chunks of up to 200 bytes. Each chunk is written to the Response or Listening characteristic and transmitted as a separate BLE notification. The client receives these notifications and reassembles the original message.

Chunks are created only at UTF-8 character boundaries. A multi-byte UTF-8 character is never split between two BLE notifications. This is a protocol requirement: peripherals implementing the BleCommands protocol must send valid UTF-8 fragments, and must not split a UTF-8 character across fragments. The .NET client decodes each received fragment separately and therefore is not intended to reassemble arbitrary byte streams from unrelated BLE peripherals.

To allow the client to detect the end of the transmission, a special delimiter character is used. The default is '\n'.
You can specify your own terminator in setup, for example:
```c++
TERMINATOR = '\x04'; // EOT
```
On the client side, create a BleTransport with the *tokenDelimiter* parameter specified:
```c#
await ArduinoClient.CreateTransportAsync("My BLE device", '\x04');
```

## Protocol requirements
The client-side library is designed to work with peripherals implementing the
[BleCommands.Arduino](https://github.com/andrey-val-rodin/BleCommands.Arduino)
protocol.

The protocol uses UTF-8 text and a token delimiter:

- every transmitted fragment must contain valid, complete UTF-8 sequences;
- a multi-byte UTF-8 character must not be split between fragments;
- fragments are reassembled until the configured token delimiter is received;
- the delimiter is not included in the resulting token;
- the default delimiter is `\n`, but both sides must use the same delimiter when it is overridden.

A peripheral that sends arbitrary binary fragments or splits UTF-8 sequences across
notifications is not compatible with the text transport implemented by
`BleCommands`.