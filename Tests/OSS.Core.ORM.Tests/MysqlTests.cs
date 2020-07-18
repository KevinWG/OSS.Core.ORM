using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OSS.Common.BasicMos.Resp;
using OSS.Common.Extention;
using OSS.Common.Helpers;
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
            //var id = NumHelper.SnowNum().ToString();
            //var time = DateTime.Now.ToUtcSeconds();

            //var addRes = MysqlUserInfoRep.Instance.Add(new UserInfo()
            //{
            //    id = id,
            //    user_name = $"test_name_{id}"
            //}).Result;
            //Assert.IsTrue(addRes.IsSuccess());

            //var updateRes = MysqlUserInfoRep.Instance.UpdateName(id,$"test_update_name{id}").Result;
            //Assert.IsTrue(updateRes.IsSuccess());


            //var getRes = MysqlUserInfoRep.Instance.Get(id).Result;
            //Assert.IsTrue(getRes.IsSuccess());

            //var getListRes = UserInfoRep.Instance.GetList().Result;
            //Assert.True(getListRes.IsSuccess());
        }
    }

    public class MysqlUserInfoRep : UserInfoRep
    {
        public MysqlUserInfoRep()
        {     
            _connectStr = "server=127.0.0.1;database=oss.core;uid=root;pwd=123456;charset=utf8mb4"; 
        }
    }

}
