using NUnit.Framework;
using OSS.Common.ComModels;
using OSS.Orm.DapperPgsql;
using OSS.Orm.DapperTests;

namespace OSS.Orm.DapperTest
{

    public class PostgresqlTests:BaseTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public  void Test1()
        {
            var addRes =  UserInfoRep.Instance.Add(new UserInfo()
            {
                id = "test",
                user_name = "TestName"
            }).Result;
            Assert.Pass();
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
