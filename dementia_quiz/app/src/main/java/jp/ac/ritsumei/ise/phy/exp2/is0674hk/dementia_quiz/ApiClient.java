package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class ApiClient {
    private static final String BASE_URL = "https://hopcardapi-4f6e9a3bf06d.herokuapp.com";

    private static Retrofit retrofit = null;

    public static ApiService getApiService() {
        if (retrofit == null) {
            // ロギングインターセプタを作成
            HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
            logging.setLevel(HttpLoggingInterceptor.Level.BODY);

            // カスタムOkHttpClientを作成
            OkHttpClient.Builder httpClient = new OkHttpClient.Builder();
            httpClient.addInterceptor(logging); // ロギングインターセプタを追加

            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .addConverterFactory(GsonConverterFactory.create())
                    .client(httpClient.build()) // カスタムOkHttpClientを使用
                    .build();
        }
        return retrofit.create(ApiService.class);
    }
}
