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

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import android.text.TextWatcher;
import android.widget.TextView;


public class MainActivity extends AppCompatActivity {

    private Button start_button;
    private ApiService apiService;
    private EditText editText;
    private FrameLayout popup;
    private Button sendCode;
    private TextView errorText;

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




    public void main_home(View view) {
        Intent intent = new Intent(MainActivity.this, home.class);
        startActivity(intent);
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

        // 文字列を数値に変換して変数に格納(今のとこ使用してない)
        int inputCode = Integer.parseInt(inputText);

        //入力された文字が6桁の場合
        if(inputText.length()==6){
            apiService.get_uuid(inputText).enqueue(new Callback<uuid>() {
                @Override
                public void onResponse(Call<uuid> call, Response<uuid> response) {
                    if (response.isSuccessful()){
                        Log.d("Success", "get_uuid()");
                        String myuuid=response.body().getUuid();
                        saveUUID(myuuid);
                        popup.setVisibility(View.GONE);
                    }
                }

                @Override
                public void onFailure(Call<uuid> call, Throwable t) {
                    Log.e("Error", t.getMessage());
                }
            });
        }else {
            //6桁じゃないよーって表示するコードをかく
            errorText.setVisibility(View.VISIBLE);
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