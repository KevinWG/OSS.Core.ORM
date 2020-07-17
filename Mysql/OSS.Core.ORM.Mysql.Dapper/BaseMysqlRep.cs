﻿#region Copyright (C) 2017 Kevin (OSS开源实验室) 公众号：osscoder

/***************************************************************************
*　　	文件功能描述：OSSCore仓储层 —— 仓储基类
*
*　　	创建人： Kevin
*       创建人Email：1985088337@qq.com
*    	创建日期：2017-4-21
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
using MySql.Data.MySqlClient;
using OSS.Common.BasicImpls;
using OSS.Common.BasicMos;
using OSS.Common.BasicMos.Enums;
using OSS.Common.BasicMos.Resp;
using OSS.Core.ORM.Mysql.Dapper.OrmExtension;
using OSS.Tools.Log;

namespace OSS.Core.ORM.Mysql.Dapper
{
    /// <summary>
    /// 仓储层基类
    /// </summary>
    public abstract class BaseMysqlRep<TRep, TType, IdType> : SingleInstance<TRep>
        where TRep : class, new()
        where TType : BaseMo<IdType>, new()
    {
        /// <summary>
        ///  仓储表名
        /// </summary>
        public string TableName => GetTableName();

        /// <summary>
        /// 获取仓储表名
        ///  便于分表时按需扩展
        /// </summary>
        /// <returns></returns>
        protected abstract string GetTableName();

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns></returns>
        protected abstract MySqlConnection GetDbConnection(bool isWriteOperate);

        #region Add

        /// <summary>
        ///   插入数据
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        public virtual async Task<IdResp<IdType>> Add(TType mo)
        {
            var res = await ExecuteWriteAsync(async con =>
            {
                var row = await con.Insert(TableName, mo);
                return row > 0 ? new IdResp<IdType>() : new IdResp<IdType>().WithResp(RespTypes.OperateFailed, "添加失败!");
            });
            if (res.IsSuccess())
            {
                res.id = mo.id;
            }

            return res;
        }

        #endregion

        #region Update 

        /// <summary>
        /// 部分字段的更新
        /// </summary>
        ///  <param name="updateExp">更新字段,示例：
        ///  u=>new{mo.Name,....} Or u=> new{ Name="",....}</param>
        /// <param name="whereExp">判断条件 示例：
        /// w=>w.id==1  , 如果为空默认根据Id判断</param>
        /// <param name="mo"></param>
        /// <returns></returns>
        protected virtual Task<Resp> Update(Expression<Func<TType, object>> updateExp,
            Expression<Func<TType, bool>> whereExp, object mo = null)
            => ExecuteWriteAsync(con => con.UpdatePartial(TableName, updateExp, whereExp, mo));

        /// <summary>
        ///  直接使用语句更新操作
        /// </summary>
        /// <param name="updateSql"></param>
        /// <param name="whereSql"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        protected virtual Task<Resp> Update(string updateSql, string whereSql, object para = null)
            => ExecuteWriteAsync(async con =>
            {
                var sql = string.Concat("UPDATE ", TableName, " SET ", updateSql, whereSql);
                var row = await con.ExecuteAsync(sql, para);
                return row > 0 ? new Resp() : new Resp().WithResp(ret: RespTypes.OperateFailed, "更新失败");
            });

        #endregion

        #region Delete

        /// <summary>
        /// 软删除，仅仅修改  status = CommonStatus.Delete 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Task<Resp> SoftDeleteById(string id)
        {
            var sql     = string.Concat("UPDATE ", TableName, " SET status=@status WHERE id=@id");
            var dirPara = new Dictionary<string, object> {{"@id", id}, {"@status", (int) CommonStatus.Delete}};

            return SoftDelete(sql, dirPara);
        }

        /// <summary>
        /// 软删除，直接修改  status = CommonStatus.Delete 
        /// </summary>
        /// <param name="whereExp">条件表达式</param>
        /// <returns></returns>
        protected virtual Task<Resp> SoftDelete(Expression<Func<TType, bool>> whereExp)
        {
            return Update(m => new {status = CommonStatus.Delete}, whereExp);
        }

        /// <summary>
        /// 软删除，直接修改状态
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        protected virtual Task<Resp> SoftDelete(string sql, object paras)
        {
            return ExecuteWriteAsync(async con =>
            {
                var rows = await con.ExecuteAsync(sql, paras);
                return rows > 0 ? new Resp() : new Resp().WithResp(RespTypes.OperateFailed, "soft delete Failed!");
            });
        }


        #endregion

        #region Get


        /// <summary>
        /// 通过id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual Task<Resp<TType>> GetById(string id)
        {
            const string whereSql = " WHERE id=@id";
            var dirPara = new Dictionary<string, object> {{"@id", id}};

            return Get(whereSql, dirPara);
        }

        /// <summary>
        ///  获取单个实体对象
        /// </summary>
        /// <param name="whereExp">判断条件，如果为空默认根据Id判断</param>
        /// <returns></returns>
        protected Task<Resp<TType>> Get(Expression<Func<TType, bool>> whereExp)
            => ExecuteReadAsync(con => con.Get(TableName, whereExp));

        /// <summary>
        /// 通过sql语句获取实体
        /// </summary>
        /// <param name="whereSql"> 条件sql语句</param>
        /// <param name="para"></param>
        /// <returns></returns>
        protected virtual Task<Resp<TType>> Get(string whereSql, object para)
        {
            var sql = string.Concat("select * from ", TableName, " ", whereSql);
            return ExecuteReadAsync(con => con.QuerySingleOrDefaultAsync<TType>(sql, para));
        }

        #endregion

        #region Get（Page）List

        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="whereExp"></param>
        /// <returns></returns>
        protected virtual Task<ListResp<TType>> GetList(Expression<Func<TType, bool>> whereExp)
            => ExecuteReadSubAsync(con => con.GetList(TableName, whereExp));

        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="getSql">查询语句</param>
        /// <param name="paras">参数内容</param>
        /// <returns></returns>
        protected Task<ListResp<TType>> GetList(string getSql,object paras)
        {
            return GetList<TType>(getSql, paras);
        }

        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="getSql">查询语句</param>
        /// <param name="paras">参数内容</param>
        /// <returns></returns>
        protected virtual async Task<ListResp<RType>> GetList<RType>(string getSql,
            object paras)
        {
            return await ExecuteReadSubAsync(async con =>
            {
                var list = (await con.QueryAsync<RType>(getSql, paras))?.ToList();

                return list?.Count > 0
                    ? new ListResp<RType>(list)
                    : new ListResp<RType>().WithResp(RespTypes.ObjectNull, "没有查到相关信息！");
            });
        }

        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="selectSql">查询语句，包含排序等</param>
        /// <param name="totalSql">查询数量语句，不需要排序,如果为空，则不计算和返回总数信息</param>
        /// <param name="paras">参数内容</param>
        /// <returns></returns>
        protected  Task<PageListResp<TType>> GetPageList(string selectSql, object paras,
            string totalSql = null)
        {
            return GetPageList<TType>(selectSql, paras, totalSql);
        }
        /// <summary>
        ///   列表查询
        /// </summary>
        /// <param name="selectSql">查询语句，包含排序等</param>
        /// <param name="totalSql">查询数量语句，不需要排序,如果为空，则不计算和返回总数信息</param>
        /// <param name="paras">参数内容</param>
        /// <returns></returns>
        protected virtual async Task<PageListResp<RType>> GetPageList<RType>(string selectSql, object paras,
            string totalSql = null)
        {
            return await ExecuteReadSubAsync(async con =>
            {
                long total = 0;

                if (!string.IsNullOrEmpty(totalSql))
                {
                    total = await con.ExecuteScalarAsync<long>(totalSql, paras);
                    if (total <= 0) return new PageListResp<RType>().WithResp(RespTypes.ObjectNull, "没有查到相关信息！");
                }

                var list = await con.QueryAsync<RType>(selectSql, paras);
                return new PageListResp<RType>(total, list.ToList());
            });
        }

        #endregion

        #region 底层基础读写分离操作封装

        /// <summary>
        /// 执行写数据库操作
        /// </summary>
        /// <typeparam name="RespType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected Task<RespType> ExecuteWriteAsync<RespType>(Func<IDbConnection, Task<RespType>> func)
            where RespType : Resp, new()
            => ExecuteAsync(func, true);

        /// <summary>
        ///  执行读操作，返回具体类型，自动包装成Resp结果实体
        /// </summary>
        /// <typeparam name="RespParaType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected Task<Resp<RespParaType>> ExecuteReadAsync<RespParaType>(Func<IDbConnection, Task<RespParaType>> func)
            => ExecuteAsync(async con =>
            {
                var res = await func(con);
                return res != null
                    ? new Resp<RespParaType>(res)
                    : new Resp<RespParaType>().WithResp(RespTypes.ObjectNull, "未发现相关数据！");
            }, false);

        /// <summary>
        /// 执行读操作，直接返回继承自Resp实体
        /// </summary>
        /// <typeparam name="SubRespType"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected async Task<SubRespType> ExecuteReadSubAsync<SubRespType>(Func<IDbConnection, Task<SubRespType>> func)
            where SubRespType : Resp, new()
            => await ExecuteAsync(func, false);

        private async Task<RType> ExecuteAsync<RType>(Func<IDbConnection, Task<RType>> func, bool isWrite)
            where RType : Resp, new()
        {
            RType t;
            try
            {
                using (var con = GetDbConnection(isWrite))
                {
                    t = await func(con);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(string.Concat("数据库操作错误,仓储表名：", TableName, "，详情：", e.Message, "\r\n", e.StackTrace),
                    "DataRepConnectionError",
                    "DapperRep_Mysql");
                t = new RType
                {
                    ret = (int) RespTypes.InnerError,
                    msg = isWrite ? "数据操作出错！" : "数据读取错误"
                };
            }

            return t ?? new RType() {ret = (int) RespTypes.ObjectNull, msg = "未发现对应结果"};
        }

        #endregion

    }

}






