﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening.Plugins;
using UnityGameFramework.Runtime;

namespace GamePlay
{
    public class TableForm : UGuiForm
    {
        ProcedureMain main;
        public List<PlayerHeadInfo> playerWigets = new List<PlayerHeadInfo>();
        private const int PlayerMax = 6;
        public Button BtnSeat,BtnStartGame,BtnBanker0,BtnCancelReady,BtnLeaveSeat;
        private PanelSelectScore selectScoreUI;
        public GameObject BetPanel;
        private Slider SliderBet;
        private Text TextBet;
        private Text TextTime;
        private Text TextRoomId;
        private GameObject BL_lose, BL_win;
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            main = userData as ProcedureMain;
            GUILink link = GetComponent<GUILink>();
            for (int i = 0; i < PlayerMax; i++)
            {
                playerWigets.Add(link.AddComponent<PlayerHeadInfo>("playerHead"+i));
            }

            selectScoreUI = link.AddComponent<PanelSelectScore>("PanelSelectScore");
            BtnSeat = link.Get<Button>("BtnSeat");
            BtnStartGame = link.Get<Button>("BtnStartGame");
            BtnBanker0 = link.Get<Button>("BtnBanker0");
            BtnCancelReady = link.Get<Button>("BtnCancelReady");
            BtnLeaveSeat = link.Get<Button>("BtnLeaveSeat");
            BetPanel = link.Get("BetPanel");
            SliderBet = link.Get<Slider>("SliderBet");
            TextBet = link.Get<Text>("TextBet");
            TextTime = link.Get<Text>("TextTime");
            TextRoomId = link.Get<Text>("TextRoomId");
            BL_lose = link.Get("BL_lose");
            BL_win = link.Get("BL_win");
            
            
            link.SetEvent("Quit", UIEventType.Click, OnClickExit);
            link.SetEvent("BtnSeat", UIEventType.Click, OnClickSeat);
            link.SetEvent("BtnStartGame", UIEventType.Click, OnClickStartGame);
            link.SetEvent("BtnCancelReady", UIEventType.Click, OnClickCancelReady);
            link.SetEvent("BtnLeaveSeat", UIEventType.Click, OnClickLeaveSeat);
            link.SetEvent("BtnBanker0", UIEventType.Click, OnClickBid);
            link.SetEvent("BtnBet", UIEventType.Click, OnClickBet);
            
            
            
            
            List<Vector3> tempUITrans = new List<Vector3>();
            var cam = Camera.main;
            foreach (var w in playerWigets)
            {
                tempUITrans.Add(cam.ScreenToWorldPoint(w.cardPos.position));
            }
            CardManager.Instance.InitCardPos(tempUITrans);
        }

        public void DoShowWinEffect()
        {
            BL_win.SetActive(true);
            BL_win.transform.DOScale(2, 1).SetEase(Ease.InFlash).OnComplete(()=>BL_win.SetActive(false));
        }
        public void DoShowLoseEffect()
        {
            BL_lose.SetActive(true);
            BL_lose.transform.DOScale(2, 1).SetEase(Ease.InFlash).OnComplete(()=>BL_lose.SetActive(false));
        }

        private void OnClickBet(object[] args)
        {
            NetWorkManager.Instance.Send(Protocal.BET_REQ,RoomManager.Instance.rData.gId.Value, (int)SliderBet.value);
           
        }

        private void OnClickLeaveSeat(object[] args)
        {
            NetWorkManager.Instance.Send(Protocal.LEAVE,RoomManager.Instance.rData.gId.Value);
        }

        private void OnClickCancelReady(object[] args)
        {
            NetWorkManager.Instance.Send(Protocal.READY_CANCEL,RoomManager.Instance.rData.gId.Value);
        }

        private void OnClickStartGame(object[] args)
        {
            BtnSeat.gameObject.SetActive(false);
            BtnStartGame.gameObject.SetActive(false);
            RoomManager.Instance.Self.Value.state = EPlayerState.GamePrepare;
            //CardManager.Instance.DoDealCards();
        }

        public void SetPlayerData(Player player)
        {
            if (player.id.Value == RoomManager.Instance.Self.Value.id.Value)
            {
                playerWigets[0].SetPlayerData(player);
            }
            else
            {
                for (int i = 0; i < playerWigets.Count; i++)
                {
                    if (playerWigets[i].pId == 0)
                    {
                        playerWigets[i].SetPlayerData(player);
                    }
                }
            }

        }
        private void OnClickSeat(object[] args)
        {
            //发送坐下
            NetWorkManager.Instance.Send(Protocal.BALANCE_INFO);
            
//            playerWigets[0].SetPlayerData(RoomManager.Instance.Self.Value);
//            RoomManager.Instance.Self.Value.SetPos(0);
//            for (int i = 1; i <= RoomManager.Instance.rData.roomPlayers.Count; i++)
//            {
//                RoomManager.Instance.rData.roomPlayers[i-1].SetPos(i);
//            }
        }

