namespace ET
{
    public class ComputerAwakeSystem : AwakeSystem<Computer>
    {
        protected override void Awake(Computer self)
        {
            Log.Debug("Computer Awake!!!!");
        }
    }

    public class ComputerUpdateSystem : UpdateSystem<Computer>
    {
        protected override void Update(Computer self)
        {
            Log.Debug("Computer Update!!!!");
        }
    }

    public class ComputerDestorySystem : DestroySystem<Computer>
    {
        protected override void Destroy(Computer self)
        {
            Log.Debug("Computer Destory!!!!");
        }
    }

    public static class ComputerSystem
    {
        public static void Start(this Computer self)
        {
            Log.Debug("Computer Start!!!!");
            self.GetComponent<PCCaseComponent>().StartPower();
        }
    }
}
