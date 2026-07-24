using UnityEngine;

public class GridManager : MonoBehaviour
{
    //グリッドのサイズ
    public int width = 10;
    public int height = 20;

    //グリッドの状態を保持する3次元配列
    //各要素にはその位置に存在するブロックのTransformが格納される
    public Transform[,] grid;

    // Start is called before the first frame update
    void Awake()
    {
        //グリッド配列を初期化
        grid = new Transform[width, height];
    }
    //テトリミノブロックをグリッドに固定する
    public void AddTetrominoToGrid(Tetromino tetromino)
    {
        foreach (Transform child in tetromino.transform)
        {
            int x = Mathf.RoundToInt(child.position.x);
            int y = Mathf.RoundToInt(child.position.y);

            //グリッドの範囲内かチェック（念のため）
            if (x >= 0 && x < width &&
               y >= 0 && y < height)
            {
                grid[x, y] = child; //
            }
        }
    }

    public void CheckForFullLines()
    {
        //3Dテトリスでは「ライン」ではなく「レイヤー（平面）」になることが多い
        //今回はZ軸方向に見て、X-Y平面の全てのブロックが埋まった場合を「ライン」とみなす
        for (int y = 0; y < height; y++)
        {
            if (IsFullLine(y))
            {
                DeleteLine(y);
                MoveAllRowsDown(y + 1);
                y--;
            }
        }
    }
    //指定されたY座標のレイヤーが全て埋まっているかチェック
    bool IsFullLine(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null) return false;
        }
        return true;
    }

    void DeleteLine(int y)
    {
        for (int x = 0; x < width; x++)
        {

            Destroy(grid[x, y].gameObject); //ブロックオブジェクトを削除
            grid[x, y] = null; //グリッドからも削除
        }
    }
    void MoveAllRowsDown(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                    if (grid[x, y] != null)
                    {
                        grid[x, y - 1] = grid[x, y]; //一つ下のグリッドの移動
                        grid[x, y] = null; //  元の位置を空にする
                        grid[x, y - 1].position += new Vector3(0, -1, 0);//グロックの実際のGameObjectの位置も更新
                    }
            }
        }
    }

}
