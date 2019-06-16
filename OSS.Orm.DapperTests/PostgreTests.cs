using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSS.Common.ComModels;
using OSS.Orm.DapperPgsql;

namespace OSS.Orm.DapperTests
{
    [TestClass]
    public class PostgresqlTests:BaseTest
    {
        [TestMethod]
        public async Task Test()
        {
            var addRes = await UserInfoRep.Instance.Add(new UserInfo()
            {
                id = "test",
                user_name = "TestName"
            });
        }
    }


    public class UserInfo:BaseMo
    {
        public string user_name { get; set; }

    }


    public class UserInfoRep : BasePgRep<UserInfoRep, UserInfo>
    {
        private static readonly string connectStr = ConfigUtil.GetConnectionString("WriteConnection");

        public UserInfoRep() : base(connectStr, connectStr)
        {
            
        }
    }
}
