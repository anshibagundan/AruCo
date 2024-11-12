package jp.ac.ritsumei.ise.phy.exp2.is0674hk.dementia_quiz;

public class uuid {
    private String uuid;
    private int sixCode;
    public uuid(int sixCode, String uuid){
        this.sixCode=sixCode;
        this.uuid=uuid;
    }
    public String getUuid(){return this.uuid;}

}
