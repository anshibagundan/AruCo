package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;

import java.util.List;

    public interface ApiService {

        // UUIDをサーバから受け取る
        @GET("getuuid")
        Call<uuid> get_uuid(@Query("code") String six_code);

        //ユーザーデータ(uuid,per,distance)を送る
        @POST("postuserdata")
        Call<Void> postUserData(@Body UserData UserData);

        @GET("getuserdata")
        Call<UserData> getUserData(@Query("uuid") String uuid);


    }
