using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using App.BaseSystem.DataStores.ScriptableObjects.Status;
using UnityEngine.SceneManagement;
public class PlayerMotion : MonoBehaviour
{
    // 左右の手のアンカーとなるトランスフォームを設定
    [SerializeField] private Transform LeftHandAnchorTransform = null;
    [SerializeField] private Transform RightHandAnchorTransform = null;
    private CharacterController Controller;
    //回転
    public GameObject objectToRotate;
    public float rotationDuration = 1f;
    private Quaternion endRotation;
    private bool Rotated = false;

    // 移動に関するパラメータ
    private Vector3 MoveThrottle = Vector3.zero;
    public float MoveScale = 3.0f;
    private float MoveScaleMultiplier = 1.0f;
    private float SimulationRate = 60f;
    private float Acceleration = 0.1f;
    private float Damping = 0.3f;

    // コントローラーの速度と加速度を保存する変数
    private Vector3 touchVelocityL;
    private Vector3 touchVelocityR;
    private Vector3 touchAccelerationL;
    private Vector3 touchAccelerationR;
    private bool motionInertia = false;
    private bool isInitialized = false;
    private float motionInertiaDuration = 1.0f;

    // 歩行と走行のしきい値
    const float WALK_THRESHOLD = 0.8f;
    const float RUN_THRESHOLD = 1.3f;

    [SerializeField]
    private StatusData statusData;

    private Quaternion rot;
    private Vector3 pos;
    public Vector3 moveDirection = Vector3.zero;



    private void Start()
    {
        // CharacterControllerコンポーネントを取得
        Controller = GetComponent<CharacterController>();
        if (statusData.LR.Count > 0)
        {
            InitializeFromStatusData();
        }
        Debug.Log("PMpos.x" + pos.x);
        Debug.Log("PMpos.z" + pos.z);
    }
    private void InitializeFromStatusData()
    {
        if (statusData == null) return;

        Controller.enabled = false;
        transform.position = statusData.Position;

        // 回転の適用
        rot = Quaternion.Euler(0, statusData.rotY, 0);
        transform.rotation = rot;

        Controller.enabled = true;
        isInitialized = true;
    }

    private void Update()
    {

        // 手を振る動作による移動制御
        HandShakeController();
        // CharacterControllerの更新
        UpdateController();
        Debug.Log(Controller.transform.position);
        statusData.Position = Controller.transform.position;
        
        // デバッグ用のログ出力
        /*Debug.Log("L-touch velocity: " + touchVelocityL);
        Debug.Log("R-touch velocity: " + touchVelocityR);
        Debug.Log("L-touch acceleration: " + touchAccelerationL);
        Debug.Log("R-touch acceleration: " + touchAccelerationR);
        Debug.Log("MoveThrottle: " + MoveThrottle);*/
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

        MoveScale = 3.0f;

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
        //Debug.Log(isWalk);
        if (isWalk)
        {
            if (!motionInertia)
            {
                StartCoroutine(SetMotionInertia());
            }

            // ワールド座標のx軸方向にのみ移動するように設定
            //rightはX正 leftがX負 forwardがZ軸正 backZ負方向
            //idが奇数でtrueなら左に，偶数でtrueなら右に


            switch (statusData.LR.Count)
            {
                case 0: // 一問も解いていない

                    tmpMoveThrottle += Vector3.right * MoveScale;
                    break;

                case 1: // 1問目解いた後
                    //まずは直近のクイズ結果でtrueなら右、falseなら左にカメラを回転
                    RotateCoroutine(statusData.LR[0] ? "L" : "R"); 
                    if (statusData.LR[0])//右なら
                    {
                        tmpMoveThrottle += Vector3.forward * MoveScale; //z軸-に進む
                    }
                    else//左なら
                    {
                        tmpMoveThrottle += Vector3.back * MoveScale; //z軸＋に進む
                    }

                    break;

                case 2: // 2問目解いた後
                    RotateCoroutine(statusData.LR[1] ? "R" : "L");
                    if (statusData.LR[0])//右
                    {
                        if (statusData.LR[1])//右右
                        {
                            tmpMoveThrottle += Vector3.left * MoveScale;
                        }
                        else//右左
                        {
                            tmpMoveThrottle += Vector3.right * MoveScale;
                        }
                    }
                    else
                    {
                        if (statusData.LR[1])//左右
                        {
                            tmpMoveThrottle += Vector3.right * MoveScale;
                        }
                        else//左左
                        {
                            tmpMoveThrottle += Vector3.left * MoveScale;
                        }
                    }
                    
                    break;

                case 3: // 3問目解いた後
                    RotateCoroutine(statusData.LR[2] ? "R" : "L");
                    if (statusData.LR[0])//右
                    {
                        if (statusData.LR[1])//右右
                        {
                            if (statusData.LR[2])//右右右
                            {
                                tmpMoveThrottle += Vector3.left * MoveScale;
                            }
                            else//右右左
                            {
                                tmpMoveThrottle += Vector3.back * MoveScale;
                            }
                        }
                        else//右左
                        {
                            if (statusData.LR[2])//右左右
                            {
                                tmpMoveThrottle += Vector3.right * MoveScale;
                            }
                            else//右左左
                            {
                                tmpMoveThrottle += Vector3.forward * MoveScale;
                            }
                        }
                    }
                    else//左
                    {
                        if (statusData.LR[1])//左右
                        {
                            if (statusData.LR[2])//左右右
                            {
                                tmpMoveThrottle += Vector3.right * MoveScale;
                            }
                            else
                            {
                                tmpMoveThrottle += Vector3.forward * MoveScale;
                            }
                        }
                        else//左左
                        {
                            if (statusData.LR[2])//左左右
                            {
                                tmpMoveThrottle += Vector3.left * MoveScale;
                            }
                            else//左左左
                            {
                                tmpMoveThrottle += Vector3.back * MoveScale;
                            }
                        }
                    }
                    break;
               }                
            // 走行状態かどうかを判定
            bool isRun = DetectHandShakeRun(Math.Abs(handShakeVel.y));
            if (isRun)
                tmpMoveThrottle *= 2.0f;
            //Debug.Log("HandShake Move Effect: " + tmpMoveThrottle);
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
        //if (!IsGrounded()) return false;
        // 速度がしきい値を超えている場合は走行状態
        if (speed > RUN_THRESHOLD) return true;
        return false;
    }

    private bool IsGrounded()
    {
        return true;
    }

    private void UpdateController()
    {
        // 移動量の減衰
        float motorDamp = 2.0f + (Damping * SimulationRate * Time.deltaTime);

        MoveThrottle.x /= motorDamp;
        MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
        MoveThrottle.z /= motorDamp;

        // 移動方向の計算
        moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;
        // 移動予測の計算
        Vector3 predictedXZ = Vector3.Scale(Controller.transform.localPosition + moveDirection, new Vector3(1, 0, 1));

        // CharacterControllerの移動
        Controller.Move(moveDirection);

        // 実際の移動量の計算
        Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));

        // 予測移動量と実際の移動量が異なる場合は、MoveThrottleを調整
        if (predictedXZ != actualXZ)
            MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);

        /*Debug.Log($"MoveThrottle: {MoveThrottle}, MoveDirection: {moveDirection}, Position: {transform.position}");*/
        statusData.Distance += 1;
        // moveDirectionをリセットして再度計算する
        moveDirection =  Vector3.zero;
        
    }

    IEnumerator RotateCoroutine(String LorR)
    {
        Quaternion startRotation = objectToRotate.transform.rotation;
        rot.y = statusData.rotY;
        transform.rotation = rot;

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
}