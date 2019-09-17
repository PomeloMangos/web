using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pomelo.WoW.Web.Models;
using MySql.Data.MySqlClient;
using Dapper;

namespace Pomelo.WoW.Web.Lib
{
    public static class CharactorCollector
    {
        public static async Task<IEnumerable<Character>> FindCharactersAsync(int accountId)
        {
            var ret = new List<Character>();
            foreach(var x in Character.GetCharacterDbs())
            {
                ret.AddRange(await FindCharactersFromRealm(accountId, x.GetConn()));
            }
            return ret;
        }

        private static async Task<IEnumerable<Character>> FindCharactersFromRealm(int accountId, MySqlConnection conn)
        {
            try
            {
                var query = await conn.QueryAsync<Character>(
                    "SELECT * FROM `characters` " +
                    "WHERE `account` = @account;", 
                    new { account = accountId });
                return query.ToList();
            }
            finally
            {
                conn.Dispose();
            }
        }
    }
}
