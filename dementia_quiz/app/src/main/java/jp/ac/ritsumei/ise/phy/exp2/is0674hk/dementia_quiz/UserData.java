package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

import java.util.List;

public class UserData {
    private String change_count;
    private String ratio;
    private String distance;
    private List<QuizCorrectRate> quiz_correct_rates;
    private String memoryRates;

    public UserData(String change_count,String ratio, String distance, List<QuizCorrectRate> quiz_correct_rates,String memoryRates){
        this.change_count=change_count;
        this.ratio=ratio;
        this.distance=distance;
        this.quiz_correct_rates=quiz_correct_rates;
        this.memoryRates=memoryRates;
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
    public String getMemoryQuizCorrectRates(){
        return memoryRates;
    }

}
