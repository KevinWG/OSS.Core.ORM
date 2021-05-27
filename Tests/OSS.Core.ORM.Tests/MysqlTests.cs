using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using OSS.Common.BasicMos.Resp;
using OSS.Common.Helpers;
using OSS.Core.ORM.Dapper;
using OSS.Core.ORM.Dapper.OrmExtension;

namespace OSS.Core.ORM.Tests
{
    [TestClass]
    public class MysqlsqlTests
    {
        [TestMethod]
        public void SqlExpTest()
        {
            var visitor = new SqlExpressionVisitor();
            var updateFlag = new SqlVistorFlag(SqlVistorType.Update);

            Expression<Func<object>> f = () => new {name = "test"};

            visitor.Visit(f, updateFlag);
            var paras = visitor.parameters;
             
            var sql = updateFlag.sql;
            Assert.IsTrue(paras.Count>0);
        }


        [TestMethod]
        public async Task Test1()
        {
            var id = NumHelper.SnowNum();

            var userList = new List<UserInfo>();
            for (int i = 0; i < 3; i++)
            {
                id += i;
                userList.Add(new UserInfo()
                {
                    id   = id,
                    name = $"test_name_{id}"
                });
            }

            var addRes = await MysqlUserInfoRep.Instance.AddList(userList);
            Assert.IsTrue(addRes.IsSuccess());

            var updateRes = await MysqlUserInfoRep.Instance.UpdateName(id, $"test_update_name{id}");
            Assert.IsTrue(updateRes.IsSuccess());


            var getRes = await MysqlUserInfoRep.Instance.Get(id);
            Assert.IsTrue(getRes.IsSuccess());
            
        }
    }

    public class MysqlUserInfoRep : BaseRep<MysqlUserInfoRep, UserInfo, long>
    {
        protected string _connectStr ;
    
        public MysqlUserInfoRep()
        {     
            _connectStr = "server=127.0.0.1;database=test_database;uid=root;pwd=123456;"; 
        }
        
        public async Task<Resp> UpdateName(long id, string name)
        {
            var teU = new UserInfo() { id = id, name = name };

            return await Update(u => new { name = teU.name }, u => u.id == id);
        }
        
        public async Task<Resp<UserBigInfo>> Get(long id)
        {
            return await Get<UserBigInfo>(u => u.id == id);
        }


        protected override string GetTableName()
        {
            return "user_info";
        }

        protected override IDbConnection GetDbConnection(bool isWriteOperate)
        {
            return new MySqlConnection(_connectStr);
        }


        public Task<ListResp<UserBigInfo>> GetList()
        {
            return GetList<UserBigInfo>(w => w.id > 0);
        }
    }

}
