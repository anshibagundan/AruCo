package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.FrameLayout;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.concurrent.ThreadLocalRandom;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import retrofit2.Call;
import retrofit2.Callback;

public class home extends AppCompatActivity {

    private ApiService apiService;
    private WebSocket webSocket;
    private final OkHttpClient client = new OkHttpClient();
    private String myuuid;
    public static int difficulty;



    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();

        // SharedPreferences から UUID を取得
        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        myuuid = uuidPrefs.getString("UUID", "default-uuid");
        Log.d("UUID Check ホーム開いたとき", "UUID: " + myuuid); // ログで確認
        startWebSocket(myuuid);



    }
    // WebSocket接続を確立
    private void startWebSocket(String uuid) {
        Request request = new Request.Builder().url("wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/difficulty/android/"+uuid).build();

        webSocket = client.newWebSocket(request, new WebSocketListener() {
            @Override
            public void onOpen(WebSocket webSocket, okhttp3.Response response) {
                super.onOpen(webSocket, response);
                Log.d("WebSocket", "Connected");
            }

            @Override
            public void onFailure(WebSocket webSocket, Throwable t, okhttp3.Response response) {
                super.onFailure(webSocket, t, response);
                Log.e("WebSocket", "Connection failed: " + t.getMessage());
            }
        });
    }


    // データを送信してWebSocket接続を閉じる
    private void sendDataAndCloseWebSocket(int difficulty) {
        if (webSocket == null) {
            Log.e("WebSocket", "WebSocket not connected");
            return;
        }
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("difficulty",difficulty);
        } catch (JSONException e) {
            e.printStackTrace();
        }
        webSocket.send(jsonObject.toString());
        Log.d("WebSocket", "Sent: " + jsonObject);
        this.difficulty=difficulty;



        Intent intent = new Intent(this, game.class);
        startActivity(intent);
        Log.d("WebSocket", "scene changed");
    }


    @Override
    protected void onPause() {
        super.onPause();
        if (webSocket != null) {
            webSocket.close(1000, "Activity Pausing");
            client.dispatcher().executorService().shutdown(); // 接続を閉じた後にシャットダウン
            Log.d("WebSocket", "Closed in onPause()");
        }
    }



    public void setEasy(View view) {
        sendDataAndCloseWebSocket(1);
        Log.d("難易度", "かんたん");
    }
    public void setNormal(View view) {
        sendDataAndCloseWebSocket(2);
        Log.d("難易度", "ふつう");
    }
    public void setDifficult(View view){
        sendDataAndCloseWebSocket(3);
        Log.d("難易度", "むずかしい");
    }

}
