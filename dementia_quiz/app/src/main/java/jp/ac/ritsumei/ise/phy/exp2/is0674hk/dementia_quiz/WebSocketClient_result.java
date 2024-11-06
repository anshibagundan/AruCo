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
import android.view.View;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

public class WebSocketClient_result extends WebSocketListener {
    private static final String TAG = "WebSocketClient";

    private Handler mainHandler = new Handler(Looper.getMainLooper());
    private WebSocket webSocket;

    private CustomCircleView customCircleView;
    OkHttpClient client = new OkHttpClient();
    private Context context; // Contextを保持

    private static WebSocketClient_xyz instance;

    // ブーリアン型のリストを作成
    public static List<Boolean> corList = new ArrayList<>();
    public static String feedback;
    public static String distance;
    WebSocketClient_xyz webSocketClient_xyz=new WebSocketClient_xyz(customCircleView,context);




    public void startWebsocket(String uuid) {
        Request request = new Request.Builder().url("wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/result/android/"+uuid).build();
        webSocket = client.newWebSocket(request, this);
    }

    // WebSocketを閉じるメソッドを追加する
    public void closeWebSocket() {
        if (webSocket != null) {
            webSocket.close(1000, "Closing connection");
            client.dispatcher().executorService().shutdown(); // 接続を閉じた後にシャットダウン
        }
    }


    public WebSocketClient_result(Context context) {
        this.context=context;
    }

    @Override
    public void onOpen(WebSocket webSocket, okhttp3.Response response) {
        Log.d(TAG, "WebSocket_result Connection Opened");
    }

    @Override
    public void onMessage(WebSocket webSocket, String text) {
        Log.d(TAG, "Received message: " + text);
        // JSONをパースして内容を確認する
        try {
            JSONObject json = new JSONObject(text);

//            String uuid=json.getString("uuid");
//            SharedPreferences uuidPrefs = context.getSharedPreferences("uuidPrefs", Context.MODE_PRIVATE);
//            String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
//            Log.d("UUID Check", "UUID: " + myuuid); // ログで確認

            distance=json.getString("distance");
            feedback=json.getString("message");
            JSONArray cor=json.getJSONArray("cor");
            // corからブーリアン型の値をリストに追加
            for (int i = 0; i < cor.length(); i++) {
                corList.add(cor.getBoolean(i));
            }
            if(corList.size()==4){
                mainHandler.post(new Runnable() {
                    @Override
                    public void run() {
                        // UI操作はここで行う
                        game.finish_button.setAlpha(0.9f);
                        game.finish_button.setEnabled(true);
                    }
                });
                // /xyzのwebsocketを閉じる
                webSocketClient_xyz.closeWebSocket();
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