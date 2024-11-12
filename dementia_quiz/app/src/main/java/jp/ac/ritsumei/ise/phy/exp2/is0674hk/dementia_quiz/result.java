package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import static jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz.history.adapter;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.TextView;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class result extends AppCompatActivity {
    private ApiService apiService;
    private boolean act1;
    private boolean quiz1,quiz2,quiz3;
    private float act_percent,quiz_percent,percent;
    int act_count,quiz_count;
    private TextView act1_text;
    private TextView quiz1_text;
    private TextView quiz2_text;
    private TextView quiz3_text;

    public static ArrayList<String> dateList;
    private String date;
    private UserData UserData;
    private Context context; // Contextを保持
    private TextView distanceText;
    private TextView feedbackText;
    WebSocketClient_result webSocketClient_result=new WebSocketClient_result(this);
    String quiz1_name,quiz2_name,quiz3_name;
    private TextView quiz1_name_text,quiz2_name_text,quiz3_name_text;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_result);
        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();
        act1_text=findViewById(R.id.act1_text);

        quiz1_text=findViewById(R.id.quiz1_text);
        quiz2_text=findViewById(R.id.quiz2_text);
        quiz3_text=findViewById(R.id.quiz3_text);
        distanceText=findViewById(R.id.distanceText);
        feedbackText=findViewById(R.id.feedbackText);
        quiz1_name_text=findViewById(R.id.quiz1_name_text);
        quiz2_name_text=findViewById(R.id.quiz2_name_text);
        quiz3_name_text=findViewById(R.id.quiz3_name_text);

