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
    /// Configuration for the database migration process.
    /// </summary>
	public class Configuration
    {
        /// <summary>
        /// Gets or sets the database file path.
        /// </summary>
        /// <value>The database file path.</value>
        public string DBPath { get; set; }
		
        /// <summary>
        /// Gets or sets the migrations provider.
        /// </summary>
        /// <value>The migrations provider.</value>
        public IMigrationsProvider MigrationsProvider { get; set; }
   
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public IMigrationLogger Logger { get; set; }
	}
}