package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.text.Editable;
import android.util.Log;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.FrameLayout;

import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.WebSocket;
import okhttp3.WebSocketListener;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import android.text.TextWatcher;
import android.widget.TextView;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.TimeUnit;


public class MainActivity extends AppCompatActivity {

    private Button start_button;
    private ApiService apiService;
    private EditText editText;
    private FrameLayout popup;
    private Button sendCode;
    private TextView errorText;
    private WebSocket webSocket;
    private final OkHttpClient client = new OkHttpClient();
    private String myuuid;
    private boolean websocketConnected ;
    private TextView caution;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        apiService = ApiClient.getApiService();
        start_button = findViewById(R.id.start);
        editText = findViewById(R.id.inputCode);
        popup=findViewById(R.id.popup);
        sendCode=findViewById(R.id.sendCode);
        errorText=findViewById(R.id.errorText);
        caution=findViewById(R.id.caution);

        // isFirstPrefsの初期化
        SharedPreferences isFirstPrefs = getSharedPreferences("isFirstPrefs", MODE_PRIVATE);

        // 初回起動かどうかを確認
        boolean isFirst = isFirstPrefs.getBoolean("isFirst", true);
        Log.d("isFirst Check", "isFirst value: " + isFirst);

        // デバッグ用コード（初回のみ追加する）
        SharedPreferences.Editor debugEditor = isFirstPrefs.edit();
        debugEditor.putBoolean("isFirst", true);
        debugEditor.apply();



        if (isFirst) {
            // 初回起動時の処理
            popup.setVisibility(View.VISIBLE);
            SharedPreferences.Editor editor = isFirstPrefs.edit();
            editor.putBoolean("isFirst", false);
            editor.apply();
            Log.d("UUID Check", "UUID: " + "通ってる"); // ログで確認
        }

        // 6桁入力されているかでボタンの透明度を変更
        editText.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
                // 何もしない
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                // 6桁入力されているか確認
                if (s.length() == 6) {
                    // 6桁ならボタンを不透明にし、有効化する
                    sendCode.setAlpha(0.9f);
                    errorText.setVisibility(View.GONE);
                } else {
                    // 6桁未満ならボタンを半透明にし、無効化する
                    sendCode.setAlpha(0.3f);
                }
            }

            @Override
            public void afterTextChanged(Editable s) {
                // 何もしない
            }
        });

    }






     //WebSocket接続を確立
    private void startWebSocket(String uuid) {
        Request request = new Request.Builder().url("wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/difficulty/android/"+uuid).build();

        webSocket = client.newWebSocket(request, new WebSocketListener() {
            @Override
            public void onOpen(WebSocket webSocket, okhttp3.Response response) {
                super.onOpen(webSocket, response);
                websocketConnected = true;
                Log.d("WebSocket", "Connected");
            }

            @Override
            public void onFailure(WebSocket webSocket, Throwable t, okhttp3.Response response) {
                super.onFailure(webSocket, t, response);
                websocketConnected = false;
                Log.e("WebSocket", "Connection failed: " + t.getMessage());
            }
        });
    }
    public void main_home(View view) {
        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        myuuid = uuidPrefs.getString("UUID", "default-uuid");
        startWebSocket(myuuid);
        Log.d("WebSocket", "Connected: " + websocketConnected);
        Log.d("UUID Check", "UUID: " + myuuid); // ログで確認
        if (websocketConnected) {
            Intent intent = new Intent(MainActivity.this, home.class);
            startActivity(intent);
        } else{
            caution.setVisibility(View.VISIBLE);
        }
    }

    public void main_history(View view){
        Intent intent = new Intent(this, history.class);
        startActivity(intent);

    }

    // /getuuidからuuidを取得し永久保存
    public void getAndsaveUUID(View view){
        // EditText から入力された値を取得し、文字列に変換
        String inputText = editText.getText().toString();
        closeKeyboard();

//        // 文字列を数値に変換して変数に格納(今のとこ使用してない)
//        int inputCode = Integer.parseInt(inputText);

        //入力された文字が6桁の場合
        if(inputText.length()==6){
            apiService.get_uuid(inputText).enqueue(new Callback<uuid>() {
//                @Override
//                public void onResponse(Call<uuid> call, Response<uuid> response) {
//                    if (response.isSuccessful()){
//                        Log.d("Success", "get_uuid()");
//                        String myuuid=response.body().getUuid();
//                        saveUUID(myuuid);
//                        popup.setVisibility(View.GONE);
//                    }
//                }

                @Override
                public void onResponse(Call<uuid> call, Response<uuid> response) {
                    if (response.isSuccessful()) {
                        // レスポンスのボディがnullでないか確認
                        if (response.body() != null) {
                            Log.d("Success", "get_uuid()");

                            myuuid = response.body().getUuid();
                            // UUIDが空でないことを確認
                            if (myuuid != null && !myuuid.isEmpty()) {
                                saveUUID(myuuid);
                                popup.setVisibility(View.GONE);
                            } else {
                                // UUIDが空の場合の処理
                                Log.e("UUID Error", "Received UUID is empty");
                                errorText.setText("UUIDが見つかりませんでした。");
                                errorText.setVisibility(View.VISIBLE);
                            }
                        } else {
                            // ボディがnullの場合の処理
                            Log.e("UUID Error", "Response body is null");
                            errorText.setText("UUIDが見つかりませんでした。");
                            errorText.setVisibility(View.VISIBLE);
                        }
                    } else {
                        // レスポンスが成功でない場合の処理
                        Log.e("Error", "Response not successful: " + response.code());
                        errorText.setText("エラーが発生しました。");
                        errorText.setVisibility(View.VISIBLE);
                    }
                }
                @Override
                public void onFailure(Call<uuid> call, Throwable t) {
                    Log.e("Error", t.getMessage());
                }
            });
        }else if (inputText.isEmpty()) {
            // 入力が空の場合の処理
            errorText.setVisibility(View.VISIBLE);
            errorText.setText("コードを入力してください。");
        }
        else {
            //6桁じゃないよーって表示するコードをかく
            errorText.setVisibility(View.VISIBLE);
            errorText.setText("※6桁入力してください");
        }
    }

    private void saveUUID(String uuid) {
        SharedPreferences uuidPrefs = getSharedPreferences("uuidPrefs", MODE_PRIVATE);
        SharedPreferences.Editor editor = uuidPrefs.edit();
        editor.putString("UUID", uuid);
        editor.apply();
        //TODO UUID受け取り成功かのテスト用コード
        String testuuid = uuidPrefs.getString("UUID", "デフォルト値");
        Log.d("UUID Check", "UUID: " + testuuid); // ログで確認
    }

    // キーボードを閉じる
    private void closeKeyboard() {
        View view = this.getCurrentFocus();
        if (view != null) {
            InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
            imm.hideSoftInputFromWindow(view.getWindowToken(), 0);
        }
    }

}