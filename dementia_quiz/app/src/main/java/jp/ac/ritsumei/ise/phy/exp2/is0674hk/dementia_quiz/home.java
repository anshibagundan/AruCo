package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
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



    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);

        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();

//        easy = findViewById(R.id.easy);
//        normal = findViewById(R.id.normal);
//        difficult = findViewById(R.id.difficult);
        quiz_selectDiff = findViewById(R.id.quiz_selectDiff);

        // WebSocket接続を確立
        startWebSocket();

        // 1から6のランダムな数字を生成
        randEasy = ThreadLocalRandom.current().nextInt(1, 7);
        randNormal = ThreadLocalRandom.current().nextInt(7, 13);
        randDifficult = ThreadLocalRandom.current().nextInt(13, 19);

        easy.setOnClickListener(v -> sendDataAndCloseWebSocket(randEasy));
        normal.setOnClickListener(v -> sendDataAndCloseWebSocket(randNormal));
        difficult.setOnClickListener(v -> sendDataAndCloseWebSocket(randDifficult));
    }

    // WebSocket接続を確立
    private void startWebSocket() {
        Request request = new Request.Builder().url("wss://teamhopcard-aa92d1598b3a.herokuapp.com/ws/hop/start/").build();
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
        JSONObject jsonObject = new JSONObject();
        try {
            jsonObject.put("start", "OK");
        } catch (JSONException e) {
            e.printStackTrace();
        }
        webSocket.send(jsonObject.toString());
        webSocket.close(1000, null);

        Act_select data = new Act_select(1, difficulty);
        apiService.insertAct_selectData(data).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, retrofit2.Response<Void> response) {
                if (response.isSuccessful()) {
                    Log.d("actDifficulty", "success");
                } else {
                    Log.e("actDifficulty", "fail");
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Log.e("actDifficulty", "connection_fail");
            }
        });

        Intent intent = new Intent(this, game.class);
        startActivity(intent);
    }

    // クイズの難易度をPOST＋画面遷移
    public void set_quizDifficulty(View view, int difficulty) {
        Quiz_select data = new Quiz_select(1, difficulty);
        apiService.insertQuiz_selectData(data).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, retrofit2.Response<Void> response) {
                if (response.isSuccessful()) {
                    Log.d("set_quizDifficulty", "success");
                } else {
                    Log.e("set_quizDifficulty", "fail");
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Log.e("set_quizDifficulty", "connection_fail");
            }
        });
    }

    public void set_quizEasy(View view) {
        set_quizDifficulty(view, 1);
    }

    public void set_quizNormal(View view) {
        set_quizDifficulty(view, 2);
    }

    public void set_quizDifficult(View view) {
        set_quizDifficulty(view, 3);
    }
}
