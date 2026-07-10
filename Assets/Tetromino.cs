using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UIElements;

public class Tetromino : MonoBehaviour
{
    //各テトリミノのブロックが占める相対位置
    public Vector4[] childBlockPositions;

    //ゲームが開始されてから経過した時間
    private float fallTime;

    //落下時間（秒）
    public float fallSpeed = 1.0f;

    //回転の中心
    public Vector3 rotationPoint;

    //GridManagerへの参照（インスペクターで設定）
    public GridManager gridManager;

    // Start is called before the first frame update
    void Start()
    {
        //GridManagerが設定されていない場合は、シーンから検索して取得
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        //初期位置が有効かチェック
        if (!IsValidGridPos())
        {
            Debug.Log("ゲームオーバー！");
            //GameManagerにゲームオーバーを通知する処理など追加
            Destroy(gameObject);//  不正な位置ならテトロミノを削除
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //キー入力による移動と回転
        HandleInput();

        //時間経過による落下
        HandleFalling();
    }
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(new Vector3(-1, 0, 0));//左に移動
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(new Vector3(1, 0, 0));//右に移動
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(new Vector3(0, -1, 0));//下にソフトドロップ
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate();//回転
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            HardDrop();//ハードドロップ
        }

    }
    void HandleFalling()
    {
        if (Time.time - fallTime >= fallSpeed)
        {
            Move(new Vector3(0, -1, 0), true);//下に移動(isFalling = true)
            fallTime = Time.time;
        }
    }

    void Move(Vector3 direction, bool isFalling = false)
    {
        //移動前の位置を保存
        transform.position += direction;

        if (IsValidGridPos())
        {
            //有効な位置であればグリッドマネージャーを更新(移動中のブロックのグリッドの位置は更新不要)
            //isFallingがtrueの場合のみ、グリッドマネージャーにブロックを固定する可能性があることを伝える
            if (isFalling)
            {
                gridManager.UpdateGrid(this);//グリッドマネージャーを更新
            }
        }
        else
        {
            //無効な位置であれば元の位置に戻す
            transform.position -= direction;

            if (isFalling)
            {
                //地面やほかのブロックに衝突した場合、テトリミノを固定する
                gridManager.AddTetrominoToGrid(this);
                gridManager.CheckForFullLines();//ライン消去チェック
                this.enabled = false;//このテトリミノの操作を停止
                FindObjectOfType<BlockSpawner>().SpawnNextTetromino();//次のテトリミノを生成
            }
        }
    }

    void Rotate()
    {
        //開店前の角度を保存
        Vector3 previousRotation = transform.eulerAngles;

        //回転の中心を考慮して回転
        transform.RotateAround(transform.TransformPoint(rotationPoint), Vector3.forward, 90);

        if (!IsValidGridPos())
        {
            //無効な位置であれば元の角度に戻す
            transform.eulerAngles = previousRotation;
        }
    }
    void HardDrop()
    {
        while (IsValidGridPos())
        {
            transform.position+= new Vector3(0,-1,0);
        }
        transform.position-= new Vector3(0,-1,0); //衝突したので一歩戻る

        //着地処理
        gridManager.AddTetrominoToGrid(this);
        gridManager.CheckForFullLines();
        this.enabled=false;
        FindObjectOfType<BlockSpawner>();
    }

    bool IsValidGridPos()
    {
        //このテトリミノの子ブロック（個々のCube）をすべてチェック
        foreach (Transform child in transform)
        {
            Vector3 blockPos = child.position;
            int x = Mathf.RoundToInt(blockPos.x);
            int y = Mathf.RoundToInt(blockPos.y);
            int z = Mathf.RoundToInt(blockPos.z);//3DテトリスなのでZ軸も考慮

            //協会チェック(GridManagerのサイズを使用)
            if (x<0 || x >=gridManager.width||
                y<0 || y >=gridManager.height ||
                z <0||z>=gridManager.depth)
            {
                return false; //ボード外
            }

            //既にほかのブロックが存在するかチェック
            //ただし。自身の子ブロックとの衝突は無視
            if (gridManager.grid[x,y,z] != null && gridManager.grid[x,y,z].parent!=transform)
            {
                return false; //ほかのブロックとの衝突
            }
        }
        return true; //有効な位置
    }
}
