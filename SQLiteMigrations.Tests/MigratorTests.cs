//
//  MigratorTests.cs
//
//  Author:
//       Jonatan Cardona Casas <jace.casas@gmail.com>
//
//  Copyright (c) 2017 
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SQLitePCL.pretty;

namespace SQLiteMigrations.Tests
{
    [TestFixture]
    public class MigratorTests
    {
        string temp_path;

        [SetUp]
        protected void SetUp()
        {
            SQLitePCL.Batteries_V2.Init();

            temp_path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(temp_path);
        }

        [TearDown]
        protected void TearDown()
        {
            Directory.Delete(temp_path, true);
        }

        [Test]
        public void DatabaseFileCreated()
        {
            var workDirPath = Path.Combine(temp_path, "database_file_created");
            Directory.CreateDirectory(workDirPath);

            var DBPath = Path.Combine(workDirPath, "db.sqlite3");

            var config = new Configuration
            {
                DBPath = DBPath
            };

            var migrator = new SQLiteMigrator(config);
            migrator.init();

            Assert.True(File.Exists(DBPath), string.Format("Database file should exist"));
        }

        [Test]
        public void MigrationsTableCreated()
        {
            var workDirPath = Path.Combine(temp_path, "migrations_table_created");
            Directory.CreateDirectory(workDirPath);

            var DBPath = Path.Combine(workDirPath, "db.sqlite3");

            var Migrations = new Dictionary<string, string>();

            var config = new Configuration
            {
                DBPath = DBPath,
                MigrationsProvider = new TestMigrationsProvider
                {
                    MigrationsDictionary = Migrations
                }
            };

            var migrator = new SQLiteMigrator(config);
            migrator.init();

            using (var db = SQLite3.Open(DBPath))
            {
                var exists = db.Query("SELECT COUNT(*) FROM sqlite_master WHERE name = ? AND type = ?", "__migrations", "table").SelectScalarInt64().First() > 0;
                Assert.True(exists, "Table __migrations should exist");
            }
        }

        [Test]
        public void MigrationIdSetted()
        {
            var workDirPath = Path.Combine(temp_path, "migration_id_setted");
            Directory.CreateDirectory(workDirPath);

            var DBPath = Path.Combine(workDirPath, "db.sqlite3");

            var Migrations = new Dictionary<string, string>();
            Migrations["123_inicial.sql"] = string.Join(
                "\n",
                new string[]
                {
                    "--- database id 123",
                    "--- database up",
                    "SELECT 'a'",
                    "--- database down",
                    "SELECT 'a'"
                }
            );

            var config = new Configuration
            {
                DBPath = DBPath,
                MigrationsProvider = new TestMigrationsProvider
                {
                    MigrationsDictionary = Migrations
                }
            };

            var migrator = new SQLiteMigrator(config);
            migrator.init();

            using (var db = SQLite3.Open(DBPath))
            {
                var current = db.Query("SELECT id FROM __migrations ORDER BY id DESC LIMIT 1").SelectScalarInt64().First();
                Assert.AreEqual(current, 123, "Current database version should be 123");
            }
        }

        [Test]
        public void MultipleMigrationsCanCreateAndPopulateDatabase()
        {
            var workDirPath = Path.Combine(temp_path, "multiple_migrations_can_create_and_populate_database");
            Directory.CreateDirectory(workDirPath);

            var DBPath = Path.Combine(workDirPath, "db.sqlite3");

            var Migrations = new Dictionary<string, string>();
            Migrations["1_create.sql"] = string.Join(
                "\n",
                new string[]
                {
                    "--- database id 1",
                    "--- database up",
                    "CREATE TABLE test (",
                    "    id INT NOT NULL,",
                    "    name VARCHAR(10) NOT NULL",
                    ");",
                    "--- database down",
                    "DROP TABLE test;"
                }
            );
            Migrations["2_populate.sql"] = string.Join(
                "\n",
                new string[]
                {
                    "--- database id 2",
                    "--- database up",
                    "INSERT INTO test",
                    "    (id, name)",
                    "VALUES",
                    "    (1, 'Jacinto'),",
                    "    (2, 'Susana');",
                    "--- database down",
                    "DELETE FROM test WHERE id IN (1, 2);"
                }
            );

            var config = new Configuration
            {
                DBPath = DBPath,
                MigrationsProvider = new TestMigrationsProvider
                {
                    MigrationsDictionary = Migrations
                }
            };

            var migrator = new SQLiteMigrator(config);
            migrator.init();

            using (var db = SQLite3.Open(DBPath))
            {
                var data = db.Query("SELECT id, name FROM test ORDER BY id ASC")
                             .Select((arg) => new Tuple<int, string>(arg[0].ToInt(), arg[1].ToString()))
                             .ToArray();

                Assert.AreEqual(data.Length, 2, "There should be 2 rows in the table");

                var jacinto = data[0];
                var susana = data[1];

                Assert.AreEqual(jacinto.Item1, 1, "Jacinto's ID should be 1");
                Assert.AreEqual(jacinto.Item2, "Jacinto", "Jacinto's name should be Jacinto");

                Assert.AreEqual(susana.Item1, 2, "Susana's ID should be 2");
                Assert.AreEqual(susana.Item2, "Susana", "Susana's name should be Susana");
            }
        }

        [Test]
        public void TimestampIdFormatWorks()
        {
            var workDirPath = Path.Combine(temp_path, "timestamp_id_format_works");
            Directory.CreateDirectory(workDirPath);

            var DBPath = Path.Combine(workDirPath, "db.sqlite3");

            var Migrations = new Dictionary<string, string>();
            Migrations["201704080412_inicial.sql"] = string.Join(
                "\n",
                new string[]
                {
                    "--- database id 201704080412",
                    "--- database up",
                    "SELECT 'a'",
                    "--- database down",
                    "SELECT 'a'"
                }
            );

            var config = new Configuration
            {
                DBPath = DBPath,
                MigrationsProvider = new TestMigrationsProvider
                {
                    MigrationsDictionary = Migrations
                }
            };

            var migrator = new SQLiteMigrator(config);
            migrator.init();

            using (var db = SQLite3.Open(DBPath))
            {
                var current = db.Query("SELECT id FROM __migrations ORDER BY id DESC LIMIT 1").SelectScalarInt64().First();
                Assert.AreEqual(current, 201704080412, "Current database version should be 201704080412");
            }
        }
    }
}
