namespace ET
{
    public enum AccountType
    {
        General = 0,
        BlackList = 1,
    }

    [ChildOf(typeof(Session))]
    public class Account:Entity,IAwake
    {
        public string AccountName { get; set; }
        public string Password { get; set; }
        public long CreateTime { get; set; }
        public AccountType AccountType { get; set; }
    }
}
