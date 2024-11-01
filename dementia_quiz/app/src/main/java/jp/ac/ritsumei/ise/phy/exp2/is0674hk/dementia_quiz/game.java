package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import static androidx.constraintlayout.helper.widget.MotionEffect.TAG;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;

import android.animation.ObjectAnimator;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;

import org.json.JSONException;
import org.json.JSONObject;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import okio.ByteString;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class game extends AppCompatActivity {
    private Handler mainHandler = new Handler(Looper.getMainLooper());
    private ApiService apiService;
    private TextView act_text;
    private TextView quiz_text;
    private String quiz_diff_text;
    private String act_diff_text;
    private CustomCircleView customCircleView;
    private int quizSize,actSize;
    private TextView nowgame;
    private WebSocket webSocket;
    private final OkHttpClient client = new OkHttpClient();
    private Context context; // Contextを保持
    public static Button finish_button;
    public TextView diff_text;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game);

        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();


        nowgame=findViewById(R.id.nowgame);
        customCircleView = findViewById(R.id.customCircleView);
        finish_button=findViewById(R.id.finish_button);
        diff_text=findViewById(R.id.diff_text);


        SharedPreferences uuidPrefs = context.getSharedPreferences("uuidPrefs", Context.MODE_PRIVATE);
        String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
        Log.d("UUID Check", "UUID: " + myuuid); // ログで確認

        setDiff(home.difficulty);
        WebSocketClient_xyz webSocketClient_xyz = new WebSocketClient_xyz(customCircleView,this);
        WebSocketClient_result webSocketClient_result=new WebSocketClient_result(this);
        webSocketClient_xyz.startWebsocket(myuuid);
        webSocketClient_result.startWebsocket(myuuid);
    }


    //難易度を表示
    public void setDiff(int difficulty){
        if(difficulty==1){
            diff_text.setText("かんたん");
        }
        if(difficulty==2){
            diff_text.setText("ふつう");
        }
        if (difficulty==3){
            diff_text.setText("むずかしい");
        }
    }

    public void game_result(View view){
        Intent intent =new Intent(game.this,result.class);
        startActivity(intent);
    }

//    //gameからresultへの画面遷移
//    public void game_result(View view) {
//        apiService.getAct_tfs().enqueue(new Callback<List<Act_TF>>() {
//            @Override
//            public void onResponse(Call<List<Act_TF>> call, Response<List<Act_TF>> response) {
//                if (response.isSuccessful()&&response.body()!=null){
//                    actSize=response.body().size();
//                    Log.e("actSize",String.valueOf(actSize));
//                    Log.e("quizSize",String.valueOf(quizSize));
//
//                }else{
//                    //エラー時ハンドリング
//                }
//            }
//
//            @Override
//            public void onFailure(Call<List<Act_TF>> call, Throwable t) {
//                //エラー時ハンドリング
//            }
//        });
//        apiService.getQuiz_tfs().enqueue(new Callback<List<Quiz_TF>>() {
//            @Override
//            public void onResponse(Call<List<Quiz_TF>> call, Response<List<Quiz_TF>> response) {
//                if (response.isSuccessful()&&response.body()!=null){
//                    Log.e("responcebody",String.valueOf(response.body()));
//                    quizSize=response.body().size();
//                    Log.e("responcebody",String.valueOf(quizSize));
//                    if (actSize==1&&quizSize==3){
//                        Intent intent =new Intent(game.this,result.class);
//                        startActivity(intent);
//                    }else{
//                        nowgame.setText("クイズが終了していません！");
//                    }
//                }else{
//                    //エラー時ハンドリング
//                }
//            }
//
//            @Override
//            public void onFailure(Call<List<Quiz_TF>> call, Throwable t) {
//                //エラー時ハンドリング
//            }
//        });
//
//    }





}