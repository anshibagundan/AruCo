package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import java.util.List;

public class UserData {
    private String change_count;
    private String ratio;
    private String distance;
    private List<QuizCorrectRate> quiz_correct_rates;
    private String action_correct_rate;
    private String message;

    public UserData(String change_count,String ratio, String distance, List<QuizCorrectRate> quiz_correct_rates,String action_correct_rate,String massage){
        this.change_count=change_count;
        this.ratio=ratio;
        this.distance=distance;
        this.quiz_correct_rates=quiz_correct_rates;
        this.action_correct_rate=action_correct_rate;
        this.message=massage;
    }
    public String getChange_count(){
        return change_count;
    }
    public String getRatio(){
        return ratio;
    }
    public String getDistance(){
        return distance;
    }
    public List<QuizCorrectRate> getQuiz_correct_rates() {
        return quiz_correct_rates;
    }
    public String getAction_correct_rate(){
        return action_correct_rate;
    }
    public String getMessage(){
        return message;
    }

}
