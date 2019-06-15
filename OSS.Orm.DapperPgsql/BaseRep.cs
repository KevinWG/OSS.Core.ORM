#region Copyright (C) 2019 Kevin (OSS开源实验室) 公众号：osscore

/***************************************************************************
*　　	文件功能描述：OSSCore仓储层 —— 仓储基类
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*    	创建日期：2019-6-15
*       
*****************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using OSS.Common.ComModels;
using OSS.Common.ComModels.Enums;
using OSS.Common.Plugs.LogPlug;
using OSS.Orm.DapperPgsql.OrmExtention;

namespace OSS.Orm.DapperPgsql
{
    /// <summary>
    /// 仓储层基类
    /// </summary>
    public class BaseRep<TRep,TType>
        where TRep:class ,new()
        where TType:BaseMo,new()
    {
        protected static string m_TableName;

        private static string _writeConnectionString;
        private static string _readeConnectionString;

        public BaseRep(string writeConnectionStr, string readeConnectionStr )
        {
            _writeConnectionString = writeConnectionStr?? "Host=117.48.228.143;Username=postgres;Password=admin20190610;Database=test_db";
            _readeConnectionString = readeConnectionStr?? "Host=117.48.228.143;Username=postgres;Password=admin20190610;Database=test_db";
        }
        

        #region 底层基础读写分离操作封装

        /// <summary>
        /// 执行写数据库操作
        /// </summary>
        /// <typeparam name="RType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected internal static Task<RType> ExcuteWriteAsync<RType>(Func<IDbConnection, Task<RType>> func) where RType : ResultMo, new()
            => Execute(func, _writeConnectionString);

        /// <summary>
        ///  执行读操作，返回具体类型，自动包装成ResultMo结果实体
        /// </summary>
        /// <typeparam name="RType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected internal static  Task<ResultMo<RType>> ExcuteReadeAsync<RType>(Func<IDbConnection, Task<RType>> func) => Execute(async con =>
        {
            var res =await func(con);
            return res != null ? new ResultMo<RType>(res) : new ResultMo<RType>(ResultTypes.ObjectNull, "未发现相关数据！");
        }, _readeConnectionString);

        /// <summary>
        /// 执行读操作，直接返回继承自ResultMo实体
        /// </summary>
        /// <typeparam name="RType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected internal static async Task<RType> ExcuteReadeResAsync<RType>(Func<IDbConnection, Task<RType>> func) where RType : ResultMo, new()
            =>await Execute(func, _readeConnectionString);

        private static async Task<RType> Execute<RType>(Func<IDbConnection, Task<RType>> func, string connecStr)
            where RType : ResultMo, new()
        {
            RType t;

            try
            {

                using (var con = new NpgsqlConnection(connecStr))
                {
                    t = await func(con);
                }

            }
            catch (Exception e)
            {
                LogUtil.Error(string.Concat("数据库操作错误，详情：", e.Message, "\r\n", e.StackTrace), "DataRepConnectionError",
                    "DapperRep");
                t = new RType
                {
                    ret = (int) ResultTypes.InnerError,
                    msg = "数据更新出错！"
                };
            }
            return t ?? new RType() {ret = (int) ResultTypes.ObjectNull, msg = "未发现对应结果"};
        }

        #endregion

        #region 基础CRUD 表达式扩展
        
        /// <summary>
        ///   插入数据
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        public Task<ResultIdMo> Add(TType mo)
            => ExcuteWriteAsync(con => con.Insert(m_TableName, mo));


        /// <summary>
        /// 部分字段的更新
        /// </summary>
        ///  <param name="updateExp">更新字段new{m.Name,....} Or new{ Name="",....}</param>
        /// <param name="whereExp">判断条件，如果为空默认根据Id判断</param>
        /// <param name="mo"></param>
        /// <returns></returns>
        protected static Task<ResultMo> Update(Expression<Func<TType, object>> updateExp,
            Expression<Func<TType, bool>> whereExp, object mo = null)
            => ExcuteWriteAsync(con => con.UpdatePartail(m_TableName, updateExp, whereExp, mo));


        /// <summary>
        ///  获取单个实体对象
        /// </summary>
        /// <param name="whereExp">判断条件，如果为空默认根据Id判断</param>
        /// <returns></returns>
        protected static Task<ResultMo<TType>> Get(Expression<Func<TType, bool>> whereExp)
            => ExcuteReadeAsync(con => con.Get(m_TableName, whereExp));


        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="whereExp"></param>
        /// <returns></returns>
        protected static Task<ResultMo<IList<TType>>> GetList(Expression<Func<TType, bool>> whereExp)
            => ExcuteReadeAsync(con => con.GetList(m_TableName, whereExp));


        /// <summary>
        /// 软删除，直接修改  status = CommonStatus.Delete 
        /// </summary>
        /// <param name="whereExp">条件表达式</param>
        /// <returns></returns>
        protected static Task<ResultMo> SoftDelete(Expression<Func<TType, bool>> whereExp)
        {
            return Update(m => new {status = CommonStatus.Delete}, whereExp);
        }

        #endregion

        #region 基础CRUD sql方法扩展

        /// <summary>
        ///  直接使用语句更新操作
        /// </summary>
        /// <param name="updateSql"></param>
        /// <param name="whereSql"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        protected virtual  Task<ResultMo> Update(string updateSql, string whereSql, object para = null)
            =>  ExcuteWriteAsync(async con =>
            {
                var sql = string.Concat("UPDATE ", m_TableName, " SET ", updateSql, whereSql);
                var row = await con.ExecuteAsync(sql, para);
                return row > 0 ? new ResultMo() : new ResultMo(ResultTypes.UpdateFail, "更新失败");
            });


        /// <summary>
        /// 通过sql语句获取实体
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="whereSql"> 条件sql语句</param>
        /// <param name="para"></param>
        /// <returns></returns>
        protected virtual Task<ResultMo<TType>> Get(string whereSql, object para)
        {
            string sql = string.Concat("select * from ", m_TableName," ", whereSql);
            return ExcuteReadeAsync(con => con.QuerySingleOrDefaultAsync<TType>(sql, para));
        }


        /// <summary>
        /// 通过id获取实体
        /// 此方法因为使用广泛，不添加tenantid条件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  virtual Task<ResultMo<TType>> GetById(string id)
        {
            const string whereSql = " WHERE id=@id";
            var dirPara = new Dictionary<string, object> { { "@id", id } };
           
            return  Get(whereSql, dirPara);
        }

        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="getsql">查询语句</param>
        /// <param name="paras">参数内容</param>
        /// <returns></returns>
        protected virtual async Task<ResultListMo<TType>> GetList(string getsql,
            object paras)
        {
            return await ExcuteReadeResAsync(async con =>
            {
                var list = (await con.QueryAsync<TType>(getsql, paras))?.ToList();

                return list?.Count > 0
                    ? new ResultListMo<TType>(list)
                    : new ResultListMo<TType>(ResultTypes.ObjectNull, "没有查到相关信息！");
            });
        }
        
        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="selectSql">查询语句，包含排序等</param>
        /// <param name="totalSql">查询数量语句，不需要排序</param>
        /// <param name="paras">参数内容</param>
        /// <returns></returns>
        protected virtual async Task<PageListMo<TType>> GetPageList(string selectSql, string totalSql,
            object paras)
        {
            return await ExcuteReadeResAsync(async con =>
            {
                var total = await con.ExecuteScalarAsync<long>(totalSql, paras);
                if (total <= 0) return new PageListMo<TType>(ResultTypes.ObjectNull, "没有查到相关信息！");

                var list = await con.QueryAsync<TType>(selectSql, paras);
                return new PageListMo<TType>(total, list.ToList());
            });
        }



        /// <summary>
        /// 软删除，仅仅修改  status = CommonStatus.Delete 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual  Task<ResultMo> SoftDeleteById(string id)
        {
            var sql = string.Concat("UPDATE ", m_TableName, " SET status=@status WHERE id=@id");
            var dirPara = new Dictionary<string, object> {{"@id", id}, {"@status", (int) CommonStatus.Delete}};

            return SoftDelete(sql, dirPara);
        }

        /// <summary>
        /// 软删除，直接修改状态
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        protected virtual Task<ResultMo> SoftDelete(string sql,object paras)
        {
            return ExcuteWriteAsync(async con =>
            {
                var rows = await con.ExecuteAsync(sql, paras);
                return rows > 0 ? new ResultMo() : new ResultMo(ResultTypes.UpdateFail, "soft delete Failed!");
            });
        }

        #endregion


        #region 单例模块

        private static readonly object _lockObj = new object();

        private static TRep _instance;

        /// <summary>
        ///   接口请求实例  
        ///  当 DefaultConfig 设值之后，可以直接通过当前对象调用
        /// </summary>
        public static TRep Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (_lockObj)
                {
                    if (_instance == null)
                        // ReSharper disable once PossibleMultipleWriteAccessInDoubleCheckLocking
                        _instance = new TRep();
                }
                return _instance;
            }

        }

        #endregion

    }



}