//        clearDateAndScore();
        getTF();
        MakeQuizPercent(quiz1,quiz2,quiz3);
        MakeActPercent(act1);
        percent=(act_percent+quiz_percent)*100;
        setTF_act();
        setTF_quiz();
        setDistance(WebSocketClient_result.distance);
        setFeedBack(WebSocketClient_result.feedback);
        setQuizName();

    }


    //クイズの正誤を取得
    public void getTF(){
        quiz1=WebSocketClient_result.corList.get(0);
        quiz2=WebSocketClient_result.corList.get(1);
        quiz3=WebSocketClient_result.corList.get(2);
        act1=WebSocketClient_result.corList.get(3);
    }
    //クイズの問題を取得
    public void getQuizName(){
        quiz1_name=WebSocketClient_result.quiz_nameList.get(0);
        quiz2_name=WebSocketClient_result.quiz_nameList.get(1);
        quiz3_name=WebSocketClient_result.quiz_nameList.get(2);
    }
    public void setQuizName(){
        getQuizName();
        quiz1_name_text.setText(quiz1_name);
        quiz2_name_text.setText(quiz2_name);
        quiz3_name_text.setText(quiz3_name);
    }

    //〇✕テキストをセット
    public void setTF_act(){
        act1_text.setText(marubatsu(act1));
        if (act1) {
            act1_text.setTextColor(Color.parseColor("#ff0000"));
        } else {
            act1_text.setTextColor(Color.parseColor("#0000ff"));
        }
    }
    public void setTF_quiz() {
        // quiz1の結果を設定し、色を変更
        quiz1_text.setText(marubatsu(quiz1));
        if (quiz1) { // quiz1がtrueの場合
            quiz1_text.setTextColor(Color.parseColor("#ff0000")); // 赤（#ff0000）
        } else { // quiz1がfalseの場合
            quiz1_text.setTextColor(Color.parseColor("#0000ff")); // 青（#0000ff）
        }

        // quiz2の結果を設定し、色を変更
        quiz2_text.setText(marubatsu(quiz2));
        if (quiz2) { // quiz2がtrueの場合
            quiz2_text.setTextColor(Color.parseColor("#ff0000")); // 赤（#ff0000）
        } else { // quiz2がfalseの場合
            quiz2_text.setTextColor(Color.parseColor("#0000ff")); // 青（#0000ff）
        }

        // quiz3の結果を設定し、色を変更
        quiz3_text.setText(marubatsu(quiz3));
        if (quiz3) { // quiz3がtrueの場合
            quiz3_text.setTextColor(Color.parseColor("#ff0000")); // 赤（#ff0000）
        } else { // quiz3がfalseの場合
            quiz3_text.setTextColor(Color.parseColor("#0000ff")); // 青（#0000ff）
        }
    }

    //距離を表示
    public void setDistance(String distance){
        distanceText.setText(distance+"m");
    }
    public void setFeedBack(String feedback){
        feedbackText.setText(feedback);
    }

    //booleanから〇✕返す
    public String marubatsu(boolean value) {
        Log.e("marubatu",String.valueOf(value));
        if (value) {
            return "〇";
        } else {
            return "✕";
        }
    }


    //履歴に残す%の計算
    public void MakeActPercent(boolean act1){
        act_count=0;
        if(act1){
            act_count+=40;
        }
        act_percent=(float) act_count/100;
    }
    public void MakeQuizPercent(boolean quiz1,boolean quiz2,boolean quiz3){
        quiz_count=0;
        if (quiz1) {
            quiz_count+=20;
            Log.e("quiz_count",String.valueOf(quiz_count));
        }
        if (quiz2) {
            quiz_count+=20;
            Log.e("quiz_count",String.valueOf(quiz_count));
        }
        if (quiz3) {
            quiz_count+=20;
            Log.e("quiz_count",String.valueOf(quiz_count));
        }
        Log.e("quiz_count",String.valueOf(quiz_count));
        quiz_percent=(float)quiz_count/100;
    }


    // UserData(UUID,per,distance)をPOST
    public void result_main(View view){
//        saveDateAndScore();
        webSocketClient_result.closeWebSocket();
        WebSocketClient_result.corList.clear();
        WebSocketClient_result.quiz_nameList.clear();
        Intent intent = new Intent(result.this, MainActivity.class);
        startActivity(intent);



//        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
//        String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
//        Log.d("UUID Check", "UUID: " + myuuid); // ログで確認
//        UserData=new UserData(myuuid,percent,WebSocketClient_result.distance);
//        Log.d("UserData",myuuid+percent+WebSocketClient_result.distance);
//        apiService.postUserData(UserData).enqueue(new Callback<Void>() {
//            @Override
//            public void onResponse(Call<Void> call, Response<Void> response) {
//                if (response.isSuccessful()){
//                    Log.d("POST", "Data sent successfully");
//                    webSocketClient_result.closeWebSocket();
//                    Intent intent = new Intent(result.this, MainActivity.class);
//                    startActivity(intent);
//                }else {
//                    Log.e("POST", "Failed to send data: " + response.message());
//                }
//            }
//
//            @Override
//            public void onFailure(Call<Void> call, Throwable t) {
//                Log.e("POST", "Request failed: " + t.getMessage());
//            }
//        });

    }

    public void saveDateAndScore(){
        long currentTime = System.currentTimeMillis();
        String timeStr = formatTime(currentTime);
        Log.d("Time", timeStr);
        SharedPreferences sharedPreferences = getSharedPreferences("DateAndScore", MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        // 既存のデータをカンマ区切りの文字列として取得
        String timeData = sharedPreferences.getString("PressTime", "");
        String scoreData = sharedPreferences.getString("Scores", "");

        // 新しい値を追加
        timeData = timeData.isEmpty() ? timeStr : timeData + "," + timeStr;
        scoreData = scoreData.isEmpty() ? percent + "%" : scoreData + "," + percent + "%";
        Log.d("TimeData", timeData);
        Log.d("ScoreData", scoreData);
        editor.putString("PressTime", timeData);
        editor.putString("Scores", scoreData);
        editor.apply();
    }


    // 時刻をフォーマットする
    private String formatTime(long time) {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy/MM/dd HH:mm", Locale.getDefault());
        return sdf.format(new Date(time));
    }

    //デバッグ用
    private void clearDateAndScore() {
        SharedPreferences sharedPreferences = getSharedPreferences("DateAndScore", MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.clear(); // すべてのデータをクリア
        editor.apply(); // 変更を適用

    }


}