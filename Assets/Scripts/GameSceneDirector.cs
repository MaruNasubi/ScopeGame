using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameSceneDirector : MonoBehaviour
{
    public bool debug = true;
    float debtimer;

    // ゲーム設定
    public const int TILE_X = 7;
    public const int TILE_Y = 7;
    const int PLAYER_MAX = 2;

    // タイルのプレハブ
    public GameObject[] prefabTile;

    // カーソルのプレハブ
    public GameObject prefabCursor;

    // 内部データ
    GameObject[,] tiles;
    UnitController[,] units;

    // ユニットのプレハブ（プレイヤーごと）
    public List<GameObject> prefabWhiteUnits;
    public List<GameObject> prefabBlackUnits;

    // 1 = 海賊 2 = 侍 3 = 忍者 4 = 犬 5 = 宝箱
    public int[,] unitType =
    {
        { 1, 0, 0, 0, 0, 0, 15 },
        { 3, 0, 0, 0, 0, 0, 13 },
        { 4, 0, 0, 0, 0, 0, 14 },
        { 2, 0, 0, 0, 0, 0, 12 },
        { 4, 0, 0, 0, 0, 0, 14 },
        { 3, 0, 0, 0, 0, 0, 13 },
        { 5, 0, 0, 0, 0, 0, 11 },
    };



    // UI関連
    GameObject txtTurnInfo;
    GameObject txtResultInfo;
    GameObject btnApply;
    GameObject btnCancel;

    // 選択ユニット
    public UnitController selectUnit;

    // 移動関連
    List<Vector2Int> movableTiles;
    List<GameObject> cursors;

    // モード
    enum MODE
    {
        NONE,
        CHECK_MATE,
        NORMAL,
        STATUS_UPDATE,
        TURN_CHANGE,
        RESULT
    }

    MODE nowMode, nextMode;
    int nowPlayer;

    // 前回ユニット削除から経過ターン
    int prevDestroyTurn;

    // 前回の盤面
    List<UnitController[,]> prevUnits;


    // Start is called before the first frame update
    void Start()
    {
        // UIオブジェクト取得
        txtTurnInfo = GameObject.Find("TextTurnInfo");
        txtResultInfo = GameObject.Find("TextResultInfo");
        btnApply = GameObject.Find("ButtonApply");
        btnCancel = GameObject.Find("ButtonCancel");

        // リザルトは非表示
        btnApply.SetActive(false);
        btnCancel.SetActive(false);

        // 内部データの初期化
        tiles = new GameObject[TILE_X, TILE_Y];
        units = new UnitController[TILE_X, TILE_Y];
        cursors = new List<GameObject>();
        prevUnits = new List<UnitController[,]>();

        for (int i = 0; i < TILE_X; i++)
        {
            for (int j = 0; j < TILE_Y; j++)
            {
                // タイルとユニットのポジション
                float x = i - TILE_X / 2;
                float y = j - TILE_Y / 2;

                Vector3 pos = new Vector3(x, 0, y);

                // 作成
                int idx = (i + j) % 2;
                GameObject tile = Instantiate(prefabTile[idx], pos, Quaternion.identity);

                tiles[i, j] = tile;

                // ユニットの作成
                int type = unitType[i, j] % 10;
                int player = unitType[i, j] / 10;

                GameObject prefab = getPrefabUnit(player, type);
                GameObject unit = null;
                UnitController ctrl = null;

                if (null == prefab) continue;

                pos.y += 1.5f;
                unit = Instantiate(prefab);

                // 初期化処理
                ctrl = unit.GetComponent<UnitController>();
                ctrl.SetUnit(player, (UnitController.TYPE)type, tile);

                // 内部データセット
                units[i, j] = ctrl;
            }
        }

        nowPlayer = -1;
        nowMode = MODE.NONE;
        nextMode = MODE.TURN_CHANGE;
    }

    // Update is called once per frame
    void Update()
    {
        if(MODE.CHECK_MATE == nowMode)
        {
            checkMateMode();
        }
        else if(MODE.NORMAL == nowMode)
        {
            normalMode();
        }
        else if(MODE.STATUS_UPDATE == nowMode)
        {
            statusUpdateMode();
        }
        else if(MODE.TURN_CHANGE == nowMode)
        {
            turnChangeMode();
        }
        else if(MODE.RESULT == nowMode)
        {
            if (debug)
            {
                debtimer += Time.deltaTime;
                if(5 < debtimer)
                {
                    Retry();
                }
            }
        }

        // モード変更
        if(MODE.NONE != nextMode)
        {
            nowMode = nextMode;
            nextMode = MODE.NONE;
        }



    }

    // チェックメイトモード
    void checkMateMode()
    {
        // 次のモード
        nextMode = MODE.NORMAL;
        Text info = txtResultInfo.GetComponent<Text>();
        info.text = "";

        // 宝箱のチェックのチェック
        UnitController target = getUnit(nowPlayer, UnitController.TYPE.CHEST);
        // チェックしているユニット
        List<UnitController> checkunits = GetCheckUnits(units, nowPlayer);
        // チェック状態セット
        bool ischeck = (0 < checkunits.Count) ? true : false;

        if( null != target)
        {
            target.SetCheckStatus(ischeck);
        }

        // 移動可能範囲を調べる
        int tilecount = 0;

        // 移動可能範囲をカウント
        foreach(var v in getUnits(nowPlayer))
        {
            tilecount += getMovableTiles(v).Count;
        }

        // 動かせない
        if( 1 > tilecount )
        {
            info.text = "引き分け";

            if (ischeck)
            {
                info.text = (getNextPlayer() + 1) + "Pの勝ち！！";
            }

            nextMode = MODE.RESULT;
        }

        // 今回の盤面をコピー
        UnitController[,] copyunits = GetCopyArray(units);
        prevUnits.Add(copyunits);

        // 次のモードの準備
        if(MODE.RESULT == nextMode)
        {
            btnApply.SetActive(true);
            btnCancel.SetActive(true);
        }
    }

    // ノーマルモード
    void normalMode()
    {
        GameObject tile = null;
        UnitController unit = null;

        // プレイヤーの処理
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // ヒットした全てのオブジェクト情報を取得
            foreach (RaycastHit hit in Physics.RaycastAll(ray))
            {
                if (hit.transform.name.Contains("Tile"))
                {
                    tile = hit.transform.gameObject;
                    break;
                }
            }
        }

        // CPUの処理
        while( TitleSceneDirector.PlayerCount <= nowPlayer
                && (null == selectUnit || null == tile ) )
        {
            // ユニット選択
            if( null == selectUnit)
            {
                // 今回の全ユニット
                List<UnitController> tmpunits = getUnits(nowPlayer);
                // ランダムで1体選ぶ
                UnitController tmp = tmpunits[Random.Range(0, tmpunits.Count)];
                // ユニットがいるタイルを選択
                tile = tiles[tmp.Pos.x, tmp.Pos.y];

                // 一旦処理へ流す
                break;
            }

            // ここから下はselectUnitが入った状態でくる
            if( 1 > movableTiles.Count)
            {
                setSelectCursors();
                break;
            }

            // 移動可能範囲があればランダムで移動
            int rnd = Random.Range(0, movableTiles.Count);
            tile = tiles[movableTiles[rnd].x, movableTiles[rnd].y];
        }

        // タイルが押されていなければ処理しない
        if (null == tile) return;

        // 選んだタイルからユニット取得
        Vector2Int tilepos = new Vector2Int(
            (int)tile.transform.position.x + TILE_X / 2,
            (int)tile.transform.position.z + TILE_Y / 2);

        // ユニット
        unit = units[tilepos.x, tilepos.y];

        // ユニット選択
        if (null != unit
            && selectUnit != unit
            && nowPlayer == unit.Player )
        {
            // 移動可能範囲を取得
            List<Vector2Int> tiles = getMovableTiles(unit);

            // 選択不可
            if (1 > tiles.Count) return;

            movableTiles = tiles;
            setSelectCursors(unit);
        }
        // 移動
        else if (null != selectUnit && movableTiles.Contains(tilepos))
        {
            moveUnit(selectUnit, tilepos);
            nextMode = MODE.STATUS_UPDATE;
        }
        // 移動範囲だけ見られる
        else if( null != unit && nowPlayer != unit.Player)
        {
            setSelectCursors(unit, false);
        }
        // 選択解除
        else
        {
            setSelectCursors();
        }
    }

    // 移動後の処理
    void statusUpdateMode()
    {
        // ターン経過
        foreach (var v in getUnits(nowPlayer))
        {
            v.ProgressTurn();
        }

        // カーソル
        setSelectCursors();

        nextMode = MODE.TURN_CHANGE;
    }

    // ターン変更
    void turnChangeMode()
    {
        // ターンの処理
        nowPlayer = getNextPlayer();

        // Infoの更新
        txtTurnInfo.GetComponent<Text>().text = "" + (nowPlayer + 1) + "Pの番です";

        // 経過ターン（１P側にきたら+1）
        if( 0 == nowPlayer)
        {
            prevDestroyTurn++;
        }

        nextMode = MODE.CHECK_MATE;
    }

    int getNextPlayer()
    {
        int next = nowPlayer + 1;
        if (PLAYER_MAX <= next) next = 0;

        return next;
    }

    // 指定のユニットを取得する
    UnitController getUnit(int player, UnitController.TYPE type)
    {
        foreach (var v in getUnits(player))
        {
            if (player != v.Player) continue;
            if(type == v.Type ) return v;
        }
        return null;
    }

    // 指定されたプレイヤー番号のユニットを取得する
    List<UnitController> getUnits(int player = -1)
    {
        List<UnitController> ret = new List<UnitController>();

        foreach (var v in units)
        {
            if (null == v) continue;

            if(player == v.Player)
            {
                ret.Add(v);
            }
            else if( 0 > player)
            {
                ret.Add(v);
            }
        }
        return ret;
    }

    // 指定された配列をコピーして返す
    public static UnitController[,] GetCopyArray(UnitController[,] org)
    {
        UnitController[,] ret = new UnitController[org.GetLength(0), org.GetLength(1)];
        Array.Copy(org, ret, org.Length);
        return ret;
    }

    // 移動可能範囲取得
    List<Vector2Int> getMovableTiles(UnitController unit)
    {
        // そこをどいたらチェックされてしまうか
        UnitController[,] copyunits = GetCopyArray(units);
        copyunits[unit.Pos.x, unit.Pos.y] = null;

        // チェックされるかどうか
        List<UnitController> checkunits = GetCheckUnits(copyunits, unit.Player);

        // チェックを回避できるタイルを返す
        if( 0 < checkunits.Count)
        {
            // 移動可能範囲
            List<Vector2Int> ret = new List<Vector2Int>();

            // 移動可能範囲
            List<Vector2Int> movetiles = unit.GetMovableTiles(units);

            // 移動してみる
            foreach(var v in movetiles)
            {
                // 移動した状態を作る
                UnitController[,] copyunits2 = GetCopyArray(units);
                copyunits2[unit.Pos.x, unit.Pos.y] = null;
                copyunits2[v.x, v.y] = unit;

                int checkcount = GetCheckUnits(copyunits2, unit.Player, false).Count;

                if (1 > checkcount) ret.Add(v);
            }
            return ret;
        }

        // 通常移動可能範囲を返す
        return unit.GetMovableTiles(units);
    }

    // 選択時の関数
    void setSelectCursors(UnitController unit=null, bool setunit = true)
    {
        // カーソル解除
        foreach (var v in cursors)
        {
            Destroy(v);
        }

        // 選択ユニットの非選択状態
        if( null != selectUnit)
        {
            selectUnit.SelectUnit(false);
            selectUnit = null;
        }

        // なにもセットされないなら終了
        if (null == unit) return;

        // カーソル作成
        foreach(var v in getMovableTiles(unit))
        {
            Vector3 pos = tiles[v.x, v.y].transform.position;
            pos.y += 0.51f;

            GameObject obj = Instantiate(prefabCursor, pos, Quaternion.identity);
            cursors.Add(obj);
        }

        // 選択状態
        if(setunit)
        {
            selectUnit = unit;
            selectUnit.SelectUnit();
        }
    }

    // ユニット移動
    void moveUnit(UnitController unit, Vector2Int tilepos)
    {
        // 現在地
        Vector2Int unitpos = unit.Pos;

        // 誰かいたら消す
        if(null != units[tilepos.x, tilepos.y])
        {
            Destroy(units[tilepos.x, tilepos.y].gameObject);
            prevDestroyTurn = 0;
        }

        // 新しい場所へ移動
        unit.MoveUnit(tiles[tilepos.x, tilepos.y]);

        // 内部データ更新（元の場所）
        units[unitpos.x, unitpos.y] = null;

        // 内部データ更新（新しい場所）
        units[tilepos.x, tilepos.y] = unit;
    }

    // ユニットのプレハブを取得
    GameObject getPrefabUnit(int player, int type)
    {
        int idx = type - 1;

        if (0 > idx) return null;//パーティクルを再生（追加）
        //GetComponent<ParticleSystem>().Play();

        GameObject prefab = prefabWhiteUnits[idx];
        if( 1 == player ) prefab = prefabBlackUnits[idx];

        return prefab;
    }

    // 指定された配置でチェックされているかチェック
    static public List<UnitController> GetCheckUnits(UnitController[,] units, int player, bool checkking = true)
    {
        List<UnitController> ret = new List<UnitController>();

        foreach (var v in units)
        {
            if (null == v) continue;
            if (player == v.Player) continue;

            // 敵1体の移動可能範囲
            List<Vector2Int> enemytiles = v.GetMovableTiles(units, checkking);

            foreach (var t in enemytiles)
            {
                if (null == units[t.x, t.y]) continue;

                if(UnitController.TYPE.CHEST == units[t.x, t.y].Type)
                {
                    ret.Add(v);
                }
            }
        }

        return ret;
    }

    public void Retry()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Title()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
