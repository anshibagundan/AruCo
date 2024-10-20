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
    private Button easy;
    private Button normal;
    private Button difficult;
    private FrameLayout act_selectDiff;
    private FrameLayout quiz_selectDiff;
    private WebSocket webSocket;
    private final OkHttpClient client = new OkHttpClient();
    private int randEasy,randNormal,randDifficult;
    private String myuuid;



    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();

        // SharedPreferences から UUID を取得
        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        myuuid = uuidPrefs.getString("UUID", "default-uuid");

        // WebSocket接続を確立
        startWebSocket();


    }

    // WebSocket接続を確立
    private void startWebSocket() {
        Request request = new Request.Builder().url("wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/difficulty").build();
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
    private void sendDataAndCloseWebSocket(String uuid, int difficulty) {
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("uuid", uuid);
            jsonObject.put("difficulty",difficulty);
        } catch (JSONException e) {
            e.printStackTrace();
        }
        webSocket.send(jsonObject.toString());
        webSocket.close(1000, null);

        Intent intent = new Intent(this, game.class);
        startActivity(intent);
    }

    // 別クラスで SharedPreferences から uuid を取得する


    public void setEasy() {
        sendDataAndCloseWebSocket(myuuid,1);
    }
    public void setNormal(){
        sendDataAndCloseWebSocket(myuuid,2);
    }
    public void setDifficult(){
        sendDataAndCloseWebSocket(myuuid,3);
    }
}
