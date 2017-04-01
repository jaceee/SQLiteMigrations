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
    /// Migration logger.
    /// </summary>
	public interface IMigrationLogger
	{
        /// <summary>
        /// Log the specified message.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
		void Log(string message);
	}
}