package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;

import java.util.List;

    public interface ApiService {

        @GET("/quiz-tfs/")
        Call<List<Quiz_TF>> getQuiz_tfs();
        @GET("/quiz-tfs/{id}/")
        Call<Quiz_TF> getQuiz_tfsById(@Path("id") int quiz_tfsId);
        @GET("/act-tfs/")
        Call<List<Act_TF>> getAct_tfs();
        @GET("/act-tfs/{id}/")
        Call<Act_TF> getAct_tfsById(@Path("id") int act_tfsId);


        @GET("/act-selects/")
        Call<List<Act_select>> getAct_select();
        @GET("/act_selects/{id}/")
        Call<Act_select> getAct_selectById(@Path("id") int Act_selectId);
        @GET("/quiz-selects/")
        Call<List<Quiz_select>> getQuiz_select();
        @GET("/quiz_selects/{id}/")
        Call<Quiz_select> getQuiz_selectById(@Path("id") int Quiz_selectId);


        @POST("/quiz-selects/")
        Call<Void> insertQuiz_selectData(@Body Quiz_select data);
        @POST("/act-selects/")
        Call<Void> insertAct_selectData(@Body Act_select data);


        //ここで前データ削除用メソッド定義
        @DELETE("/quiz-selects/destroy_all/")
        Call<Void> deleteAllQuizSelect();

        @DELETE("/act-selects/destroy_all/")
        Call<Void> deleteAllActSelect();

        @DELETE("/quiz-tfs/destroy_all/")
        Call<Void> deleteAllQuizTF();

        @DELETE("/act-tfs/destroy_all/")
        Call<Void> deleteAllActTF();

        //UUIDをサーバから受け取る
        @GET("getuuid/{six_code}")
        Call<uuid> get_uuid(@Path("six_code") String six_code);


    }
