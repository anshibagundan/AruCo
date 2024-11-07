## AruCo
<img width="1706" alt="スクリーンショット 2024-11-07 10 01 51" src="https://github.com/user-attachments/assets/2a824264-777d-4f5f-94fa-8214df5b6534">

技育博202407 企業賞
技育展2024 決勝出場

## 背景・作品概要

私たちが初め,どのような作品を作成するか考えたとき,VRを使用して社会問題に対し何か取り組めないかと考えた.そこで社会的問題である認知症に対し,認知症予防に効果的といわれている運動とゲーム要素である記憶力向上を同時に行えるものを作成することにした.
今回作成したAruCoというアプリケーションは,「運動をしながら記憶力を鍛えることで認知症を改善または予防できないか」というテーマを持つVRとスマートフォンを使用する記憶力向上クイズゲームだ.認知症または認知症を予防したい人とその補助を行う人の二人で操作する.

## 操作方法

VR側はVRゴーグルを被りコントローラをもってゲーム操作を行う.スマートフォン側はスマートフォンで,専用のアプリケーションを用いてVRゲームを設定を操作する.

VR側が行えることは以下の二つである. 
- コントローラーを持ち腕を振ることで前進させること- 途中と最後のクイズにはコントローラーのボタンを押して解答する

スマートフォン側が行えることは以下の五つである.
- ゲームの開始- クイズの難易度設定- map移動位置の確認- クイズ結果の確認- 今までの結果のリスト
VR側で行えることが少ないのは,VR側を操作する対象が認知症患者の可能性があり複雑な操作を行うことができない,または負担になる可能性が高かったためである.

## 具体的な実装内容

VR部分はMeta quest3とUnityを用いた.運動の要素をである腕振り前進について,コントローラを持って腕振りすることによって前進するという実装内容のために, *XR Interaction Toolkit* というSDKを用いた. XR Origin(XR Rig)オブジェクトを使用し, `Player Motion.cs` スクリプトにおいてコントローラーの速度と加速度を取得しその値から移動量を設定するコードを記述しXR Originにスクリプトをアタッチして動かすことで実装した.また,Unityのシーン内に配置した建物などのオブジェクトはBlenderで作成したものとUnityのAsset storeからダウンロードしたものを併用して作成した。大きいオブジェクトである道などはAsset Storeから細部にはBlenderで作成したオブジェクトを使用した.画面のシーン切り替えについてもUnity上で行っており,一定位置へ移動するとシーン切り替えが行われる。シーンは合計4つ存在し,タイトルを表示するタイトルシーン,歩いているシーンであるウォーキングシーン,クイズを表示するクイズシーン,ゴール後の最後クイズを表示するシーンに分かれている.

スマートフォンアプリ実装にはandroid Stadioを使用し,データの送信やデータの保存をローカルサーバーを通じて行い,ゲーム実行中に行っているmap位置の提示はdjangochannelsを用いて非同期通信で実装した.各シーンに配置されるクイズの通信や難易度選択などもローカルサーバーを用いて実装した.

腕振りによる前進機能を実装するために作成したPlayer Motion.csについて解説とその実装方法について解説する.ただし、腕振りによる前進機能に関係のある部分のみを抜き出している。XR Interaction ToolkitのInput Action ManagerとXR interaction Managerはオブジェクトまたはコンポーネントで追加済み前提で設定するシーンによってはLocomotion Systemが必要の場合がある.
出来るだけ詳細にコードにコメントを記述した.簡単にまとめると,コントローラの速度加速度を取得してきて,速度の大きい方のコントローラの速度を採択して計算処理を行い,速度加速度から得られた値がwalkまたはrunの閾値を超えた場合,character Controllerコンポーネントでキャラクターを動作させている.今回はゲームの仕様上,どの方向にコントローラーを振っても一方向にしか移動できないように値を書き換えている.
まず各種変数設定とメインのStart()とUpdate()を記す. Start()ではCharacter Controllerコンポーネントを取得し、Update()ではHandShakeController()メソッドとUpdateController()メソッドを更新し続ける.

