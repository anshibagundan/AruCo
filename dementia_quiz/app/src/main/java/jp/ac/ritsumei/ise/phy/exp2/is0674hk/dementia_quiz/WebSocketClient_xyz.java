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


//    private static WebSocketClient_xyz instance;





    public void startWebsocket() {
        Request request = new Request.Builder().url("wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/xyz/").build();
        webSocket = client.newWebSocket(request, this);
    }

    // WebSocketを閉じるメソッドを追加する
    public void closeWebSocket() {
        if (webSocket != null) {
            webSocket.close(1000, "Closing connection");
            client.dispatcher().executorService().shutdown(); // 接続を閉じた後にシャットダウン
        }
    }


    public WebSocketClient_xyz(CustomCircleView customCircleView, Context context) {
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
        Log.d(TAG, "WebSocket Connection Opened");
    }

    @Override
    public void onMessage(WebSocket webSocket, String text) {
        Log.d(TAG, "Received message: " + text);
        // JSONをパースして内容を確認する
        //TODO 座標位置を相対値にする
        try {
            JSONObject json = new JSONObject(text);

            //uuidが一致しているところのx,zを取得
            String uuid=json.getString("uuid");
            SharedPreferences uuidPrefs = context.getSharedPreferences("uuidPrefs", Context.MODE_PRIVATE);
            String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
            Log.d("UUID Check", "UUID: " + myuuid); // ログで確認
            if(uuid.equals(myuuid)){
                float x = (float) (((float)json.getDouble("x")+872)*0.145+265);
                float z = (float) (((float)json.getDouble("z")+966)*(-0.165)+1210);
                Log.d(TAG,  "x = " + x + " z = " + z);
                mainHandler.post(() -> {
                    customCircleView.setCirclePosition(x, z);
                }); //y座標はいらんっしょ
            }

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
        Log.d(TAG, "WebSocket Connection Closing: " + code + " / " + reason);
    }

    @Override
    public void onFailure(WebSocket webSocket, Throwable t, okhttp3.Response response) {
        Log.e(TAG, "WebSocket Connection Failed: " + t.getMessage());
    }
}