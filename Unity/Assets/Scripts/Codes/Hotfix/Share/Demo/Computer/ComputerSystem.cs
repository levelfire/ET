namespace ET
{
    public static class ComputerSystem
    {
        public static void Start(this Computer self)
        {
            Log.Debug("Computer Start!!!!");
            self.GetComponent<PCCaseComponent>().StartPower();
        }
    }
}
