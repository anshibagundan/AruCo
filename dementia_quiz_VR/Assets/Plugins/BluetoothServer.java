package com.example.bluetooth;

import android.app.Activity;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothServerSocket;
import android.bluetooth.BluetoothSocket;
import android.util.Log;

import java.io.InputStream;
import java.util.UUID;

public class BluetoothServer {
    private static final String TAG = "BluetoothServer";
    private static final String APP_NAME = "BluetoothApp";
    private static UUID MY_UUID = UUID.fromString("YOUR-UNIQUE-UUID-STRING");
    private BluetoothAdapter bluetoothAdapter;
    private Activity activity;
    private String code;

    public BluetoothServer(Activity activity, String code) {
        this.activity = activity;
        this.code = code;
        bluetoothAdapter = BluetoothAdapter.getDefaultAdapter();

        if (bluetoothAdapter == null) {
            Log.e(TAG, "Device doesn't support Bluetooth");
        }

        // デバイス名にコードを含める
        bluetoothAdapter.setName("MetaQuest-" + code);
    }

    public void startServer() {
        new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    BluetoothServerSocket serverSocket = bluetoothAdapter.listenUsingRfcommWithServiceRecord(APP_NAME, MY_UUID);

                    Log.d(TAG, "Waiting for client connection...");
                    BluetoothSocket socket = serverSocket.accept();
                    Log.d(TAG, "Client connected");

                    InputStream inputStream = socket.getInputStream();

                    // UUID の読み取り
                    byte[] buffer = new byte[1024];
                    int bytes;
                    bytes = inputStream.read(buffer);
                    String receivedUUID = new String(buffer, 0, bytes);

                    Log.d(TAG, "Received UUID: " + receivedUUID);

                    // 受信した UUID を Unity に渡す（コールバックや UnitySendMessage を使用）

                    inputStream.close();
                    socket.close();
                    serverSocket.close();
                } catch (Exception e) {
                    Log.e(TAG, "Error: " + e.getMessage());
                }
            }
        }).start();
    }
}
