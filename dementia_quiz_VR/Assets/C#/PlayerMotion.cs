using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

public class PlayerMotion : MonoBehaviour
{
    // 左右の手のアンカーとなるトランスフォームを設定
    [SerializeField] private Transform LeftHandAnchorTransform = null;
    [SerializeField] private Transform RightHandAnchorTransform = null;
    private CharacterController Controller;
    private String geturl = "https://teamhopcard-aa92d1598b3a.herokuapp.com/quiz-tfs/";

    //回転
    public GameObject objectToRotate;
    public float rotationDuration = 1f;
    private Quaternion endRotation;
    private bool Rotated = false;

    // 移動に関するパラメータ
    private Vector3 MoveThrottle = Vector3.zero;
    private float MoveScale = 1.0f;
    private float MoveScaleMultiplier = 1.0f;
    private float SimulationRate = 60f;
    private float FallSpeed = 0.0f;
    private float Acceleration = 0.1f;
    private float Damping = 0.3f;
    private float GravityModifier = 0f;

    // コントローラーの速度と加速度を保存する変数
    private Vector3 touchVelocityL;
    private Vector3 touchVelocityR;
    private Vector3 touchAccelerationL;
    private Vector3 touchAccelerationR;
    private bool motionInertia = false;
    private float motionInertiaDuration = 1.0f;

    // 歩行と走行のしきい値
    const float WALK_THRESHOLD = 0.8f;
    const float RUN_THRESHOLD = 1.3f;
    public float moveScale = 0.3f;
    private bool QuizTFData1;
    private bool QuizTFData2;
    private bool QuizTFData3;
    private int QuizIdData1;
    private int QuizIdData2;
    private int QuizIdData3;
    private QuizTF[] QuizTFDataArray;

    private void Start()
    {
        // CharacterControllerコンポーネントを取得
        Controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 手を振る動作による移動制御
        HandShakeController();
        // CharacterControllerの更新
        UpdateController();

        // デバッグ用のログ出力
        Debug.Log("L-touch velocity: " + touchVelocityL);
        Debug.Log("R-touch velocity: " + touchVelocityR);
        Debug.Log("L-touch acceleration: " + touchAccelerationL);
        Debug.Log("R-touch acceleration: " + touchAccelerationR);
        Debug.Log("MoveThrottle: " + MoveThrottle);
    }

    private void HandShakeController()
    {
        // 左右のコントローラーの入力デバイスを取得
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        // コントローラーの速度と加速度を取得
        leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out touchVelocityL);
        rightController.TryGetFeatureValue(CommonUsages.deviceVelocity, out touchVelocityR);
        leftController.TryGetFeatureValue(CommonUsages.deviceAcceleration, out touchAccelerationL);
        rightController.TryGetFeatureValue(CommonUsages.deviceAcceleration, out touchAccelerationR);

        // 地面に接地しているかどうかで移動スケールを調整
        if (!IsGrounded()) MoveScale = 0.0f;
        else MoveScale = 1.0f;

        MoveScale *= SimulationRate * Time.deltaTime;

        float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;

        // 速度の大きい方の手を選択
        Transform activeHand;
        Vector3 handShakeVel;
        Vector3 handShakeAcc;

        if (Math.Abs(touchVelocityL.y) > Math.Abs(touchVelocityR.y))
        {
            activeHand = LeftHandAnchorTransform;
            handShakeVel = touchVelocityL;
            handShakeAcc = touchAccelerationL;
        }
        else
        {
            activeHand = RightHandAnchorTransform;
            handShakeVel = touchVelocityR;
            handShakeAcc = touchAccelerationR;
        }

        // 選択した手の回転を取得し、z軸とx軸の回転をゼロに設定
        Quaternion ort = activeHand.rotation;
        Vector3 ortEuler = ort.eulerAngles;
        ortEuler.z = ortEuler.x = 0f;
        ort = Quaternion.Euler(ortEuler);