        public void OnClickExit(params object[] args)
        {
            NetWorkManager.Instance.Send(Protocal.UNWATCH,RoomManager.Instance.rData.gId.Value);
            Close(true);
            main.ChangeGame(GameMode.Lobby);
        }


        public PlayerHeadInfo GetPlayerHeadUI(int pos)
        {
            if (pos >= 0 && pos < playerWigets.Count)
            {
                return playerWigets[pos];
            }

            return null;
        }
#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);
    
            
            RoomManager.Instance.rData.roomPlayers.ObserveAdd().Subscribe(x => RegRxForPlayer(x.Value))
                .AddTo(disPosable);

            RoomManager.Instance.Self.Value.pos.ObserveEveryValueChanged(x => x.Value).Where(x=>x<10).Subscribe(x =>
                {
                    RegRxForPlayer(RoomManager.Instance.Self.Value);
                }).AddTo(disPosable);
            
            SliderBet.OnValueChangedAsObservable().SubscribeToText(TextBet).AddTo(disPosable);
            //下注积分不能超过带入当前截止带入的剩余积分。
            RoomManager.Instance.Self.Value.score.Subscribe(x => SliderBet.maxValue = x).AddTo(disPosable);
            RoomManager.Instance.rData.name.SubscribeToText(TextRoomId).AddTo(disPosable);
            
            ResetWiget();
        }

        private void RegRxForPlayer(Player player)
        {
            SetPlayerData(player);
        }

        private void ResetWiget()
        {
            BtnSeat.gameObject.SetActive(true);
            BtnStartGame.gameObject.SetActive(false);
            BtnCancelReady.gameObject.SetActive(false);
            BtnLeaveSeat.gameObject.SetActive(false);
            BetPanel.SetActive(false);
            BtnBanker0.gameObject.SetActive(false);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(object userData)
#else
        protected internal override void OnClose(object userData)
#endif
        {

            base.OnClose(userData);
            BtnSeat.interactable = true;
            foreach (var wight in playerWigets)
            {
                wight.Reset();
            }
            
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            DateTime NowTime = DateTime.Now.ToLocalTime();

            TextTime.text = NowTime.ToString("HH:mm");
        }

        public void OpenSelectScore(int balance)
        {
            selectScoreUI.OpenUI(balance);
        }

        private void OnClickBid(object[] args)
        {
            //发送抢庄
            NetWorkManager.Instance.Send(Protocal.BID,RoomManager.Instance.rData.gId.Value,GameManager.Instance.GetRoleData().pId.Value);
        }
    }
    
    public class PlayerHeadInfo : UGuiComponent
    {
        private string _PlayerName;
        private Text textName;
        private Text textScore;
        public Transform cardPos;
        public int pId;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            GUILink link = GetComponent<GUILink>();

            textName = link.Get<Text>("TextName");
            textScore = link.Get<Text>("TextScore");
            cardPos = link.Get<Transform>("cardPos");
            textName.text = "";
            textScore.text = "";
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
        }

        public void SetPlayerData(Player role)
        {
            pId = role.id.Value;
            disPosable.Clear();
            _PlayerName = name;
          
            role.name.ObserveEveryValueChanged(x => x.Value).SubscribeToText(textName).AddTo(disPosable);
            role.score.ObserveEveryValueChanged(x => x.Value).SubscribeToText(textScore).AddTo(disPosable);
        }

        protected override void OnClose(object userData)
        {
            base.OnClose(userData);
        }

        public void Reset()
        {
            textName.text = "";
            textScore.text = "";
            pId = 0;
        }
    }
    public class PanelSelectScore : UGuiComponent
    {
        private Text TextScore;
        private Slider slider;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            GUILink link = GetComponent<GUILink>();

            TextScore = link.Get<Text>("TextScore");
            slider = link.Get<Slider>("Slider");
            link.SetEvent("ButtonOK", UIEventType.Click, OnButtonOK);
            link.SetEvent("ButtonClose", UIEventType.Click, OnButtonClose);
            
        }

        private void OnButtonClose(object[] args)
        {
            RoomManager.Instance.Self.Value.state = EPlayerState.Watch;
            OnClose(null);
        }

        private void OnButtonOK(object[] args)
        {
            byte pos = 0;
            int gId = RoomManager.Instance.rData.gId.Value;
            foreach (var seatData in RoomManager.Instance.rData.roomSeats)
            {
                if (seatData.pid <= 0)
                    pos = seatData.pos;
            }

            
           NetWorkManager.Instance.Send(Protocal.JOIN,gId,pos,(int)slider.value);
           OnClose(null);
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            slider.OnValueChangedAsObservable().SubscribeToText(TextScore).AddTo(disPosable);
            int balance = (int)userData;
            slider.maxValue = balance;
        }


        protected override void OnClose(object userData)
        {
            base.OnClose(userData);
        }
    }
}
