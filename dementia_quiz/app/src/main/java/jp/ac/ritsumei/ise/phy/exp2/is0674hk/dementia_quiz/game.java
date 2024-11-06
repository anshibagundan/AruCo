package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import static androidx.constraintlayout.helper.widget.MotionEffect.TAG;

import android.content.Intent;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import androidx.appcompat.app.AppCompatActivity;


import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;

import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;

import org.json.JSONObject;

import java.net.URI;

import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import okio.ByteString;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.util.Base64;
import android.widget.ImageView;

import androidx.appcompat.app.AppCompatActivity;

import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;

import org.json.JSONObject;

import java.net.URI;
import java.net.URISyntaxException;
import android.util.Log;
import javax.net.ssl.SSLSocketFactory;
import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.cert.X509Certificate;

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
    private ImageView diff_image;
    private ImageView live;
    private WebSocketClient webSocketClient;
    private String serverUrl;
    private String TAG = "WebSocket";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_game);

        // ApiServiceインスタンスを取得
        apiService = ApiClient.getApiService();



        customCircleView = findViewById(R.id.customCircleView);
        finish_button=findViewById(R.id.finish_button);
        diff_image=findViewById(R.id.diff_image);
        live=findViewById(R.id.live);


        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        String myuuid = uuidPrefs.getString("UUID", "デフォルト値");
        Log.d("UUID Check", "UUID: " + myuuid); // ログで確認
        serverUrl = "wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/cast/android/"+myuuid;

        Log.d(TAG,"kekekekeke"+serverUrl);

        finish_button.setEnabled(false);
        setDiff(home.difficulty);
        WebSocketClient_xyz webSocketClient_xyz = new WebSocketClient_xyz(customCircleView,this);
        WebSocketClient_result webSocketClient_result=new WebSocketClient_result(this);
        webSocketClient_xyz.startWebsocket(myuuid);
        webSocketClient_result.startWebsocket(myuuid);

        try {
            URI url = new URI(serverUrl);

            // SSL証明書の検証を無視（テスト目的のみ）
            TrustManager[] trustAllCerts = new TrustManager[]{
                    new X509TrustManager() {
                        public void checkClientTrusted(X509Certificate[] chain, String authType) {}
                        public void checkServerTrusted(X509Certificate[] chain, String authType) {}
                        public X509Certificate[] getAcceptedIssuers() { return new X509Certificate[0]; }
                    }
            };
            SSLContext sslContext = SSLContext.getInstance("TLS");
            sslContext.init(null, trustAllCerts, new java.security.SecureRandom());
            SSLSocketFactory factory = sslContext.getSocketFactory();

            webSocketClient = new WebSocketClient(url) {
                @Override
                public void onOpen(ServerHandshake handshakedata) {
                    runOnUiThread(() -> {
                        Log.d(TAG, "WebSocket_cast接続成功");
                    });
                }

                @Override
                public void onMessage( String message) {
                    runOnUiThread(() -> {
                        Log.d(TAG, "メッセージ受信");
                        try {
                            JSONObject jsonObject = new JSONObject(message);
                            String base64Image = jsonObject.getString("data");
                            byte[] imageBytes = Base64.decode(base64Image, Base64.DEFAULT);
                            Bitmap bitmap = BitmapFactory.decodeByteArray(imageBytes, 0, imageBytes.length);
                            live.setImageBitmap(bitmap);
                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    });
                }

                @Override
                public void onClose(int code, String reason, boolean remote) {
                    runOnUiThread(() -> {
                        Log.d(TAG, "WebSocket切断: " + reason + ", Code: " + code);
                    });
                }

                @Override
                public void onError(Exception ex) {
                    runOnUiThread(() -> {
                        Log.e(TAG, "WebSocketエラー", ex);
                    });
                }
            };

            // ファクトリーを設定
            webSocketClient.setSocketFactory(factory);
            webSocketClient.connect();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (webSocketClient != null) {
            webSocketClient.close();
        }
    }








    //難易度を表示
    public void setDiff(int difficulty){
        if(difficulty==1){
            diff_image.setImageResource(R.drawable.easy_button);
        }
        if(difficulty==2){
            diff_image.setImageResource(R.drawable.normal_button);
        }
        if (difficulty==3){
            diff_image.setImageResource(R.drawable.hard_button);
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