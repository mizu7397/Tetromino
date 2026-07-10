using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    //グリッドのサイズ
    public int width = 10;
    public int height = 20;
    public int depth = 10;//3Dテトリスなので奥行きも

    //グリッドの状態を保持する3次元配列
    //各要素にはその位置に存在するブロックのTransformが格納される
    public Transform[,,] grid;

    // Start is called before the first frame update
    void awake()
    {
        //グリッド配列を初期化
        grid = new Transform[width, height,depth];
    }
    //テトリミノブロックをグリッドに固定する
    public void AddTetrominoToGrid(Tetromino tetromino)
    {
        foreach(Transform child in tetromino.transform)
        {
            int x = Mathf.RoundToInt(child.position.x);
            int y = Mathf.RoundToInt(child.position.y);
            int z = Mathf.RoundToInt(child.position.z);

            //グリッドの範囲内かチェック（念のため）
            if(x >=0 && x <width &&
               y >=0 && y <height &&
               z >=0 && z <depth)
                {
                grid[x,y,z] = child; //
                }
        }
    }
    //グリッドをクリアし指定されたテトリミノの位置を更新する（移動中のブロックはグリッドには登録しない）
    //このメソッドはテトリミノが移動するたびに呼び出されるのではなく、テトリミノが最終的に固定されたときにAddTetrominoToriを呼び出すのが一般的です
    //移動中はIsValidGridPosでチェックするのみで、girid配列を直接変更することはありません。
    public void UpdateGrid(Tetromino currentTetromino)
    {
        //実際にはこのメソッドはAddTetrominoTogridで固定する際に使用し移動中のテトリミノの位置はIsValidGridPosでチェックすることだけでいい場合が多いです。
        //ここでは概念としてグリッドが更新されることを示しています。（現時点ではこのメソッドはTetrominoスクリプトでは呼ばれていません
    }
    public void CheckForFullLines()
    {
        //3Dテトリスでは「ライン」ではなく「レイヤー（平面）」になることが多い
        //今回はZ軸方向に見て、X-Y平面の全てのブロックが埋まった場合を「ライン」とみなす
    }
    //指定されたY座標のレイヤーが全て埋まっているかチェック
    bool IsFullLayer(int y)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                if (grid[x, y, z] == null)
                {
                    return false; //一つでも空があれば埋まっていない
                }
            }
        }
            return true;
    }
    void DeleteLayer(int y)
    {
        for(int x = 0; x < width;x++)
        {
            for(int z = 0;x< depth;x++)
            {
                Destroy(grid[x, y, z].gameObject);　//ブロックオブジェクトを削除
                grid[x, y, z] = null; //グリッドからも削除
            }
        }
    }
    void MoveAllLayersDown(int starty)
    {
        for (int y = startY; y < height;y++)
        {
            for (int x = 0;x < width;x++)
            {
                for(int z = 0;z < depth;z++)
                {
                    if(grid[x, y, x] != null)
                        {
                        grid[x,y-1,z] = grid[x,y,x]; //一つ下のグリッドの移動
                        grid[x,y,z] = null; //  元の位置を空にする
                        grid[x,y-1,z].position += new Vector3(0,-1,0);//グロックの実際のGameObjectの位置も更新
                    }
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