```
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

    // 移動に関するパラメータ
    private Vector3 MoveThrottle = Vector3.zero;//移動速度を保持するベクトル
    private float MoveScale = 1.0f;//移動スケール
    private float MoveScaleMultiplier = 1.0f;//上の倍率
    private float SimulationRate = 60f;//シュミレーションの更新レート
    private float FallSpeed = 0.0f;//落下速度
    private float Acceleration = 0.1f;//加速度
    private float Damping = 0.3f;//減衰係数
    private float GravityModifier = 0.379f;//重力修正用

    // コントローラーの速度と加速度を保存する変数/
    private Vector3 touchVelocityL;//左コントローラーの速度
    private Vector3 touchVelocityR;//右コントローラーの速度
    private Vector3 touchAccelerationL;//左コントローラーの加速度
    private Vector3 touchAccelerationR;//右コントローラーの加速度
    private bool motionInertia = false;//モーション慣性の状態 有効true/無効false
    private float motionInertiaDuration = 1.0f;//モーション慣性の持続時間

    // 歩行と走行のしきい値
    const float WALK_THRESHOLD = 0.8f;//歩行の閾値
    const float RUN_THRESHOLD = 1.3f;//走行の閾値
    public float moveScale = 0.3f;//移動スケール

    private void Start()
    {
        // 同じオブジェクトにアタッチされているCharacterControllerコンポーネントを取得
        Controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 手を振る動作による移動制御
        HandShakeController();
        // CharacterControllerの更新
        UpdateController();
    }
```
Update()メソッドにおいて実行されるHandShakeControllerメソッドを解説する手を振る動作による移動制御をするメソッドである.また,このメソッド内の移動影響量、回転、コントローラの速度と加速度のベクトルを引数にして移動量と方向のベクトルを返すCalculateMoveEffectメソッドも示す. 二つのメソッドで使用されている地面と設置しているかを判別するIsGroundedメソッド,歩行状態かを判別するDetectHandShakeWalkメソッド,走行状態かを判別するメソッドDetectHandShakeRunメソッド,モーション慣性を有効にし、一定時間後に無効にするSetMotionInertiaメソッドは省略した.
```
//手を振る動作による移動制御をするメソッド　Update()内で動くメソッド
private void HandShakeController()
{
    InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);//左手
    InputDevice rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);//右手

    // コントローラーの速度と加速度を取得
    leftController.TryGetFeatureValue(CommonUsages.deviceVelocity, out touchVelocityL);//左速度
    rightController.TryGetFeatureValue(CommonUsages.deviceVelocity, out touchVelocityR);//右速度
            
    leftController.TryGetFeatureValue(CommonUsages.deviceAcceleration, out touchAccelerationL);//左加速度
    rightController.TryGetFeatureValue(CommonUsages.deviceAcceleration, out touchAccelerationR);//右加速度

    // 地面に接地しているかどうかで移動スケールを調整
    if (!IsGrounded()) MoveScale = 0.0f;
    else MoveScale = 1.0f;
    //移動スケールをシュミレーションレートとΔタイムで修正　Qiitaから
    MoveScale *= SimulationRate * Time.deltaTime;
    //移動影響力を計算　Qiitaから
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
    }else{
        activeHand = RightHandAnchorTransform;
        handShakeVel = touchVelocityR;
        handShakeAcc = touchAccelerationR;
    }

    // 選択した手の回転を取得し、z軸とx軸の回転をゼロに設定
    Quaternion ort = activeHand.rotation;//手の回転取得
    Vector3 ortEuler = ort.eulerAngles;
    ortEuler.z = ortEuler.x = 0f;
    ort = Quaternion.Euler(ortEuler);

    // 移動効果を計算し、MoveThrottleに加算
    MoveThrottle += CalculateMoveEffect(moveInfluence, ort, handShakeVel, handShakeAcc);
}

//HandShakeController()内で（というか↑）MoveThrottleの移動効果計算のために必要 
//移動影響量、回転、コントローラの速度と加速度のベクトルを引数にして移動量と方向のベクトルを返す　今回はx軸正方向に指定している(他のコードに変更してる)
private Vector3 CalculateMoveEffect(float moveInfluence, Quaternion ort, Vector3 handShakeVel, Vector3 handShakeAcc)
{
    //一時的な移動スロットルを初期化と返り値の設定
    Vector3 tmpMoveThrottle = Vector3.zero;

    // 歩行状態かどうかを判定　歩行状態かつモーション慣性が働いているか
    //bool isWalk = DetectHandShakeWalk(Math.Abs(handShakeVel.y)) || motionInertia;
    if (isWalk)
    {
        //モーション慣性が有効でない場合一定時間有効にする
        if (!motionInertia)
        SetMotionInertia();
        // ワールド座標のx軸方向にのみ移動するように設定し移動量を計算　
        tmpMoveThrottle += Vector3.right * moveScale;

        // 走行状態かどうかを判定
        bool isRun = DetectHandShakeRun(Math.Abs(handShakeVel.y));
        //run状態だったら
        if (isRun)
            tmpMoveThrottle *= 2.0f;
            Debug.Log("HandShake Move Effect: " + tmpMoveThrottle);
    }
    return tmpMoveThrottle;
}
```
CharacterControllerのアップデートを行うメソッドUpdateControllerを解説する.移動量の調整を行っている。また滑らかな移動を実装するためのメソッドでもある.
```
private void UpdateController()
{
    //移動方向の初期化
		//次のキャラクターのフレームでの移動先を示す
    Vector3 moveDirection = Vector3.zero;

    // 移動量の減衰 計算式はQiitaから
    //Dampingは減少係数 SimulationRateは更新レート Time.deltaTimeは前フレームからの経過時間
    float motorDamp = 2.0f + (Damping * SimulationRate * Time.deltaTime);

    //HandShakeController()で更新したMoveThrottleをmotorDampで減衰させる
		//y成分が正（上方向）の時のみ減衰させる
    MoveThrottle.x /= motorDamp;
    MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
    MoveThrottle.z /= motorDamp;
    
    // 上で変更したことを踏まえて移動方向の計算
    moveDirection += MoveThrottle * SimulationRate * Time.deltaTime;
    
   // 段差を乗り越える処理
    if (Controller.isGrounded && MoveThrottle.y <= transform.lossyScale.y * 0.001f)
    {
        float bumpUpOffset = Mathf.Max(Controller.stepOffset, new Vector3(moveDirection.x, 0, moveDirection.z).magnitude);
        moveDirection -= bumpUpOffset * Vector3.up;
    }
    
    // 移動予測の計算　この時y成分を無視してxz平面上の位置のみ計算
    Vector3 predictedXZ = Vector3.Scale(Controller.transform.localPosition + moveDirection, new Vector3(1, 0, 1));
    
    // CharacterControllerの移動
    Controller.Move(moveDirection);
    
    // 実際の移動量の計算　この時y成分を無視してxz平面上の位置のみ計算
    Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));
    
    // 予測移動量と実際の移動量が異なる場合は、MoveThrottleを調整
    //衝突などの急激な動きな変化を滑らかにする
    if (predictedXZ != actualXZ)
        MoveThrottle += (actualXZ - predictedXZ) / (SimulationRate * Time.deltaTime);
}
```

[UnityとQuest2でVR空間を移動する手振り歩行の実装 #C# - Qiita](https://qiita.com/takafumihoriuchi/items/0f4bf9e6ed060bc2efbe)
こちらのQiitaを参考にして移動速度などを実装している。

PlayermotionをXR Interaction ToolkitのXR Origin (XR Rig)にアタッチして,Left Hand Anchor TransformとRight Hand Anchor TransformにLeftHand ControllerとRightHand Controllerをそれぞれドラッグ＆ドロップすれば動作する.XR Origin(XR Rig)にCharacter Controllerコンポーネントを追加する必要があるので注意が必要である.

## 使用環境

### 言語/フレームワーク/SDK

Java,XML

Python (DjangoRestFramework,DjangoChannels)

C# 

Meta All in One SDK

### 開発環境

Unity 2022.3.5f1

android studio
