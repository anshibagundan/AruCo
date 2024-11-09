package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.ScrollView;
import android.widget.TextView;

import java.math.RoundingMode;
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
import java.math.BigDecimal;


public class history extends AppCompatActivity {

    public static ArrayAdapter<String> adapter;
    private LinearLayout historyContainer;
    private ApiService apiService;
    private String change_count,ratio,distance;
    private TextView change_count_text,ratio_text,distance_text;
    private LinearLayout quizCorrectRatesContainer;
    private TextView memoryQuizCorrectRates;
    private String action_correct_rate;
    private FrameLayout popup_quiz;
    private TextView totalFeedbackText;
    private String message;
    private Button quizHistoryButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_history);
//        historyContainer=findViewById(R.id.historyContainer);
        apiService = ApiClient.getApiService();
//        loadHistory();
        getUserData();

        quizHistoryButton = findViewById(R.id.quizHistoryButton);
        totalFeedbackText = findViewById(R.id.totalFeedbackText);
        popup_quiz=findViewById(R.id.popup_quiz);
        change_count_text=findViewById(R.id.changed_count_text);
        ratio_text=findViewById(R.id.ratio_text);
        distance_text=findViewById(R.id.distance_text);
        quizCorrectRatesContainer = findViewById(R.id.quizCorrectRatesContainer);
        memoryQuizCorrectRates = findViewById(R.id.memoryQuizCorrectRates);
    }


    public void popup_gone(View view){
        popup_quiz.setVisibility(View.GONE);
        quizHistoryButton.setEnabled(true);
    }
    public void popup_visible(View view){
        popup_quiz.setVisibility(View.VISIBLE);
        quizHistoryButton.setEnabled(false);
    }




    public void history_main(View view){
        Intent intent =new Intent(this,MainActivity.class);
        startActivity(intent);
    }

    public void getUserData() {
        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
        apiService.getUserData(myuuid).enqueue(new Callback<UserData>() {
            @Override
            public void onResponse(Call<UserData> call, Response<UserData> response) {
                if (response.isSuccessful() && response.body() != null) {
                    UserData userData = response.body();
                    change_count = userData.getChange_count();
                    message = userData.getMessage();
                    // ratio の四捨五入処理
                    ratio = userData.getRatio();
                    double ratioValue = Double.parseDouble(ratio);
                    BigDecimal roundedRatio = new BigDecimal(ratioValue * 100).setScale(1, RoundingMode.HALF_UP);
                    //action_correct_rateの四捨五入処理
                    action_correct_rate = userData.getAction_correct_rate();
                    double action_correct_rateValue = Double.parseDouble(action_correct_rate);
                    BigDecimal roundedAction_correct_rate = new BigDecimal(action_correct_rateValue * 100).setScale(1, RoundingMode.HALF_UP);
                    distance = userData.getDistance();
                    int change_count_int = Integer.parseInt(change_count);
                    String playCount = String.valueOf(change_count_int + 1);
                    change_count_text.setText(playCount + "回、");
                    ratio_text.setText(roundedRatio + "%");
                    distance_text.setText(distance + "m)");
                    memoryQuizCorrectRates.setText(roundedAction_correct_rate+"%");
                    totalFeedbackText.setText(message);
                    Log.d("memoryRates", action_correct_rate);
                    Log.d("totalFeedback", message);

                    List<QuizCorrectRate> quizCorrectRates = userData.getQuiz_correct_rates();
                    Log.d("quizCorrectRates", quizCorrectRates.toString());

                    if (quizCorrectRates != null) {
                        for (QuizCorrectRate quiz : quizCorrectRates) {
                            TextView textView = new TextView(history.this);
                            textView.setText("問題: " + quiz.getName() + "\n" +
                                    "正解率: " + (quiz.getCorrect_rate() * 100) );
                            textView.setTextSize(16);
                            textView.setPadding(0, 10, 0, 15);

                            quizCorrectRatesContainer.addView(textView);
                        }
                    }
                } else {
                    Log.e("GET", "Failed to get data: " + response.message());
                }
            }

            @Override
            public void onFailure(Call<UserData> call, Throwable t) {
                Log.e("GET", "API call failed: " + t.getMessage());
            }
        });
    }



    }