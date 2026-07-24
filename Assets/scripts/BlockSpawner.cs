using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject[] tetrominoPrefabs;//テトリミノのprefabの配列

    // Start is called before the first frame update
    void Start()
    {
        SpawnNextTetromino();
    }

    public void SpawnNextTetromino()
    {
        //まずprefabが設定されているかどうか確認
        if (tetrominoPrefabs == null || tetrominoPrefabs.Length == 0)
        {
            Debug.LogError("テトリミノのprefabがblockspawnerに設定されていません！");
            return;
        }

        int randomIndex = Random.Range(0, tetrominoPrefabs.Length);//0以上、配列の数未満の中から数をランダム
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0);
        GameObject newTetromino = Instantiate(tetrominoPrefabs[randomIndex], spawnPos
            , Quaternion.identity);


        //ここで生成したテトリミノにGridManagerへの参照を設定
        Tetromino newTetrominoScript = newTetromino.GetComponent<Tetromino>();
        if (newTetrominoScript != null)
        {
            newTetrominoScript.gridManager = FindObjectOfType<GridManager>();
        }
        else
        {
            Debug.LogError("生成されたテトリミノのprefabにtetrominoスクリプトがアタッチされていません");
        }
    }
}
