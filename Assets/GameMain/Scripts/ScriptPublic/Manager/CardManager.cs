﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using GamePlay;
using UniRx;
#if false
public class CardManager : MonoSingleton<CardManager> 
{
    private string[] cardNames;  //所有牌集合
    public Transform heapPos;           //牌堆位置
    public Transform[] playerHeapPos;    //玩家牌堆位置
    public CardManagerStates cardManagerState;  //卡牌回合状态
    private int termCurrentIndex;  //回合当前玩家索引
    private int termStartIndex;  //回合开始玩家索引
    public GameObject coverPrefab;      //背面排预制件
    public float dealCardSpeed = 20;  //发牌速度
    public override void Init()
    {
         cardNames = GetCardNames();
    }

    public ReactiveCollection<Player> Players
    {
        get { return RoomManager.Instance.rData.roomPlayers; }
    }

    #region 洗牌、发牌
    /// <summary>
    /// 洗牌
    /// </summary>
    public void ShuffleCards()
    {
        //进入洗牌阶段
        cardManagerState = CardManagerStates.ShuffleCards;
        cardNames = cardNames.OrderBy(c => Guid.NewGuid()).ToArray();
    }
    /// <summary>
    /// 开始发牌
    /// </summary>
    public IEnumerator DealCards()
    {
        //进入发牌阶段
        cardManagerState = CardManagerStates.DealCards;
        termCurrentIndex = termStartIndex;

        yield return DealHeapCards(false);
    }
    /// <summary>
    /// 发牌堆上的牌（如果现在不是抢地主阶段，发普通牌，如果是，发地主牌）
    /// </summary>
    /// <returns></returns>
    private IEnumerator DealHeapCards(bool ifForBid)
    {
        //显示牌堆
        heapPos.gameObject.SetActive(true);
        playerHeapPos.ToList().ForEach(s => { s.gameObject.SetActive(true); });

        //按玩家数量，每人3张牌来取牌
        var cardNamesNeeded = cardNames.Take(Players.Count * 3);       

        //计算每张地主牌的位置
        int cardIndex = 0;
        foreach (var cardName in cardNamesNeeded)
        {
            //给当前玩家发一张牌
            Players[termCurrentIndex].AddCard(cardName);

            var cover = Instantiate(coverPrefab, heapPos.position, Quaternion.identity, heapPos.transform);
            cover.GetComponent<RectTransform>().localScale = Vector3.one;
            //移动动画，动画结束后自动销毁
            var tween = cover.transform.DOMove(playerHeapPos[termCurrentIndex].position, 0.3f);
            tween.OnComplete(() => Destroy(cover));

            yield return new WaitForSeconds(1 / dealCardSpeed);

         
            //下一个需要发牌者
            SetNextPlayer();

            cardIndex++;
        }

        //隐藏牌堆
        heapPos.gameObject.SetActive(false);
        playerHeapPos[0].gameObject.SetActive(false);

        //发普通牌
        //显示玩家手牌
        ShowPlayerSelfCards();
    }

    /// <summary>
    /// 显示玩家手牌
    /// </summary>
    private void ShowPlayerSelfCards()
    {
        //销毁玩家手牌
        DestroyPlayerSelfCards();

        Players.ToList().ForEach(s =>
        {
            var player0 = s as PlayerSelf;
            if (player0 != null)
            {
                player0.GenerateAllCards();
            }
        });
    }
    /// <summary>
    /// 清空牌局
    /// </summary>
    public void ClearCards()
    {
        //清空所有玩家卡牌
        Players.ToList().ForEach(s => s.DropCards());

        //销毁玩家手牌
        DestroyPlayerSelfCards();
    }
    /// <summary>
    /// 
    /// </summary>
    private void DestroyPlayerSelfCards()
    {
        //销毁玩家手牌
        Players.ToList().ForEach(s =>
        {
            var player0 = s as PlayerSelf;
            if (player0 != null)
            {
                player0.DestroyAllCards();
            }
        });
    }
    /// <summary>
    /// 加载所有卡牌名
    /// </summary>
    /// <returns></returns>
    private string[] GetCardNames()
    {
       //路径  
        string fullPath = "Assets/Arts/Resources/Cards/";

        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            FileInfo[] files = direction.GetFiles("*.jpg", SearchOption.AllDirectories);

            return files.Select(s => Path.GetFileNameWithoutExtension(s.Name)).ToArray();
        }
        return null;
    }
    #endregion
}
#endif