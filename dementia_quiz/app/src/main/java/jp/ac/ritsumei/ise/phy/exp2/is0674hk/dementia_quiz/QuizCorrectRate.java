package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

public class QuizCorrectRate {
    private String name;
    private double correct_rate;
    private String detail;

    public QuizCorrectRate(String name, double correct_rate, String detail) {
        this.name = name;
        this.correct_rate = correct_rate;
        this.detail = detail;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public double getCorrect_rate() {
        return correct_rate;
    }

    public void setCorrect_rate(double correct_rate) {
        this.correct_rate = correct_rate;
    }

    public String getDetail() {
        return detail;
    }

    public void setDetail(String detail) {
        this.detail = detail;
    }
}
