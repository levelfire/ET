using System.Timers;

namespace ET.Server
{
    [FriendOf(typeof(AccountCheckOutTimeComponent))]
    //[Timer(TimerInvokeType.AccountSessionCheckOutTimer)]
    [Invoke(TimerInvokeType.AccountSessionCheckOutTimer)]
    public class AccountSessionCheckOutTimer : ATimer<AccountCheckOutTimeComponent>
    {
        protected override void Run(AccountCheckOutTimeComponent self)
        {
            try
            {
                self.DeleteSession();
            }
            catch (System.Exception e)
            {

                Log.Error(e.ToString());
            }
        }
    }

    public class AccountCheckOutTimeComponentAwakeSystem : AwakeSystem<AccountCheckOutTimeComponent, long>
    {
        protected override void Awake(AccountCheckOutTimeComponent self, long accountId)
        {
            self.AccountId = accountId;
            TimerComponent.Instance.Remove(ref self.Timer);
            //TODO
            self.Timer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 100 * 60 * 1000, TimerInvokeType.AccountSessionCheckOutTimer, self);
        }
    }

    public class AccountCheckOutTimeComponentDestroySystem : DestroySystem<AccountCheckOutTimeComponent>
    {
        protected override void Destroy(AccountCheckOutTimeComponent self)
        {
            self.AccountId = 0;
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }

    [FriendOf(typeof(AccountCheckOutTimeComponent))]
    public static class AccountCheckOutTimeComponentSystem
    {
        public static void DeleteSession(this AccountCheckOutTimeComponent self)
        {
            Session session = self.GetParent<Session>();
            long sesionInstanceId = session.DomainScene().GetComponent<AccountSessionsComponent>().Get(self.AccountId);
            if (self.InstanceId == sesionInstanceId)
            {
                session.DomainScene().GetComponent<AccountSessionsComponent>().Get(self.AccountId);
            }

            session?.Send(new A2C_Disconnect { Error = 1 });
            session?.Disconnect().Coroutine();
        }


    }
}
