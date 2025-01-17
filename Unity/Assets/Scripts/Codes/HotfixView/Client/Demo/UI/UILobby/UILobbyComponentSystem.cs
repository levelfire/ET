﻿using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [FriendOf(typeof(UILobbyComponent))]
    public static class UILobbyComponentSystem
    {
        [ObjectSystem]
        public class UILobbyComponentAwakeSystem: AwakeSystem<UILobbyComponent>
        {
            protected override void Awake(UILobbyComponent self)
            {
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

                self.enterMap = rc.Get<GameObject>("EnterMap");
                self.enterMap.GetComponent<Button>().onClick.AddListener(() => { self.EnterMap().Coroutine(); });
            }
        }
        
        public static async ETTask EnterMap(this UILobbyComponent self)
        {
            //await EnterMapHelper.EnterMapAsync(self.ClientScene());
            await MatchingHelper.MatchingApply(self.ClientScene());
            await UIHelper.Remove(self.ClientScene(), UIType.UILobby);
            //await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("LocalScene");
            //await ETTask.CompletedTask;

            
        }
    }
}