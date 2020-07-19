using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Npgsql;
using OSS.Common.BasicMos.Resp;
using OSS.Common.Helpers;
using OSS.Core.ORM.Mysql.Dapper;
using OSS.Core.ORM.Mysql.Dapper.OrmExtension;

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
        public void Test1()
        {
            var userList = new List<UserInfo>();
            for (int i = 0; i < 3; i++)
            {
                var id = NumHelper.SnowNum().ToString();

                userList.Add(new UserInfo()
                {
                    id = id,
                    name = $"test_name_{id}"
                });
            }

            var addRes = MysqlUserInfoRep.Instance.AddList(userList).Result;
            Assert.IsTrue(addRes.IsSuccess());

            //var updateRes = MysqlUserInfoRep.Instance.UpdateName(id,$"test_update_name{id}").Result;
            //Assert.IsTrue(updateRes.IsSuccess());


            //var getRes = MysqlUserInfoRep.Instance.Get(id).Result;
            //Assert.IsTrue(getRes.IsSuccess());

            //var getListRes = UserInfoRep.Instance.GetList().Result;
            //Assert.True(getListRes.IsSuccess());
        }
    }

    public class MysqlUserInfoRep : BaseMysqlRep<MysqlUserInfoRep, UserInfo, string>
    {
        protected string _connectStr ;
    
        public MysqlUserInfoRep()
        {     
            _connectStr = "server=127.0.0.1;database=test_database;uid=root;pwd=123456;"; 
        }


        public async Task<Resp> UpdateName(string id, string name)
        {
            var teU = new UserInfo() { id = id, name = name };
            return await Update(u => new { user_name = teU.name }, u => u.id == id);
        }

        public async Task<Resp> Get(string id)
        {
            return await Get(u => u.id == id);
        }


        protected override string GetTableName()
        {
            return "user_info";
        }

        protected override MySqlConnection GetDbConnection(bool isWriteOperate)
        {
            return new MySqlConnection(_connectStr);
        }
    }

}
