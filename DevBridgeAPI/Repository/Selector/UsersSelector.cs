﻿using DevBridgeAPI.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DevBridgeAPI.Repository.Selector
{
    public class UsersSelector : IModelSelector
    {
        public IEnumerable<IModel> SelectAllRows()
        {
            string sql = "SELECT * FROM Users";
            using (var db = new DbContext().Connection)
            {
                //Example parametrized query:
                //sql = "SELECT * FROM Users WHERE UserId = @UserId";
                //db.Connection.Query<User>(sql, new {UserId = 2});
                return db.Query<User>(sql);
            }
        }

        public IModel SelectOneRow(string username, string password)
        {
            string sql = "SELECT * FROM Users WHERE Email = @username AND Password = @password";
            using (var db = new DbContext().Connection)
            {
                return db.Query<User>(sql, new { username, password }).FirstOrDefault();
            }
        }
    }
}