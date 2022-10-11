using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene clientScene, string account, string password)
        {
            try
            {
                // 创建一个ETModel层的Session
                clientScene.RemoveComponent<RouterAddressComponent>();
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
                if (routerAddressComponent == null)
                {
                    routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();

                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }
                IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

                R2C_Login r2CLogin;
                using (Session session = await RouterHelper.CreateRouterSession(clientScene, realmAddress))
                {
                    r2CLogin = (R2C_Login)await session.Call(new C2R_Login() { Account = account, Password = password });
                }

                // 创建一个gate Session,并且保存到SessionComponent中
                Session gateSession = await RouterHelper.CreateRouterSession(clientScene, NetworkHelper.ToIPEndPoint(r2CLogin.Address));
                clientScene.AddComponent<SessionComponent>().Session = gateSession;

                G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(
                    new C2G_LoginGate() { Key = r2CLogin.Key, GateId = r2CLogin.GateId });

                Log.Debug("登陆gate成功!");

                await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static async ETTask<int> LoginAccount(Scene clientScene, string address, string account, string password)
        {
            A2C_LoginAccount a2C_LoginAccount = null;
            Session accountSession = null;

            try
            {
                // 创建一个ETModel层的Session
                clientScene.RemoveComponent<RouterAddressComponent>();
                // 获取路由跟realmDispatcher地址
                RouterAddressComponent routerAddressComponent = clientScene.GetComponent<RouterAddressComponent>();
                if (routerAddressComponent == null)
                {
                    routerAddressComponent = clientScene.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
                    await routerAddressComponent.Init();

                    clientScene.AddComponent<NetClientComponent, AddressFamily>(routerAddressComponent.RouterManagerIPAddress.AddressFamily);
                }

                IPAddress ipAddress = IPAddress.Parse(ConstValue.RouterHttpHost);
                var address1 =  new IPEndPoint(ipAddress, 10005);
                //accountSession = clientScene.GetComponent<NetClientComponent>().Create(NetworkHelper.ToIPEndPoint(address));
                accountSession = await RouterHelper.CreateRouterSession(clientScene, address1);
                //clientScene.AddComponent<SessionComponent>().Session = accountSession;
                password = MD5Helper.StringMD5(password);
                a2C_LoginAccount
                    = (A2C_LoginAccount)await accountSession.Call(new C2A_LoginAccount { AccountName = account, Password = password });
            }
            catch (Exception e)
            {
                accountSession.Dispose();
                Log.Error(e.ToString());
                return ErrorCode.ERR_NetWorkError;
            }

            if (a2C_LoginAccount.Error != ErrorCode.ERR_Success)
            {
                return a2C_LoginAccount.Error;
            }

            clientScene.AddComponent<SessionComponent>();
            clientScene.GetComponent<SessionComponent>().Session = accountSession;
            //clientScene.GetComponent<SessionComponent>().Session.AddComponent<PingComponent>();

            clientScene.GetComponent<AccountInfoComponent>().Token = a2C_LoginAccount.Token;
            clientScene.GetComponent<AccountInfoComponent>().AccountId = a2C_LoginAccount.AccountId;

            Log.Debug("登陆Account成功!");
            await EventSystem.Instance.PublishAsync(clientScene, new EventType.LoginFinish());

            return ErrorCode.ERR_Success;
        }
    }
}