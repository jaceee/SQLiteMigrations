# SQLiteMigrations

**This library is in development stage. It does not yet implement every planned feature.
Right now it only implements key features I needed for a project.**

SQLiteMigrations is a .NET Portable Class Library (PCL) to enable database schema management for
[SQLitePCL.raw](https://github.com/ericsink/SQLitePCL.raw) built on top of
[SQLitePCL.pretty](https://github.com/bordoley/SQLitePCL.pretty).

This project was inspired by [SQLiteMigrationManager.swift](https://github.com/garriguv/SQLiteMigrationManager.swift) (from which I borrowed the README structure)
and [goose](https://bitbucket.org/liamstask/goose).

## Concept

SQLiteMigrations works by introducing a `__migrations` table into the database:

```sql
CREATE TABLE __migrations (
  id INT NOT NULL
);
```

Each row in `__migrations` corresponds to a single migration that has been applied and represents a unique version of
the schema. This schema supports any versioning scheme that is based on integers, but it is recommended that you utilize
an integer that encodes a timestamp.

## Usage

Have a look at the [tests](https://github.com/jaceee/SQLiteMigrations/tree/master/SQLiteMigrations.Tests).

### Initializing the database

You would first have to implement the interface `IMigrationsProvider` in order to provide the migrations to the migrator.
When you are done with that you can define your configuration instance.

```csharp
var config = new Configuration
{
  DBPath = DBPath,
  MigrationsProvider = new MigrationsProviderImplementation()
};
```

You can also provide a `IMigrationsLogger` implementation for logging of profiling information on each executed statement.

From there the process is really simple.

```csharp
var migrator = new SQLiteMigrator(config);
migrator.init();
```

The `init` method call will create the database at the specified path if it does not exist already.
Then it will check for the `__migrations` table and create it if is missing.
It will then check for the current database schema version (initially 0) and retrieve all the migrations with version code
higher than the current database schema version and execute them in order.

### Creating a migration

Create a migration file:

```
$ touch 201704011234_add_phone_field_to_user.sql
```

SQLiteMigrations will only recognize filenames of the form `<version>_<name>.sql`.
The following filenames are valid:

* `1.sql`
* `2_add_new_table.sql`
* `3_add-new-table.sql`
* `4_add new table.sql`

The file must be structured as follows:

```sql
--- database id 201704011234

--- database up

-- Upgrade your database
ALTER TABLE users ADD COLUMN phone VARCHAR(25);

--- database down

-- Downgrade your database
CREATE TABLE users_backup(name VARCHAR(50));
INSERT INTO users_backup SELECT name FROM users;
DROP TABLE users;
ALTER TABLE users_backup RENAME TO users;
```

The migration is ran inside a transaction so it can be rolled back in case of failure.
Be careful not to try to begin a transaction since this would cause an error.

## Installation

### NuGet

To install SQLiteMigrations, run the following command in the Package Manager Console

```
Install-Package MallaCreativa.SQLiteMigrations
```

Or just go to the NuGet package gallery and add it to your project.

## TODO

 * ~~Implement schema upgrading~~
 * Implement schema downgrading
 * Flexibility on schema migration process
 * Information of schema version (and if it has migrations table)

Helping hands are totally wellcome.

## Contributing

1. [Fork it](https://github.com/jaceee/SQLiteMigrations/fork)
2. Create your feature branch (`git checkout -b feature/my-new-feature` or `git checkout -b hotfix/my-hotfix`)
3. Commit your changes (`git commit -am 'Add some feature'` or `git commit -am 'Fix some error'`)
4. Push to the branch (`git push origin feature/my-new-feature` or `git push origin hotfix/my-hotfix`)
5. Create a new Pull Request
6. You're awesome! 👍

You can create the Pull Request before having completed the changes (or even before starting) to request feedback
or ask for help for where to begin, just remember to keep `[WIP]` to let everyone know it is a Work In Progress
before the name until your work is done.

## Author

Jonatan Cardona Casas, [jace.casas@gmail.com](mailto:jace.casas@gmail.com)

## License

Migrations is available under the MIT license. See the LICENSE file for more info.
