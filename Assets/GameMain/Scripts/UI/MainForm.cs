﻿using GameFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using UniRx;

namespace GamePlay
{
    public class MainForm : UGuiForm
    {
        private SubMainKeyBoard subMainKeyBoard;
        
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            GUILink link = GetComponent<GUILink>();
            subMainKeyBoard = link.AddComponent<SubMainKeyBoard>("PanelJoinRoom");
            link.SetEvent("BtnSangong", UIEventType.Click, OnStartSangong);
            link.SetEvent("BtnShop", UIEventType.Click, OnShopClick);
            link.SetEvent("BtnJoinRoom", UIEventType.Click, OnJoinRoom);


            RoleData role = GameManager.Instance.GetRoleData();
            //string id = "user_id=" + role.id.Value;
            //string token = "access_token=" + role.token.Value;
            //long time = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            //string timeStr = "timestamp=" + time.ToString();

            Recv_Get_MainPage mainPage = NetWorkManager.Instance.CreateGetMsg<Recv_Get_MainPage>(GameConst._mainPage, 
                GameManager.Instance.GetSendInfoStringList<Send_Get_MainPage>(role.id.Value, role.token.Value));
            if (mainPage != null && mainPage.code == 0)
            {
                role.SetRoleProperty(mainPage.data);
            }
        }

        private void OnJoinRoom(object[] args)
        {
            subMainKeyBoard.OpenUI();
        }

        public void OnStartSangong(params object[] args)
        {
            var main = GameEntry.Procedure.CurrentProcedure as ProcedureMain;
            main.ChangeGame(GameMode.Sangong);
        }

        public void OnShopClick(params object[] args)
        {
            RoleData role = GameManager.Instance.GetRoleData();

            //商店测试
            //Recv_Get_Shop shopPage = NetWorkManager.Instance.CreateGetMsg<Recv_Get_Shop>(GameConst._shop,
            //    GameManager.Instance.GetSendInfoStringList<Send_Get_Shop>(role.id.Value, role.token.Value));
            //if (shopPage != null && shopPage.code == 0)
            //{
            //    Debug.Log("show skop page");
            //}

            //订单测试
            Recv_Post_Order shopPage = NetWorkManager.Instance.CreatePostMsg<Recv_Post_Order>(GameConst._order,
                GameManager.Instance.GetSendInfoStringList<Send_Post_Order>(role.id.Value, role.token.Value, "0"));
            if (shopPage != null && shopPage.code == 0)
            {
                Debug.Log("show skop page");
            }
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnOpen(object userData)
#else
        protected internal override void OnOpen(object userData)
#endif
        {
            base.OnOpen(userData);
        }

#if UNITY_2017_3_OR_NEWER
        protected override void OnClose(object userData)
#else
        protected internal override void OnClose(object userData)
#endif
        {

            base.OnClose(userData);
        }
    }
    
    public class SubMainKeyBoard : UGuiComponent
    {
        private Text TextContent;
        private GameObject TextPsd,TextTitle;
        private ReactiveProperty<int> inputText = new ReactiveProperty<int>(-1);
        private ReactiveProperty<bool> bInputId = new ReactiveProperty<bool>(true);
        private int roomId;
        private string room_name;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            GUILink link = GetComponent<GUILink>();
            TextContent = link.Get<Text>("TextContent");
            TextPsd = link.Get("TextPsd");
            TextTitle = link.Get("TextTitle");
            for (int i = 0; i < 10; i++)
            {
                int index = i;
                link.SetEvent("Button" + i, UIEventType.Click, x =>
                {
                    if (inputText.Value == -1)
                    {
                        inputText.Value = index;
                    }
                    else if (inputText.Value < 1000)
                    {
                        inputText.Value = inputText.Value * 10 + index;
                    }
                });
            }

            link.SetEvent("ButtonOK", UIEventType.Click, ClickOk);
            link.SetEvent("ButtonDel", UIEventType.Click, ClickDel);
            link.SetEvent("ButtonClose", UIEventType.Click, _ => {OnClose(null); });
        }

        private void ClickDel(object[] args)
        {
            if (inputText.Value <10 && inputText.Value >=0)
            {
                inputText.Value = -1;
            }
            else if (inputText.Value>=10 && inputText.Value < 10000)
            {
                inputText.Value = inputText.Value /10;
            }
        }

        private void ClickOk(object[] args)
        {
            var rData = GameManager.Instance.GetRoleData();
            if (bInputId.Value)
            {
                //search
               
                inputText.Value = -1;
                roomId = -1;
               
                Recv_SearchRoom searchRoom = NetWorkManager.Instance.CreateGetMsg<Recv_SearchRoom>(GameConst._searchRoom, GameManager.Instance.GetSendInfoStringList<Send_Search_Room>(inputText.Value.ToString()));

                if (searchRoom != null)
                {
                    if (searchRoom.code == 0)
                    {
                        bInputId.Value = false;
                        roomId = searchRoom.data.room_id;
                        room_name = searchRoom.data.room_name;
                    }
                    else
                    {
                        //提示
                        Log.Debug("搜索房间{0}失败",searchRoom.data.room_id);
                    }
                }
            }
            else
            {
                //Join
                Recv_JoinRoom joinRoom = NetWorkManager.Instance.CreateGetMsg<Recv_JoinRoom>(GameConst._joinRoom, GameManager.Instance.GetSendInfoStringList<Send_Join_Room>(roomId.ToString(),inputText.Value.ToString()));

                if (joinRoom != null)
                {
                    if (joinRoom.code == 0)
                    {
                        bInputId.Value = false;
                        RoomManager.Instance.rData.id.Value = joinRoom.data.room_id;
                        RoomManager.Instance.rData.gId.Value = joinRoom.data.GID;
                        Log.Debug("加入房间{0}成功，GID={1}",joinRoom.data.room_id,joinRoom.data.GID);
                        
                        //初始化房间
                        RoomManager.Instance.Init(joinRoom.data.GID,joinRoom.data.room_id,room_name,0);
                        //初始化扑克管理器
                        var cardManager = CardManager.Instance;
                        var main = GameEntry.Procedure.CurrentProcedure as ProcedureMain;
                        main.ChangeGame(GameMode.Sangong);
                        GameEntry.UI.OpenUIForm(UIFormId.TableForm, main);
                        HideUI();
                    }
                    else
                    {
                        //提示
                    }
                    roomId = -1;
                    //TODO 关闭界面跳转
                }
            }
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            bInputId.Value = true;
            inputText.Value = -1;
            roomId = -1;
            bInputId.Subscribe(x=>TextTitle.SetActive(x)).AddTo(disPosable);
            bInputId.Subscribe(x=>TextPsd.SetActive(!x)).AddTo(disPosable);
            inputText.Subscribe(x=>
            {
                TextContent.text = x == -1? "" : x.ToString();
                
            }).AddTo(disPosable);
        }
    }
}
