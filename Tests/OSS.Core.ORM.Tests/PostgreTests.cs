using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using OSS.Common.BasicMos.Resp;
using OSS.Common.Extention;
using OSS.Common.Helpers;
using OSS.Core.ORM.Dapper;

namespace OSS.Core.ORM.Tests
{
    [TestClass]
    public class PostgresqlTests
    {
        [TestMethod]
        public void Test1()
        {
            //var id = NumHelper.SnowNum().ToString();
            //var time = DateTime.Now.ToUtcSeconds();

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

    public class PGUserInfoRep : BaseRep<PGUserInfoRep, UserInfo, long>
    {

        protected string _connectStr = "";


        public async Task<Resp> UpdateName(long id, string name)
        {
            var teU = new UserInfo() { id = id, name = name };
            return await Update(u => new { user_name = teU.name }, u => u.id == id);
        }

        public async Task<Resp> Get(long id)
        {
            return await Get(u => u.id == id);
        }


        protected override string GetTableName()
        {
            return "user_info";
        }

        protected override IDbConnection GetDbConnection(bool isWriteOperate)
        {
            return new NpgsqlConnection(_connectStr);
        }

    }

}
