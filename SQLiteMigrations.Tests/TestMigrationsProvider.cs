//
//  TestMigrationsProvider.cs
//
//  Author:
//       Jonatan Cardona Casas <jace.casas@gmail.com>
//
//  Copyright (c) 2017 
//

using System.Collections.Generic;
using System.Linq;

namespace SQLiteMigrations.Tests
{

    class TestMigrationsProvider : IMigrationsProvider
    {
		public Dictionary<string, string> MigrationsDictionary = new Dictionary<string, string>();

        public string[] MigrationList
        {
            get
            {
                return MigrationsDictionary.Keys.ToArray();
            }
        }

        public string GetMigration(string name)
        {
            return MigrationsDictionary[name];
        }
    }
}
