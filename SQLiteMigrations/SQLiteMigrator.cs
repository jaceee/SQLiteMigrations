//
//  SQLiteMigrator.cs
//
//  Author:
//       Jonatan Cardona Casas <jace.casas@gmail.com>
//
//  Copyright (c) 2017 
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SQLitePCL.pretty;

namespace SQLiteMigrations
{
    /// <summary>
    /// SQLite database migrator.
    /// </summary>
    public class SQLiteMigrator
    {

        #region Constants

        static readonly string logTag = typeof(SQLiteMigrator).FullName;

        #endregion


        #region Properties

        Configuration Config { get; set; }
        object Locker { get; set; } = new object();

        #endregion


        #region Public API

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SQLiteMigrations.SQLiteMigrator"/> class.
        /// </summary>
        /// <param name="config">Migration process configuration.</param>
        public SQLiteMigrator(Configuration config)
        {
            Config = config;
        }

        /// <summary>
        /// Initializes the database.
        /// </summary>
        public void init()
        {
            try
            {
                using (var db = openConnection())
                {
                    // just to make sure it creates the file
                }

                if (Config.MigrationsProvider != null)
                {
                    runMigrations();
                }
            }
            catch (Exception ex)
            {
                trace(string.Format("Imposible inicializar la base de datos. Motivo: `{0}`", ex.Message));
            }
        }

        #endregion


        #region Migration process

        bool migrationsTableExists(SQLiteDatabaseConnection db)
        {
            var sql = "SELECT COUNT(*) FROM sqlite_master WHERE name = '__migrations' AND type = 'table'";
            var result = db.Query(sql).SelectScalarInt().FirstOrDefault();

            return result > 0;
        }

        void createMigrationsTable(SQLiteDatabaseConnection db)
        {

            var ceate_sql = "CREATE TABLE __migrations ( id INT NOT NULL )";
            db.Execute(ceate_sql);

            var insert_sql = "INSERT INTO __migrations ( id ) VALUES ( 0 )";
            db.Execute(insert_sql);
        }

        long currentMigration(SQLiteDatabaseConnection db)
        {
            var sql = "SELECT id FROM __migrations ORDER BY id DESC LIMIT 1";
            var result = db.Query(sql).SelectScalarInt64().FirstOrDefault();

            return result;
        }

        SQLiteDatabaseConnection openConnection()
        {
            var db = SQLite3.Open(Config.DBPath);

            db.Profile += (sender, e) =>
            {
                trace(string.Format("TIME: {0} - SQL: {1}", e.ExecutionTime.ToString(), e.Statement));
            };

            return db;
        }

        void runMigrations()
        {
            using (var db = openConnection())
            {
                if (!migrationsTableExists(db))
                {
                    createMigrationsTable(db);
                }

                var current = currentMigration(db);

                Func<string, bool> isValid = p =>
                {
                    var matches = Regex.Matches(p, @"(\d+)(?:_.+)?(?:\.sql)", RegexOptions.IgnoreCase);
                    if (matches.Count > 0)
                    {
                        return long.Parse(matches[0].Groups[1].Value) > current;
                    }

                    return false;
                };

                var migrations = Config.MigrationsProvider
                                       .MigrationList
                                       .Where(isValid)
                                       .OrderBy(p => p)
                                       .ToArray();

                foreach (var migration in migrations)
                {
                    trace(string.Format("Ejecutando migración `{0}`", migration));

                    if (!runMigration(db, migration))
                    {
                        throw new Exception("Error executing some migrations.");
                    }
                }
            }
        }

        bool runMigration(SQLiteDatabaseConnection db, string name)
        {
            var migration = Config.MigrationsProvider.GetMigration(name);

            var up = new List<string>();
            var down = new List<string>();

            long id = 0;
            var state = 0;

            foreach (var line in migration.Split('\n'))
            {
                var id_match = Regex.Match(line, @"(?:--- database id )(\d+)");
                if (id_match.Success)
                {
                    id = long.Parse(id_match.Groups[1].Value);
                    continue;
                }

                if (Regex.IsMatch(line, @"--- database up"))
                {
                    state = 1;
                    continue;
                }

                if (Regex.IsMatch(line, @"--- database down"))
                {
                    state = 2;
                    continue;
                }

                var sql = line.Split(new char[] { '-', '-' })[0].Trim();

                switch (state)
                {
                    case 1:
                        up.Add(sql);
                        break;

                    case 2:
                        down.Add(sql);
                        break;
                }
            }

            if (id == 0)
            {
                throw new Exception("No id defined for the migration.");
            }

            var migration_sql = string.Join(" ", up).Trim();
            if (string.IsNullOrEmpty(migration_sql))
            {
                throw new Exception("Empty migration.");
            }

            var id_sql = string.Format("INSERT INTO __migrations ( id ) VALUES ( {0} )", id);

            return db.TryRunInTransaction(
                (t) =>
                {
                    t.ExecuteAll(migration_sql);
                    t.Execute(id_sql);
                }
            );
        }

        #endregion


        #region Tracing

        void trace(string log)
        {
            if (Config.Logger != null)
                Config.Logger.Log(string.Format("[{0}]: {1}", logTag, log));
        }

        #endregion

    }
}
