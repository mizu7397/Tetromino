using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetromino : MonoBehaviour
{
    // --- タイマー関連 ---
    // ゲームが開始されてから経過した時間（前回の落下からの蓄積時間）
    private float fallCounter;
    // 落下速度（何秒で1マス落ちるか）
    public float fallSpeed = 1.0f;

    // --- 参照関連 ---
    // 回転の中心（インスペクターで各プレハブごとに設定）
    public Vector3 rotationPoint;
    // GridManagerへの参照（BlockSpawnerから自動設定される想定）
    [HideInInspector] public GridManager gridManager;

    // このテトリミノが既に固定処理に入っているかを管理するフラグ
    private bool isLocked = false;

    // --- Unity Messages ---

    void Start()
    {
        // GridManagerの参照がなければ検索して設定（保険）
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        // 初期位置が有効かチェック
        if (!IsValidGridPos())
        {
            Debug.Log("ゲームオーバー：初期位置で衝突");
            // ゲームオーバー処理をここに呼ぶ（例: GameManager.Instance.GameOver();）
            Destroy(gameObject); // 不正な位置ならテトロミノを削除
        }
    }

    void Update()
    {
        // 既に固定されているか、GridManagerが存在しなければ操作を受け付けない
        if (isLocked || gridManager == null) return;

        HandleInput();
        HandleFalling();
    }

    // --- 入力処理 ---
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(new Vector3(-1, 0, 0)); // 左に移動
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(new Vector3(1, 0, 0)); // 右に移動
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TrySoftDrop(); // 下にソフトドロップ
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(); // 回転
        }
        // 長押しの場合はGetKeyを使用
        else if (Input.GetKey(KeyCode.Space))
        {
            HardDrop(); // ハードドロップ
        }
    }

    // --- 落下処理 ---
    void HandleFalling()
    {
        // 時間経過を加算
        fallCounter += Time.deltaTime;

        // 落下時間を超えたら
        if (fallCounter >= fallSpeed)
        {
            FallOneStep();
        }
    }

    // 1マス自然落下させる
    private void FallOneStep()
    {
        // カウンターをリセット
        fallCounter = 0;

        // 下へ移動を試みる
        transform.position += new Vector3(0, -1, 0);

        // 移動した結果、無効な位置なら
        if (!IsValidGridPos())
        {
            // 位置を戻す
            transform.position -= new Vector3(0, -1, 0);
            // 着地とみなして固定する
            LockTetromino();
        }
    }

    // ソフトドロップ（下キー）の処理
    private void TrySoftDrop()
    {
        // 下へ移動を試みる
        transform.position += new Vector3(0, -1, 0);

        // 移動した結果、無効な位置なら
        if (!IsValidGridPos())
        {
            // 位置を戻す
            transform.position -= new Vector3(0, -1, 0);
            // ソフトドロップの場合は、すぐに固定せず、次の自然落下タイミングまで待つか、
            // 地面に着き続けるならLockさせるかは仕様によります。
            // ここではシンプルに何もしない（あるいは接地したままにする）でおきます。
            // ガイドラインに厳密に従うなら、接地時は即Lockする場合もあります。
        }
        else
        {
            // 移動成功したので、自然落下タイマーをリセット（連続落下を防ぐため）
            fallCounter = 0;
        }
    }

    // --- 移動・回転ロジック ---

    // 一般的な移動処理（衝突したら戻す）
    void Move(Vector3 direction)
    {
        // 仮に移動してみる
        transform.position += direction;

        // 移動した結果、有効な位置ならOK
        if (IsValidGridPos())
        {
            return; // 移動確定
        }

        // 無効な位置なら、移動をキャンセル
        transform.position -= direction;
    }

    // 回転処理
    void Rotate()
    {
        // 相対座標をワールド座標に変換して回転軸を取得
        Vector3 pivot = transform.TransformPoint(rotationPoint);
        // Z軸（2D画面）を中心に90度回転
        transform.RotateAround(pivot, Vector3.forward, 90);

        // 回転した結果、無効な位置なら
        if (!IsValidGridPos())
        {
            // 逆回転させて元に戻す
            transform.RotateAround(pivot, Vector3.forward, -90);
        }
        // ※ 壁蹴り（Wall Kick）を実装する場合はここに複雑なロジックが入ります。
    }

    // ハードドロップ処理
    void HardDrop()
    {
        // 一気に最下段まで下げる
        // 無限ループ防止のため、最大20回（高さ分）ループさせる
        for (int i = 0; i < 20; i++)
        {
            transform.position += new Vector3(0, -1, 0);

            // 衝突した瞬間にループを抜ける
            if (!IsValidGridPos())
            {
                transform.position -= new Vector3(0, -1, 0); // 衝突したので一歩戻る
                break;
            }
        }
        // 即座に固定
        LockTetromino();
    }

    // --- 固定処理 ---

    void LockTetromino()
    {
        if (isLocked) return; // 二重実行防止
        isLocked = true;

        // GridManagerに着地を通知（自身のTransform情報を渡す）
        gridManager.AddTetrominoToGrid(this);

        // ラインが揃っているかチェック
        gridManager.CheckForFullLines();

        // このテトロミノのUpdate処理を無効化
        this.enabled = false;

        // BlockSpawnerに次のテトロミノ生成を依頼
        // ※ FindObjectOfTypeは遅いので、気になる場合はBlockSpawnerのインスタンスをシングトンにしてください
        FindObjectOfType<BlockSpawner>().SpawnNextTetromino();
    }

    // --- 衝突判定 ---

    // 現在の位置が有効か判定する
    bool IsValidGridPos()
    {
        // このテトロミノの子ブロック（個々のCube）をすべてチェック
        foreach (Transform child in transform)
        {
            // 子オブジェクトのワールド座標
            Vector3 blockPos = child.position;

            // グリッド座標に丸める（Int型へ変換）
            // NOTE: テトロミノのPivot（中心）が整数座標にある前提です。
            // 子ブロックのlocalPositionが(0.5, 0.5)などになっている場合は注意が必要です。
            // 通常、テトロミノの子Cubeは(-0.5, 0.5), (0.5, 0.5)などの相対位置に配置します。
            // 親のpositionが整数であれば、RoundToIntで正しくグリッド中心に合います。
            int x = Mathf.RoundToInt(blockPos.x);
            int y = Mathf.RoundToInt(blockPos.y);

            // 1. 境界チェック (GridManagerのサイズを使用)
            if (x < 0 || x >= gridManager.width || y < 0)
            {
                // y >= height のチェックは天井抜けを許容するなら必須ではありませんが、
                // 通常は y < gridManager.height も入れます。
                return false; // ボード外（左右と床）
            }

            // 天井チェックを入れる場合
            if (y >= gridManager.height)
            {
                // まだ完全に画面内に入りきっていないスポーン時はtrueを返す等の工夫が必要ですが、
                // シンプルにやるならfalseです。
                // 今回はStart時の初期位置チェックのために厳密にfalseにします。
                return false;
            }

            // 2. 既に他のブロックが存在するかチェック（GridManagerの配列を確認）
            // 配列外参照を防ぐため、上記の境界チェックの後に行うこと。
            if (gridManager.grid[x, y] != null)
            {
                // 既に埋まっているセルと重なっている
                // ただし、自身の子ブロックとの衝突は無視する必要があります。
                // (現在の実装では子ブロックを移動させてから判定しているので、
                //  自身の子ブロックもgridManagerに登録済みだと誤検知します。
                //  通常、固定処理AddTetrominoToGridの直後にSetParent(null)するか、
                //  リストで管理する手法をとりますが、
                //  このコードのフロー（Moveしてダメなら戻す）であれば、
                //  まだgridManagerに登録されていない子ブロックとは衝突しません)
                return false; // 他のブロックとの衝突
            }
        }
        return true; // 全てのチェックをパスした＝有効な位置
    }

    // --- エディタでの可視化用（Gizmos） ---
    // シーンビューで回転中心を確認しやすくする
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // ローカル座標rotationPointをワールド座標に変換して描画
        Gizmos.DrawSphere(transform.TransformPoint(rotationPoint), 0.2f);
    }
}