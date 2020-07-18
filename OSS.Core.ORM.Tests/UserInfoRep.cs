using System.Threading.Tasks;
using Npgsql;
using OSS.Common.BasicMos;
using OSS.Common.BasicMos.Resp;
using OSS.Core.ORM.Pgsql.Dapper;

namespace OSS.Core.ORM.Tests
{

    public class UserInfo : BaseMo<string>
    {
        public string user_name { get; set; }
    }


    public class UserInfoRep : BasePgRep<UserInfoRep, UserInfo, string>
    {

        protected string _connectStr = "";

       
        public async Task<Resp> UpdateName(string id, string name)
        {
            var teU = new UserInfo() { id = id, user_name = name };
            return await Update(u => new { teU.user_name }, u => u.id == id);
        }

        public async Task<Resp> Get(string id)
        {
            return await Get(u => u.id == id);
        }


        protected override string GetTableName()
        {
            return "user_info";
        }

        protected override NpgsqlConnection GetDbConnection(bool isWriteOperate)
        {
            return new NpgsqlConnection(_connectStr);
        }

    }
}
