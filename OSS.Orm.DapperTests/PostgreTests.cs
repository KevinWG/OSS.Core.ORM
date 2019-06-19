using System;
using System.Threading.Tasks;
using OSS.Common.ComModels;
using OSS.Common.ComUtils;
using OSS.Common.Extention;
using OSS.Orm.DapperPgsql;
using Xunit;

namespace OSS.Orm.DapperTests
{

    public class PostgresqlTests
    {
      
        [Fact]
        public void Test1()
        {
            var id = NumUtil.SnowNum().ToString();
            var time = DateTime.Now.ToUtcSeconds();

            var addRes = UserInfoRep.Instance.Add(new UserInfo()
            {
                id = id,
                user_name = $"test_name_{id}",
                add_time = time,
                m_time = time
            }).Result;
            Assert.True(addRes.IsSuccess());

            var updateRes = UserInfoRep.Instance.UpdateName(id,$"test_update_name{id}").Result;
            Assert.True(updateRes.IsSuccess());


            var getRes = UserInfoRep.Instance.Get(id).Result;
            Assert.True(getRes.IsSuccess());

            var getListRes = UserInfoRep.Instance.GetList().Result;
            Assert.True(getListRes.IsSuccess());
        }
    }


    public class UserInfo:BaseMo<string>
    {
        public string user_name { get; set; }

    }


    public class UserInfoRep : BasePgRep<UserInfoRep, UserInfo,string>
    {
        private static readonly string connectStr = "";

        public UserInfoRep() : base(connectStr, connectStr)
        {
            m_TableName = "userinfo";
        }


        public async Task<ResultMo> UpdateName(string id, string name)
        {
            var teU=new UserInfo(){id=id,user_name = name};
            return await Update(u => new {teU.user_name}, u => u.id == id);
        }
        //public async Task<ResultMo> UpdateName(string id, string name)
        //{
        //    var teU = new UserInfo() { id = id, user_name = name };
        //    return await Update(u => new { teU.user_name }, u => u.id == id, teU);
        //}
        public async Task<ResultMo> Get(string id)
        {
            return await Get(u => u.id == id);
        }


        public async Task<ResultMo> GetList()
        {
            return await GetList(u => u.add_time>0);
        }
    }
}
