namespace ET.Client
{
    
    public class AccountInfoComponentDestroySystem : DestroySystem<AccountInfoComponent>
    {
        protected override void Destroy(AccountInfoComponent self)
        {
            self.Token = string.Empty;
            self.AccountId = 0;
        }
    }

    [FriendOf(typeof(AccountInfoComponent))]
    public static class AccountInfoComponentSystem
    {
    }
}
