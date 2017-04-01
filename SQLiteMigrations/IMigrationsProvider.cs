//
//  SQLiteMigrator.cs
//
//  Author:
//       Jonatan Cardona Casas <jace.casas@gmail.com>
//
//  Copyright (c) 2017 
//

namespace SQLiteMigrations
{
    /// <summary>
    /// Migrations provider.
    /// </summary>
	public interface IMigrationsProvider
	{
        /// <summary>
        /// Gets the migration list.
        /// </summary>
        /// <value>The migration list.</value>
		string[] MigrationList { get; }

        /// <summary>
        /// Gets the migration for the given name.
        /// </summary>
        /// <returns>The migration.</returns>
        /// <param name="name">The migration name.</param>
        string GetMigration(string name);
	}
}