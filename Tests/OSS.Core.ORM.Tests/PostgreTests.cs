using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSS.Common.BasicMos.Resp;
using OSS.Common.Extention;
using OSS.Common.Helpers;

namespace OSS.Core.ORM.Tests
{
    [TestClass]
    public class PostgresqlTests
    {
        [TestMethod]
        public void Test1()
        {
            var id = NumHelper.SnowNum().ToString();
            var time = DateTime.Now.ToUtcSeconds();

            //var addRes = PgUserInfoRep.Instance.Add(new UserInfo()
            //{
            //    id = id,
            //    user_name = $"test_name_{id}"
            //}).Result;
            //Assert.IsTrue(addRes.IsSuccess());

            //var updateRes = PgUserInfoRep.Instance.UpdateName(id,$"test_update_name{id}").Result;
            //Assert.IsTrue(updateRes.IsSuccess());


            //var getRes = PgUserInfoRep.Instance.Get(id).Result;
            //Assert.IsTrue(getRes.IsSuccess());

            //var getListRes = UserInfoRep.Instance.GetList().Result;
            //Assert.True(getListRes.IsSuccess());
        }
    }

    public class PgUserInfoRep : UserInfoRep
    {
        public PgUserInfoRep()
        {
            _connectStr = "";
        }
    }

}
