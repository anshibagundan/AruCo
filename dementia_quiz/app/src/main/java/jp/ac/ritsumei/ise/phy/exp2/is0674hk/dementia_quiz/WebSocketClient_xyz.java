package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import okio.ByteString;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import org.json.JSONException;
import org.json.JSONObject;

public class WebSocketClient_xyz extends WebSocketListener {
    private static final String TAG = "WebSocketClient";

    private Handler mainHandler = new Handler(Looper.getMainLooper());
    private WebSocket webSocket;

    private CustomCircleView customCircleView;
    OkHttpClient client = new OkHttpClient();
    private Context context; // Contextを保持
    private int startPositionX=872;
    private int startPositionZ=964;


//    private static WebSocketClient_xyz instance;





    public void startWebsocket(String uuid) {
        Request request = new Request.Builder().url("wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/xyz/android/"+uuid).build();
        webSocket = client.newWebSocket(request, this);
    }

    // WebSocketを閉じるメソッドを追加する
    public void closeWebSocket() {
        if (webSocket != null) {
            webSocket.close(1000, "Closing connection");
            client.dispatcher().executorService().shutdown(); // 接続を閉じた後にシャットダウン
        }
    }


    public WebSocketClient_xyz(CustomCircleView customCircleView,Context context) {
        this.customCircleView = customCircleView;
        this.context=context;
    }

//    // インスタンスを取得するためのメソッド
//    public static synchronized WebSocketClient_xyz getInstance(CustomCircleView customCircleView,Context context) {
//        if (instance == null) {
//            instance = new WebSocketClient_xyz(customCircleView,context);
//        }
//        return instance;
//    }

    @Override
    public void onOpen(WebSocket webSocket, okhttp3.Response response) {
        Log.d(TAG, "WebSocket_xyz Connection Opened");
    }

    @Override
    public void onMessage(WebSocket webSocket, String text) {
        Log.d(TAG, "Received message: " + text);
        Log.d("xyz","受け取りok");
        // JSONをパースして内容を確認する
        try {
            JSONObject json = new JSONObject(text);

            // 画面の幅と高さを取得
            int screenWidth = context.getResources().getDisplayMetrics().widthPixels;
            int screenHeight = context.getResources().getDisplayMetrics().heightPixels;

            float x = (float) (((float)json.getDouble("x")+startPositionX)*0.145+450);
            float z = (float) (((float)json.getDouble("z")+startPositionZ)*(-0.165)+680);

            // 相対位置に変換
            float X = x / 1920 * screenWidth;  // 1920 は基準の幅（例）
            float Z = z / 1080 * screenHeight; // 1080 は基準の高さ（例）

            Log.d(TAG,  "x = " + x + " z = " + z);
            Log.d(TAG,  "X = " + X + " Z = " + Z);
            Log.d(TAG,  "screenWidth = " + screenWidth + " screenHeight = " + screenHeight);
            mainHandler.post(() -> {
                customCircleView.setCirclePosition(X, Z);
            }); //y座標はいらんっしょ

        } catch (JSONException e) {
            Log.e(TAG, "JSON parsing error: " + e.getMessage());
        }
    }

    @Override
    public void onMessage(WebSocket webSocket, ByteString bytes) {
        Log.d(TAG, "Received bytes message: " + bytes.hex());
    }

    @Override
    public void onClosing(WebSocket webSocket, int code, String reason) {
        webSocket.close(1000, null);
        Log.d(TAG, "WebSocket_xyz Connection Closing: " + code + " / " + reason);
    }

    @Override
    public void onFailure(WebSocket webSocket, Throwable t, okhttp3.Response response) {
        Log.e(TAG, "WebSocket Connection Failed: " + t.getMessage());
    }
}