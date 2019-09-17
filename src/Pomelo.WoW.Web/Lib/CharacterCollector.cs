using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pomelo.WoW.Web.Models;
using MySql.Data.MySqlClient;
using Dapper;

namespace Pomelo.WoW.Web.Lib
{
    public static class CharacterCollector
    {
        public static async Task<IEnumerable<Character>> FindCharactersAsync(ulong accountId)
        {
            var ret = new List<Character>();
            foreach(var x in Character.GetCharacterDbs())
            {
                var characters = await FindCharactersFromRealm(accountId, x.GetConn());
                foreach (var y in characters)
                {
                    y.RealmId = x.RealmId.Value;
                }
                ret.AddRange(characters);
            }
            return ret;
        }

        public static async Task<Character> GetCharacterAsync(uint realmId, uint characterId)
        {
            using (var conn = Character.GetCharacterDb(realmId))
            {
                var query = await conn.QueryAsync<Character>(
                    "SELECT * FROM `characters` WHERE `guid` = @characterId",
                    new { characterId });
                var ret = query.SingleOrDefault();
                ret.RealmId = realmId;
                return ret;
            }
        }

        private static async Task<IEnumerable<Character>> FindCharactersFromRealm(ulong accountId, MySqlConnection conn)
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
