package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import android.content.ContentValues;
import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.database.Cursor;
import android.util.Log;

import java.util.ArrayList;
import java.util.List;

public class DataBaseHelper extends SQLiteOpenHelper {

    private static final String DATABASE_NAME = "AndoroidDevice.db"; // データベース名
    private static final int DATABASE_VERSION = 2; //データベース更新時にバージョンの値を上げていくこと
    private static final String TABLE_NAME_USERS = "users"; //生成するテーブル名
    private static final String USERS_COLUMN_ID = "id"; // テーブル内の属性1
    private static final String USERS_COLUMN_PER = "per";// テーブル内の属性2
    private static final String USERS_COLUMN_DATE = "date";// テーブル内の属性3



    //コンストラクタを定義
    public DataBaseHelper(Context context) {
        super(context, DATABASE_NAME, null, DATABASE_VERSION);
    }


    // データベースが初めて作成されたときに呼び出される
    @Override
    public void onCreate(SQLiteDatabase db) {
        String createTableQuery = "CREATE TABLE " + TABLE_NAME_USERS + " (" +
                USERS_COLUMN_ID + " INTEGER PRIMARY KEY AUTOINCREMENT, " +
                USERS_COLUMN_PER + " TEXT, " +
                USERS_COLUMN_DATE + " TEXT)";
        db.execSQL(createTableQuery);
    }

    //データベースのバージョンが変更されたときに呼び出されるテーブルをアップグレードするメソッド
    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_NAME_USERS);
        onCreate(db);
    }

    //データベースからすべてのユーザーを取得するメソッド
    public List<User> getAllUsers() {
        List<User> users = new ArrayList<>();
        SQLiteDatabase db = getReadableDatabase();
        Cursor cursor = db.query(TABLE_NAME_USERS, null, null, null, null, null, null);
        while (cursor.moveToNext()) {
            int id = cursor.getInt(cursor.getColumnIndexOrThrow(USERS_COLUMN_ID));
            float per = cursor.getFloat(cursor.getColumnIndexOrThrow(USERS_COLUMN_PER));
            String date = cursor.getString(cursor.getColumnIndexOrThrow(USERS_COLUMN_DATE));
            users.add(new User(id, per,date));
        }Log.e("getall",String.valueOf(users));
        cursor.close();
        db.close();
        return users;
    }
    //データベースにユーザーデータをPOSTするメソッド
    public long insertUser(User user) {
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues values = new ContentValues();
        values.put(USERS_COLUMN_ID, user.getId());
        values.put(USERS_COLUMN_PER, user.getPer());
        values.put(USERS_COLUMN_DATE,user.getDate());

        long rowId = db.insert(TABLE_NAME_USERS, null, values);
        db.close();
        return rowId;
    }

    // 各行のperとdate列をNULLに更新するメソッド
    public void clearPerAndDate() {
        SQLiteDatabase db = this.getWritableDatabase();
//        ContentValues values = new ContentValues();
        db.execSQL("DELETE FROM "+TABLE_NAME_USERS);

//        db.update(TABLE_NAME_USERS, values, null, null);
        db.close();
    }






}

