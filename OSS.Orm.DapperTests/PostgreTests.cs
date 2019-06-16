using OSS.Common.ComModels;
using OSS.Orm.DapperPgsql;
using Xunit;

namespace OSS.Orm.DapperTests
{

    public class PostgresqlTests
    {
      
        [Fact]
        public void Test1()
        {
            var addRes =  UserInfoRep.Instance.Add(new UserInfo()
            {
                id = "test",
                user_name = "TestName"
            }).Result;
        }
    }


    public class UserInfo:BaseMo
    {
        public string user_name { get; set; }

    }


    public class UserInfoRep : BasePgRep<UserInfoRep, UserInfo>
    {
        private static readonly string connectStr =
            "";

        public UserInfoRep() : base(connectStr, connectStr)
        {
            m_TableName = "userinfo";
        }
    }
}
