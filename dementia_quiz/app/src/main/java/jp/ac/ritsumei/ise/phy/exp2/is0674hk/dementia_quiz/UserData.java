package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

public class UserData {
    private String change_count;
    private String ratio;
    private String distance;

    public UserData(String change_count,String ratio, String distance){
        this.change_count=change_count;
        this.ratio=ratio;
        this.distance=distance;
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

}
