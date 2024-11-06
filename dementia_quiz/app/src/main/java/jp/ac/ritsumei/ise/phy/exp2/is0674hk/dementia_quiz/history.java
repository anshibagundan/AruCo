package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.ScrollView;
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


public class history extends AppCompatActivity {

    public static ArrayAdapter<String> adapter;
    public FrameLayout popup;
    private LinearLayout historyContainer;
    private ApiService apiService;
    private String change_count,ratio,distance;
    private TextView change_count_text,ratio_text,distance_text;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_history);
        historyContainer=findViewById(R.id.historyContainer);
        apiService = ApiClient.getApiService();
//        loadHistory();
        getUserData();



        popup=findViewById(R.id.popup);
        change_count_text=findViewById(R.id.changed_count_text);
        ratio_text=findViewById(R.id.ratio_text);
        distance_text=findViewById(R.id.distance_text);
    }

    // 履歴のロードと表示
    private void loadHistory() {
        SharedPreferences sharedPreferences = getSharedPreferences("DateAndScore", MODE_PRIVATE);
        String timeData = sharedPreferences.getString("PressTime", "");
        String scoreData = sharedPreferences.getString("Scores", "");

        ArrayList<String> times = new ArrayList<>();
        ArrayList<String> scores = new ArrayList<>();
        // カンマ区切りで保存したデータを分割してリストに変換
        if (!timeData.isEmpty()) {
            String[] timeArray = timeData.split(",");
            for (String time : timeArray) {
                times.add(time);
            }
        }

        if (!scoreData.isEmpty()) {
            String[] scoreArray = scoreData.split(",");
            for (String score : scoreArray) {
                scores.add(score);
            }
        }
        Log.d("times", String.valueOf(times));
        Log.d("scores", String.valueOf(scores));
        Log.d("times.size()", String.valueOf(times.size()));
        Log.d("scores.size()", String.valueOf(scores.size()));
        for (int i = 0; i < times.size(); i++) {
            addHistoryItem(times.get(i), scores.get(i));
        }
    }

    // UIに履歴アイテムを追加
    private void addHistoryItem(String time, String score) {
        TextView textView = new TextView(this);
        textView.setText(time + " - " + score);
        textView.setTextSize(18);
        historyContainer.addView(textView);
    }





    public void history_main(View view){
        Intent intent =new Intent(this,MainActivity.class);
        startActivity(intent);
        popup.setVisibility(View.GONE);
    }

    public void getUserData(){
        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
        apiService.getUserData(myuuid).enqueue(new Callback<UserData>() {
            @Override
            public void onResponse(Call<UserData> call, Response<UserData> response) {
                if (response.isSuccessful()&&response.body()!=null){
                    UserData userData=response.body();
                    change_count=userData.getChange_count();
                    ratio=userData.getRatio();
                    distance=userData.getDistance();
                    int change_count_int=Integer.parseInt(change_count);
                    String playCount=String.valueOf(change_count_int+1);
                    change_count_text.setText(playCount+"回、");
                    ratio_text.setText(ratio+"%");
                    distance_text.setText(distance+"m)");
                    Log.e("UserData",userData.getChange_count()+userData.getRatio()+userData.getDistance());
                }else{
                    Log.e("GET", "Failed to get data: " + response.message());
                }
            }

            @Override
            public void onFailure(Call<UserData> call, Throwable t) {

            }
        });
    }



    }