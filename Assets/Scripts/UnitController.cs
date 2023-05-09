using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    // プレイヤー番号
    public int Player;
    // 種類
    public TYPE Type;
    // 置いてからの経過ターン
    public int ProgressTurnCount;
    // 置いてる場所
    public Vector2Int Pos, OldPos;
    // 移動状態
    public List<STATUS> Status;

    // 1 = 海賊  2 = 侍 3 = 忍者 4 = 犬 5 = 宝箱
    public enum TYPE
    {
        NONE = -1,
        KAIZOKU = 1,
        SAMURAI,
        SHINOBI,
        INU,
        CHEST,
    }

    public enum STATUS
    {
        NONE=-1,
        CHECK,
    }
    
    // 必殺技ボタン
    public bool isUsually = true;
    public bool isSpecial = false;
    public bool isNinjutu = false;

    // 初期設定
    public void SetUnit(int player, TYPE type, GameObject tile)
    {
        Player = player;
        Type = type;
        MoveUnit(tile);
        ProgressTurnCount = -1; // 初期状態に戻す
    }


    // 移動可能範囲取得
    public List<Vector2Int> GetMovableTiles(UnitController[,] units, bool checkking = true)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        if( TYPE.CHEST == Type)
        {
            ret = getMovableTiles(units, TYPE.CHEST);
        }
        else
        {
            ret = getMovableTiles(units, Type);
        }
        
        return ret;
    }

    // 移動可能範囲を返す
    List<Vector2Int> getMovableTiles(UnitController[,] units, TYPE type)
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        // unitの種類 KAIZOKU
        if (TYPE.KAIZOKU == type)
        {
            if (isUsually == true)
            {
                int dir = 1;
                if (1 == Player) dir = -1;
                // 前後左右1コマずつ進む
                    List<Vector2Int> vec = new List<Vector2Int>()
                {
                    new Vector2Int(0 , 1 * dir ),
                    new Vector2Int(0 , -1 * dir ),
                    new Vector2Int(1 , 0 * dir ),
                    new Vector2Int(-1 , 0 * dir ),
                };

                // 前方
                foreach (var v in vec)
                {
                    Vector2Int checkpos = Pos + v;
                    if (!isCheckable(units, checkpos)) continue;
                    if (null != units[checkpos.x, checkpos.y]) break;

                    ret.Add(checkpos);
                }

                // 取る
                vec = new List<Vector2Int>()
                {
                    new Vector2Int(0 , 1 * dir ),
                    new Vector2Int(0 , -1 * dir ),
                    new Vector2Int(1 , 0 * dir ),
                    new Vector2Int(-1 , 0 * dir ),
                };

                foreach (var v in vec)
                {
                    Vector2Int checkpos = Pos + v;
                    if (!isCheckable(units, checkpos)) continue;

                    // なにもない
                    if (null == units[checkpos.x, checkpos.y]) continue;

                    // 味方のユニットを無視
                    if (Player == units[checkpos.x, checkpos.y].Player) continue;

                    // 追加
                    ret.Add(checkpos);
                }
            }
            else if (isSpecial == true)
            {
                int dir = 1;
                if (1 == Player) dir = -1;
                // 前後2コマずつ進む
                List<Vector2Int> vec = new List<Vector2Int>()
                {
                    new Vector2Int(0 , 2 * dir ),
                    new Vector2Int(0 , 0 * dir ),
                    new Vector2Int(2 , 0 * dir ),
                    new Vector2Int(0 , 0 * dir ),
                };

                // 前方
                foreach (var v in vec)
                {
                    Vector2Int checkpos = Pos + v;
                    if (!isCheckable(units, checkpos)) continue;
                    if (null != units[checkpos.x, checkpos.y]) break;

                    ret.Add(checkpos);
                }

                // 取る
                vec = new List<Vector2Int>()
                {
                    new Vector2Int(0 , 2 * dir ),
                    new Vector2Int(0 , -2 * dir ),
                    new Vector2Int(2 , 0 * dir ),
                    new Vector2Int(-2 , 0 * dir ),
                };

                foreach (var v in vec)
                {
                    Vector2Int checkpos = Pos + v;
                    if (!isCheckable(units, checkpos)) continue;

                    // なにもない
                    if (null == units[checkpos.x, checkpos.y]) continue;

                    // 味方のユニットを無視
                    if (Player == units[checkpos.x, checkpos.y].Player) continue;

                    // 追加
                    ret.Add(checkpos);
                }
            }
        }
        // unitの種類 SAMURAI
        if (TYPE.SAMURAI == type)
        {
            int dir = 1;
            if (1 == Player) dir = -1;
            // 前後左右1コマずつ進む
            List<Vector2Int> vec = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * dir ),
                new Vector2Int(0 , -1 * dir ),
                new Vector2Int(1 , 0 * dir ),
                new Vector2Int(-1 , 0 * dir ),
            };

            // 前方
            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;
                if (null != units[checkpos.x, checkpos.y]) break;

                ret.Add(checkpos);
            }

            // 取る
            vec = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * dir ),
                new Vector2Int(0 , -1 * dir ),
                new Vector2Int(1 , 0 * dir ),
                new Vector2Int(-1 , 0 * dir ),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;

                // なにもない
                if (null == units[checkpos.x, checkpos.y]) continue;

                // 味方のユニットを無視
                if (Player == units[checkpos.x, checkpos.y].Player) continue;

                // 追加
                ret.Add(checkpos);
            }
        }
        // unitの種類 SHINOBI
        if (TYPE.SHINOBI == type)
        {
            int dir = 1;
            if (1 == Player) dir = -1;
            // 前後左右1コマずつ進む
            List<Vector2Int> vec = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * dir ),
                new Vector2Int(0 , -1 * dir ),
                new Vector2Int(1 , 0 * dir ),
                new Vector2Int(-1 , 0 * dir ),
            };

            // 前方
            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;
                if (null != units[checkpos.x, checkpos.y]) break;

                ret.Add(checkpos);
            }

            // 取る
            vec = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * dir ),
                new Vector2Int(0 , -1 * dir ),
                new Vector2Int(1 , 0 * dir ),
                new Vector2Int(-1 , 0 * dir ),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;

                // なにもない
                if (null == units[checkpos.x, checkpos.y]) continue;

                // 味方のユニットを無視
                if (Player == units[checkpos.x, checkpos.y].Player) continue;

                // 追加
                ret.Add(checkpos);
            }
        }
        // unitの種類 INU
        if (TYPE.INU == type)
        {
            int dir = 1;
            if (1 == Player) dir = -1;
            // 前後左右1コマずつ進む
            List<Vector2Int> vec = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * dir ),
                new Vector2Int(0 , -1 * dir ),
                new Vector2Int(1 , 0 * dir ),
                new Vector2Int(-1 , 0 * dir ),
            };

            // 前方
            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;
                if (null != units[checkpos.x, checkpos.y]) break;

                ret.Add(checkpos);
            }

            // 取る
            vec = new List<Vector2Int>()
            {
                new Vector2Int(0 , 1 * dir ),
                new Vector2Int(0 , -1 * dir ),
                new Vector2Int(1 , 0 * dir ),
                new Vector2Int(-1 , 0 * dir ),
            };

            foreach (var v in vec)
            {
                Vector2Int checkpos = Pos + v;
                if (!isCheckable(units, checkpos)) continue;

                // なにもない
                if (null == units[checkpos.x, checkpos.y]) continue;

                // 味方のユニットを無視
                if (Player == units[checkpos.x, checkpos.y].Player) continue;

                // 追加
                ret.Add(checkpos);
            }
        }
        return ret;
    }

    // 盤の上か
    bool isCheckable(UnitController[,] ary, Vector2Int idx)
    {
        if(    idx.x < 0 || ary.GetLength(0) <= idx.x
            || idx.y < 0 || ary.GetLength(1) <= idx.y )
        {
            return false;
        }

        return true;
    }

    // 選択時の処理
    public void SelectUnit(bool select = true)
    {
        Vector3 pos = transform.position;
     //   pos.y += 2;
        GetComponent<Rigidbody>().isKinematic = true;

        // 選択解除
        if (!select)
        {
     //       pos.y = 1.35f;
            GetComponent<Rigidbody>().isKinematic = false;
        }

        transform.position = pos;
    }

    // 移動処理
    public void MoveUnit(GameObject tile)
    {
        // 移動時は非選択状態にする
        SelectUnit(false);

        // タイルのポジションから配列番号に戻す
        Vector2Int idx = new Vector2Int(
            (int)tile.transform.position.x + GameSceneDirector.TILE_X / 2,
            (int)tile.transform.position.z + GameSceneDirector.TILE_Y / 2);

        // 新しい場所へ移動
        Vector3 pos = tile.transform.position;
        pos.y = 1.35f;
        transform.position = pos;
        // アニメーター
            GetComponent<Animator>().SetFloat("Forward", 1);
        // 移動状態リセット
        Status.Clear();

        // インデックスの更新
        OldPos = Pos;
        Pos = idx;

        // 置いてからのターンをリセット
        ProgressTurnCount = 0;

    }

    // 前回移動してからのターンをカウント
    public void ProgressTurn()
    {
        // 最初は無視
        if (0 > ProgressTurnCount) return;

        ProgressTurnCount++;
    }

    // 今回のターンのチェック状態をセット
    public void SetCheckStatus(bool flag = true)
    {
        Status.Remove(STATUS.CHECK);
        if (flag) Status.Add(STATUS.CHECK);
    }
        
    //必殺技の切り替え3種類
    public void Special()
    {
        if (isSpecial == false)
        {
            isSpecial = true;
            isNinjutu = false;
            isUsually = false;
        }
    }
    public void Ninjutu()
    {
        if (isNinjutu == false)
        {
            isSpecial = false;
            isNinjutu = true;
            isUsually = false;
        }

    }
    public void Usually()
    {
        if (isUsually == false)
        {
            isSpecial = false;
            isNinjutu = false;
            isUsually = true;
        }
    }


}