        // 移動効果を計算し、MoveThrottleに加算
        MoveThrottle += CalculateMoveEffect(moveInfluence, ort, handShakeVel, handShakeAcc);
    }

    private Vector3 CalculateMoveEffect(float moveInfluence, Quaternion ort, Vector3 handShakeVel, Vector3 handShakeAcc)
    {
        Vector3 tmpMoveThrottle = Vector3.zero;

        // 歩行状態かどうかを判定
        bool isWalk = DetectHandShakeWalk(Math.Abs(handShakeVel.y)) || motionInertia;
        if (isWalk)
        {
            if (!motionInertia){}
                SetMotionInertia();

            // ワールド座標のx軸方向にのみ移動するように設定
            //rightはX正 leftがX負 forwardがZ軸正 backZ負方向でした．
            //idが奇数でtrueなら左に，偶数でtrueなら右にします
            Getdirection();



                //1問も解いていない時
            if (QuizTFDataArray == null)
            {
                tmpMoveThrottle += Vector3.right * moveScale;
            }

            //1問目を解いたことありますか、左の時
            else if ((QuizIdData1 % 2 == 0 && QuizTFData1) || (QuizIdData1 % 2 == 1 && !QuizTFData1))
            {
                //1問目をといた直後ですか
                if (QuizTFDataArray.Length == 1)
                {
                    //1回目に呼び出された後ですか
                    if (Rotated)
                    {
                        RotateCoroutine("L");

                    }

                    tmpMoveThrottle += Vector3.forward * moveScale;
                }

                //2問目を解いたことありますか、右の時
                else if ((QuizIdData2 % 2 == 1 && QuizTFData2) || (QuizIdData2 % 2 == 0 && !QuizTFData2))
                {
                    //2問目をといた直後ですか
                    if (QuizTFDataArray.Length == 2)
                    {
                        //1回目に呼び出された後ですか
                        if (Rotated)
                        {
                            RotateCoroutine("R");

                        }

                        tmpMoveThrottle += Vector3.right * moveScale;
                    }

                    //3問目を解いたことありますか、左の時
                    else if ((QuizIdData3 % 2 == 0 && QuizTFData3) || (QuizIdData3 % 2 == 1 && !QuizTFData3))
                    {
                        //3問目をといた直後ですか
                        if (QuizTFDataArray.Length == 3)
                        {
                            //1回目に呼び出された後ですか
                            if (Rotated)
                            {
                                RotateCoroutine("L");

                            }

                            tmpMoveThrottle += Vector3.left * moveScale;
                        }
                        //3問目を解いたことありますか、直進の時
                        else if ((QuizIdData3 % 2 == 1 && QuizTFData3) || (QuizIdData3 % 2 == 0 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("F");

                                }

                                tmpMoveThrottle += Vector3.right * moveScale;
                            }
                        }
                    }
                    //2問目をといたことありますか、左の時
                    else if ((QuizIdData2 % 2 == 0 && QuizTFData2) || (QuizIdData2 % 2 == 1 && !QuizTFData2))
                    {
                        //2問目をといた直後ですか
                        if (QuizTFDataArray.Length == 2)
                        {
                            //1回目に呼び出された後ですか
                            if (Rotated)
                            {
                                RotateCoroutine("L");

                            }

                            tmpMoveThrottle += Vector3.left * moveScale;
                        }
                        //3問目を解いたことありますか、直進の時
                        else if ((QuizIdData3 % 2 == 1 && QuizTFData3) || (QuizIdData3 % 2 == 0 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("F");

                                }

                                tmpMoveThrottle += Vector3.right * moveScale;
                            }
                        }
                        //3問目を解いたことありますか、左の時
                        else if ((QuizIdData3 % 2 == 0 && QuizTFData3) || (QuizIdData3 % 2 == 1 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("L");

                                }

                                tmpMoveThrottle += Vector3.back * moveScale;
                            }
                        }
                    }



                }

                //1問目を解いたことありますか、右の時
                else if ((QuizIdData1 % 2 == 1 && QuizTFData1) || (QuizIdData1 % 2 == 0 && !QuizTFData1))
                {

                    //1問目をといた直後ですか
                    if (QuizTFDataArray.Length == 1)
                    {
                        //1回目に呼び出された後ですか
                        if (Rotated)
                        {
                            RotateCoroutine("R");

                        }

                        tmpMoveThrottle += Vector3.forward * moveScale;
                    }
                    //2問目を解いたことありますか、左の時
                    else if ((QuizIdData2 % 2 == 0 && QuizTFData2) || (QuizIdData2 % 2 == 1 && !QuizTFData2))
                    {
                        //2問目をといた直後ですか
                        if (QuizTFDataArray.Length == 2)
                        {
                            //1回目に呼び出された後ですか
                            if (Rotated)
                            {
                                RotateCoroutine("L");

                            }

                            tmpMoveThrottle += Vector3.right * moveScale;
                        }
                        //3問目を解いたことありますか、直進の時
                        else if ((QuizIdData3 % 2 == 1 && QuizTFData3) || (QuizIdData2 % 2 == 0 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("F");

                                }

                                tmpMoveThrottle += Vector3.right * moveScale;
                            }
                        }
                        //3問目を解いたことありますか、左の時
                        else if ((QuizIdData3 % 2 == 0 && QuizTFData3) || (QuizIdData3 % 2 == 1 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("L");

                                }

                                tmpMoveThrottle += Vector3.forward * moveScale;
                            }
                        }
                    }
                    //2問目を解いたことありますか、右の時
                    else if ((QuizIdData2 % 2 == 1 && QuizTFData2) || (QuizIdData2 % 2 == 0 && !QuizTFData2))
                    {
                        //2問目をといた直後ですか
                        if (QuizTFDataArray.Length == 2)
                        {
                            //1回目に呼び出された後ですか
                            if (Rotated)
                            {
                                RotateCoroutine("R");

                            }

                            tmpMoveThrottle += Vector3.left * moveScale;
                        }
                        //3問目を解いたことありますか、直進の時
                        else if ((QuizIdData3 % 2 == 1 && QuizTFData3) || (QuizIdData3 % 2 == 0 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("F");

                                }

                                tmpMoveThrottle += Vector3.left * moveScale;
                            }
                        }
                        //3問目を解いたことありますか、左の時
                        else if ((QuizIdData3 % 2 == 0 && QuizTFData3) || (QuizIdData3 % 2 == 1 && !QuizTFData3))
                        {
                            //3問目をといた直後ですか
                            if (QuizTFDataArray.Length == 3)
                            {
                                //1回目に呼び出された後ですか
                                if (Rotated)
                                {
                                    RotateCoroutine("L");

                                }

                                tmpMoveThrottle += Vector3.back * moveScale;
                            }
                        }
                    }
                }
            }



            // 走行状態かどうかを判定
            bool isRun = DetectHandShakeRun(Math.Abs(handShakeVel.y));
            if (isRun)
                tmpMoveThrottle *= 2.0f;
            Debug.Log("HandShake Move Effect: " + tmpMoveThrottle);
        }

        return tmpMoveThrottle;
    }

    IEnumerator SetMotionInertia()
    {
        // モーション慣性を有効にし、一定時間後に無効にする
        motionInertia = true;
        yield return new WaitForSecondsRealtime(motionInertiaDuration);
        motionInertia = false;
    }

    private bool DetectHandShakeWalk(float speed)
    {
        // 地面に接地していない場合は歩行状態ではない
        if (!IsGrounded()) return false;
        // 速度がしきい値を超えている場合は歩行状態
        if (speed > WALK_THRESHOLD) return true;
        return false;
    }

    private bool DetectHandShakeRun(float speed)
    {
        // 地面に接地していない場合は走行状態ではない
        if (!IsGrounded()) return false;
        // 速度がしきい値を超えている場合は走行状態
        if (speed > RUN_THRESHOLD) return true;
        return false;
    }

    private bool IsGrounded()
    {
        // CharacterControllerが地面に接地している場合はtrueを返す
        return true;

        // レイキャストを使用して地面との接地判定を行う
        /*var pos = transform.position;
        var ray = new Ray(pos + Vector3.up * 0.1f, Vector3.down);
        var tolerance = 0.3f;
        return Physics.Raycast(ray, tolerance);
       */
        }

    private void UpdateController()
    {
        Vector3 moveDirection = Vector3.zero;

        // 移動量の減衰
        float motorDamp = 2.0f + (Damping * SimulationRate * Time.deltaTime);

        MoveThrottle.x /= motorDamp;
        MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
        MoveThrottle.z /= motorDamp;

        // 移動方向の計算
        moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;

        // 重力の計算
        if (Controller.isGrounded && FallSpeed <= 0)
            FallSpeed = Physics.gravity.y * (GravityModifier * 0.002f);
        else
            FallSpeed += Physics.gravity.y * (GravityModifier * 0.002f) * SimulationRate * Time.deltaTime;

        moveDirection.y += FallSpeed * SimulationRate * Time.deltaTime;

        // 段差を乗り越える処理
        if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
        {
            float bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
            moveDirection -= bumpUpOffset * Vector3.up;
        }

        // 移動予測の計算
        Vector3 predictedXZ = Vector3.Scale(Controller.transform.localPosition + moveDirection, new Vector3(1, 0, 1));

        // CharacterControllerの移動
        Controller.Move(moveDirection);

        // 実際の移動量の計算
        Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

        // 予測移動量と実際の移動量が異なる場合は、MoveThrottleを調整
        if (predictedXZ != actualXZ)
            MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
    }


    private IEnumerator Getdirection()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(geturl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                string json = webRequest.downloadHandler.text;

                QuizTFDataArray = JsonHelper.FromJson<QuizTF>(json);

                if (QuizTFDataArray != null && QuizTFDataArray.Length > 0)
                {
                    QuizTFData1 = QuizTFDataArray[0].getCor();
                    QuizIdData1 = QuizTFDataArray[0].getId();

                    if (QuizTFDataArray.Length > 1)
                    {
                        QuizTFData2 = QuizTFDataArray[1].getCor();
                        QuizIdData2 = QuizTFDataArray[2].getId();
                    }

                    //3問目いらないならこれいらんくない？
                    if (QuizTFDataArray.Length > 2)
                    {
                        QuizTFData3 = QuizTFDataArray[2].getCor();
                        QuizIdData3 = QuizTFDataArray[3].getId();
                    }

                }
                else
                {
                    Debug.LogWarning("No quizdiff found.");
                }
            }
        }
    }
    //回転用
    IEnumerator RotateCoroutine(String LorR)
    {
        Quaternion startRotation = objectToRotate.transform.rotation;

        if (LorR == "R")
        {
            endRotation = startRotation * Quaternion.Euler(0, 90, 0);
        }
        else if (LorR == "L")
        {
            endRotation = startRotation * Quaternion.Euler(0, -90, 0);
        }
        else
        {
            endRotation = startRotation * Quaternion.Euler(0, 0, 0);
        }




        float elapsedTime = 0;

        while (elapsedTime < rotationDuration)
        {
            objectToRotate.transform.rotation =
                Quaternion.Slerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectToRotate.transform.rotation = endRotation;
        Rotated = true;
    }

    //ここから長さ取得
    private int quizTFCount = 0;

    private IEnumerator GetQuizTFCoroutine()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(geturl))
        {
            webRequest.SetRequestHeader("X-Debug-Mode", "true");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                quizTFCount = 0;
            }
            else
            {
                string json = webRequest.downloadHandler.text;
                QuizTF[] quizTFDataArray = JsonUtility.FromJson<QuizTF[]>("{\"Items\":" + json + "}");
                quizTFCount = quizTFDataArray.Length;
            }
        }
    }

    public void StartGetQuizTF()
    {
        StartCoroutine(GetQuizTFCoroutine());
    }

    public int GetQuizTFCount()
    {
        return quizTFCount;
    }

}