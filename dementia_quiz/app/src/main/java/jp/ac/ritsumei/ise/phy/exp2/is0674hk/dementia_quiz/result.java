package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import static jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz.history.adapter;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.TextView;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;

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
    private DataBaseHelper databaseHelper;
    public static ArrayList<String> dateList;
    private String date;
    private UserData UserData;
    private Context context; // Contextを保持
    private float tem_distance=1;
    WebSocketClient_result webSocketClient_result=new WebSocketClient_result(this);


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
        databaseHelper = new DataBaseHelper(this);
        getTF();
        MakeQuizPercent(quiz1,quiz2,quiz3);
        MakeActPercent(act1);
        percent=(act_percent+quiz_percent)*100;
        setTF_act();
        setTF_quiz();
    }


    //クイズの正誤を取得
    public void getTF(){
        quiz1=WebSocketClient_result.corList.get(0);
        quiz2=WebSocketClient_result.corList.get(1);
        quiz3=WebSocketClient_result.corList.get(2);
        act1=WebSocketClient_result.corList.get(3);
    }

    //〇✕テキストをセット
    public void setTF_act(){
        act1_text.setText(marubatsu(act1));
    }
    public void setTF_quiz() {
        quiz1_text.setText(marubatsu(quiz1));
        quiz2_text.setText(marubatsu(quiz2));
        quiz3_text.setText(marubatsu(quiz3));
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
    public void post_UserData(View view){
        SharedPreferences uuidPrefs = context.getSharedPreferences("uuidPrefs", Context.MODE_PRIVATE);
        String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
        Log.d("UUID Check", "UUID: " + myuuid); // ログで確認
        UserData=new UserData(myuuid,percent,tem_distance);
        apiService.postUserData(UserData).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (response.isSuccessful()){
                    Log.d("POST", "Data sent successfully");
                    webSocketClient_result.closeWebSocket();
                    Intent intent = new Intent(result.this, MainActivity.class);
                    startActivity(intent);
                }else {
                    Log.e("POST", "Failed to send data: " + response.message());
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Log.e("POST", "Request failed: " + t.getMessage());
            }
        });

    }

//    //perをPOST＋tfデータ削除＋画面遷移
//    public void post_per(View view){
//        Log.e("percent",String.valueOf(percent));
//        List<User> users = databaseHelper.getAllUsers();
//        int Listlength = users.size() + 1;
//        // 現在の日付を取得
//        date = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault()).format(new Date());
//        Log.e("percent","length;"+String.valueOf(Listlength));
//        User user =new User(Listlength,percent,date);
//        databaseHelper.insertUser(user);
//
//
//
//        apiService.deleteAllQuizTF().enqueue(new Callback<Void>() {
//            @Override
//            public void onResponse(Call<Void> call, Response<Void> response) {
//                if (response.isSuccessful()){
//                    //ACT-TFの削除リクエストを送信
//                    apiService.deleteAllActTF().enqueue(new Callback<Void>() {
//                        @Override
//                        public void onResponse(Call<Void> call, Response<Void> response) {
//                            if (response.isSuccessful()){
//                                Intent intent = new Intent(result.this, MainActivity.class);
//                                startActivity(intent);
//                            }else{
//                                // Act_TFの削除リクエストが失敗した場合のエラーハンドリング
//                            }
//                        }
//                        @Override
//                        public void onFailure(Call<Void> call, Throwable t) {
//                            // Act_TFの削除リクエストが失敗した場合のエラーハンドリング
//                        }
//                    });
//                }else{
//                    // Quiz_TFの削除リクエストが失敗した場合のエラーハンドリング
//                }
//            }
//
//            @Override
//            public void onFailure(Call<Void> call, Throwable t) {
//                // Quiz_TFの削除リクエストが失敗した場合のエラーハンドリング
//            }
//        });
//    }
}