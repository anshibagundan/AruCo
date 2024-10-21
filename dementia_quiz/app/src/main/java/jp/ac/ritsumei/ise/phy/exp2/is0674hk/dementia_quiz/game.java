package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import androidx.appcompat.app.AppCompatActivity;

import android.animation.ObjectAnimator;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class game extends AppCompatActivity {

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
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game);

        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();

        act_text = findViewById(R.id.act_text);
        quiz_text = findViewById(R.id.quiz_text);
        nowgame=findViewById(R.id.nowgame);
        customCircleView = findViewById(R.id.customCircleView);




        act_setText();
        quiz_setText();
        WebSocketClient webSocketClient = new WebSocketClient(customCircleView,this);
        webSocketClient.startWebsocket();
    }

    


    public void act_setText() {
        apiService.getAct_select().enqueue(new Callback<List<Act_select>>() {
            @Override
            public void onResponse(Call<List<Act_select>> call, Response<List<Act_select>> response) {
                int act_diff = 0;
                if (response.isSuccessful() && response.body() != null) {
                    Log.d("act_diff", String.valueOf(response.body().get(0)));
                    act_diff = response.body().get(0).getSelect_diff();
                    if (act_diff >= 1 && act_diff <= 6) {
                        act_diff_text = "簡単";
                    } else if (act_diff >= 7 && act_diff <= 12) {
                        act_diff_text = "普通";
                    } else if (act_diff >= 13 && act_diff <= 18) {
                        act_diff_text = "難しい";
                    } else {
                        act_diff_text = "未定義";  // 1~18以外の値の場合のデフォルトメッセージ
                    }
                    quiz_text.setText(String.valueOf(act_diff_text));
                    Log.d("act_diff", String.valueOf(act_diff));
                } else {
                    Log.e("act_diff", "fail ");
                }
            }

            @Override
            public void onFailure(Call<List<Act_select>> call, Throwable t) {
                Log.e("act_diff", "onFailure ");
            }
        });
    }

    public void quiz_setText() {
        apiService.getQuiz_select().enqueue(new Callback<List<Quiz_select>>() {
            @Override
            public void onResponse(Call<List<Quiz_select>> call, Response<List<Quiz_select>> response) {
                int quiz_diff = 0;
                if (response.isSuccessful() && response.body() != null) {
                    Log.d("quiz_diff", String.valueOf(response.body().get(0)));
                    quiz_diff = response.body().get(0).getSelect_diff();
                    switch (quiz_diff) {
                        case 1:
                            quiz_diff_text = "簡単";
                            break;
                        case 2:
                            quiz_diff_text = "普通";
                            break;
                        case 3:
                            quiz_diff_text = "難しい";
                            break;
                    }
                    act_text.setText(String.valueOf(quiz_diff_text));
                    Log.d("quiz_diff", String.valueOf(quiz_diff));
                } else {
                    Log.e("quiz_diff", "fail ");
                }
            }

            @Override
            public void onFailure(Call<List<Quiz_select>> call, Throwable t) {
                Log.e("quiz_diff", "onFailure");
            }
        });
    }



    //gameからresultへの画面遷移
    public void game_result(View view) {
        apiService.getAct_tfs().enqueue(new Callback<List<Act_TF>>() {
            @Override
            public void onResponse(Call<List<Act_TF>> call, Response<List<Act_TF>> response) {
                if (response.isSuccessful()&&response.body()!=null){
                    actSize=response.body().size();
                    Log.e("actSize",String.valueOf(actSize));
                    Log.e("quizSize",String.valueOf(quizSize));

                }else{
                    //エラー時ハンドリング
                }
            }

            @Override
            public void onFailure(Call<List<Act_TF>> call, Throwable t) {
                //エラー時ハンドリング
            }
        });
        apiService.getQuiz_tfs().enqueue(new Callback<List<Quiz_TF>>() {
            @Override
            public void onResponse(Call<List<Quiz_TF>> call, Response<List<Quiz_TF>> response) {
                if (response.isSuccessful()&&response.body()!=null){
                    Log.e("responcebody",String.valueOf(response.body()));
                    quizSize=response.body().size();
                    Log.e("responcebody",String.valueOf(quizSize));
                    if (actSize==1&&quizSize==3){
                        Intent intent =new Intent(game.this,result.class);
                        startActivity(intent);
                    }else{
                        nowgame.setText("クイズが終了していません！");
                    }
                }else{
                    //エラー時ハンドリング
                }
            }

            @Override
            public void onFailure(Call<List<Quiz_TF>> call, Throwable t) {
                //エラー時ハンドリング
            }
        });

    }





}