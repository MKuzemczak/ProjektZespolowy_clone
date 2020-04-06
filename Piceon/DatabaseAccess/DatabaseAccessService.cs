using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace Piceon.DatabaseAccess
{
    [Serializable]
    public class AlreadyHasParentException : Exception
    {
        public AlreadyHasParentException() { }
        public AlreadyHasParentException(string message) : base(message) { }
        public AlreadyHasParentException(string message, Exception inner) : base(message, inner) { }
        protected AlreadyHasParentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    public static class DatabaseAccessService
    {
        public static bool Initialized = false;

        public static SqliteConnection Database;

        public async static Task InitializeDatabaseAsync()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            Database = new SqliteConnection($"Filename={dbpath}");
            
            Database.Open();

            using (SqliteCommand command = new SqliteCommand("PRAGMA recursive_triggers = ON", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS IMAGE" +
                        "(Id INTEGER PRIMARY KEY NOT NULL, path text NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS VIRTUALFOLDER" +
                        "(Id INTEGER PRIMARY KEY NOT NULL, name text NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS VIRTUALFOLDER_IMAGE" +
                        "(IMAGE_Id REFERENCES IMAGE(Id) NOT NULL, VIRTUALFOLDER_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS SIMILAR_IMAGES" +
                        "(FIRST_IMAGE_Id REFERENCES IMAGE(Id)NOT NULL, SECOND_IMAGE_Id REFERENCES IMAGE(Id)NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS VIRTUALFOLDER_RELATION" +
                        "(PARENT_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL, CHILD_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS ACCESSEDFOLDER" +
                        "(Id INTEGER PRIMARY KEY NOT NULL, token text NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS path_validator " +
                    "BEFORE INSERT ON IMAGE " +
                    "BEGIN " +
                        "SELECT " +
                        "CASE " +
                        "WHEN NEW.path NOT LIKE '%_\\_%' THEN " +
                        "RAISE(ABORT, 'Invalid path') " +
                        "END; " +
                    "END;", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_image_delete " +
                    "AFTER DELETE ON IMAGE " +
                    "BEGIN " +
                        "DELETE FROM VIRTUALFOLDER_IMAGE WHERE IMAGE_Id = OLD.Id; " +
                        "DELETE FROM SIMILAR_IMAGES WHERE FIRST_IMAGE_Id = OLD.Id; " +
                        "DELETE FROM SIMILAR_IMAGES WHERE SECOND_IMAGE_Id = OLD.Id; " +
                    "END ; ", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_virtualfolder_delete " +
                    "AFTER DELETE ON VIRTUALFOLDER " +
                    "BEGIN " +
                        "DELETE FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id = OLD.Id; " +
                        "DELETE FROM VIRTUALFOLDER_RELATION WHERE CHILD_Id = OLD.Id; " +
                        "DELETE FROM VIRTUALFOLDER WHERE Id IN (SELECT CHILD_Id FROM VIRTUALFOLDER_RELATION WHERE PARENT_Id = OLD.Id); " +
                    "END; ", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS block_imageid_change " +
                    "BEFORE UPDATE OF Id ON IMAGE " +
                    "FOR EACH ROW " +
                    "BEGIN " +
                        "SELECT RAISE(ABORT, 'NIE ZMIENIAJ ID OBRAZU'); " +
                    "END;", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS block_virtualfolderid_change " +
                    "BEFORE UPDATE OF Id ON VIRTUALFOLDER " +
                    "FOR EACH ROW " +
                    "BEGIN " +
                        "SELECT RAISE(ABORT, 'NIE ZMIENIAJ ID FOLDERU'); " +
                    "END; ", Database))
            { await command.ExecuteReaderAsync(); }
            

            Initialized = true;
        }

        public static async Task<List<DatabaseVirtualFolder>> GetRootVirtualFoldersAsync()
        {
            List<DatabaseVirtualFolder> result = new List<DatabaseVirtualFolder>();
            
            SqliteCommand selectCommand = new SqliteCommand
                ("SELECT * FROM VIRTUALFOLDER WHERE Id NOT IN (SELECT CHILD_Id FROM VIRTUALFOLDER_RELATION)", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();


            while (query.Read())
            {
                DatabaseVirtualFolder folder = new DatabaseVirtualFolder()
                {
                    Id = query.GetInt32(0),
                    Name = query.GetString(1)
                };
                    
                result.Add(folder);
            }

            return result;
        }

        public static async Task<List<DatabaseVirtualFolder>> GetChildrenOfFolderAsync(string id)
        {
            List<DatabaseVirtualFolder> result = new List<DatabaseVirtualFolder>();

            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM VIRTUALFOLDER WHERE Id IN (SELECT CHILD_Id FROM VIRTUALFOLDER_RELATION WHERE PARENT_Id={id})", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();


            while (query.Read())
            {
                DatabaseVirtualFolder folder = new DatabaseVirtualFolder()
                {
                    Id = query.GetInt32(0),
                    Name = query.GetString(1)
                };

                result.Add(folder);
            }

            return result;
        }

        public static async Task<List<DatabaseVirtualFolder>> GetChildrenOfFolderAsync(int id)
        {
            return await GetChildrenOfFolderAsync(id.ToString());
        }

        public static async Task<List<DatabaseVirtualFolder>> GetChildrenOfFolderAsync(DatabaseVirtualFolder folder)
        {
            return await GetChildrenOfFolderAsync(folder.Id);
        }

        public static async Task<DatabaseVirtualFolder> GetParentOfFolderAsync(string id)
        {
            DatabaseVirtualFolder result = new DatabaseVirtualFolder();
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM VIRTUALFOLDER WHERE Id IN (SELECT PARENT_Id FROM VIRTUALFOLDER_RELATION WHERE CHILD_Id={id})", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            if (!query.HasRows)
            {
                return null;
            }

            while (query.Read())
            {
                DatabaseVirtualFolder folder = new DatabaseVirtualFolder()
                {
                    Id = query.GetInt32(0),
                    Name = query.GetString(1)
                };

                result = folder;
            }

            return result;
        }

        public static async Task<DatabaseVirtualFolder> GetParentOfFolderAsync(int id)
        {
            return await GetParentOfFolderAsync(id.ToString());
        }

        public static async Task<DatabaseVirtualFolder> GetParentOfFolderAsync(DatabaseVirtualFolder folder)
        {
            return await GetParentOfFolderAsync(folder.Id);
        }

        public static async Task<List<string>> GetImagesInFolderAsync(string id)
        {
            var result = new List<string>();

            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM IMAGE WHERE Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={id})", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();


            while (query.Read())
            {
                result.Add(query.GetString(1));
            }

            return result;
        }

        public static async Task<List<string>> GetImagesInFolderAsync(int id)
        {
            return await GetImagesInFolderAsync(id.ToString());
        }

        public static async Task<List<string>> GetImagesInFolderAsync(DatabaseVirtualFolder folder)
        {
            return await GetImagesInFolderAsync(folder.Id);
        }

        public static async Task<int> GetImagesCountInFolderAsync(string id)
        {
            int result = -1;
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT COUNT(*) FROM IMAGE WHERE Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={id})", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                result = query.GetInt32(0);
            }

            return result;
        }

        public static async Task<int> GetImagesCountInFolderAsync(int id)
        {
            return await GetImagesCountInFolderAsync(id.ToString());
        }

        public static async Task<int> GetImagesCountInFolderAsync(DatabaseVirtualFolder folder)
        {
            return await GetImagesCountInFolderAsync(folder.Id);
        }

        public static async Task AddAccessedFolderAsync(string token)
        {
            using (SqliteCommand command = new SqliteCommand("INSERT INTO ACCESSEDFOLDER (token) " +
                $"VALUES ('{token}')", Database))
            { await command.ExecuteReaderAsync(); }        
        }

        public static async Task<List<string>> GetAccessedFoldersAsync()
        {
            var result = new List<string>();
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT token FROM ACCESSEDFOLDER", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (await query.ReadAsync())
            {
                result.Add(query.GetString(0));
            }

            return result;
        }

        public static async Task<DatabaseVirtualFolder> AddVirtualFolderAsync(string name, int parentId = -1)
        {
            DatabaseVirtualFolder result = new DatabaseVirtualFolder()
            {
                Name = name
            };
            using (SqliteCommand command = new SqliteCommand("INSERT INTO VIRTUALFOLDER (name) " +
                        $"VALUES ('{name}')", Database))
            { await command.ExecuteReaderAsync(); }

            Int64 rowid = 0;

            using (SqliteCommand command = new SqliteCommand("SELECT last_insert_rowid()", Database))
            { rowid = (Int64)await command.ExecuteScalarAsync(); }

            if (parentId > -1)
            {
                using (SqliteCommand command = new SqliteCommand("INSERT INTO VIRTUALFOLDER_RELATION (PARENT_Id, CHILD_Id) " +
                    $"VALUES ({parentId}, {rowid})", Database))
                { rowid = (Int64)await command.ExecuteScalarAsync(); }
            }

            result.Id = (int)rowid;

            return result;
        }

        public static async Task RenameVirtualFolderAsync(int id, string newName)
        {
            using (SqliteCommand command = new SqliteCommand("UPDATE VIRTUALFOLDER " +
                        $"SET name = '{newName}' " +
                        $"WHERE Id = {id}", Database))
            { await command.ExecuteReaderAsync(); }
        }

        public static async Task SetParentOfFolderAsync(int childId, int parentId)
        {
            using (SqliteCommand command = new SqliteCommand("SELECT CHILD_Id FROM VIRTUALFOLDER_RELATION " +
                    $"WHERE CHILD_Id={childId}", Database))
            {
                var query = await command.ExecuteReaderAsync();
                if (query.HasRows)
                {
                    throw new AlreadyHasParentException();
                }
            }

            using (SqliteCommand command = new SqliteCommand("INSERT INTO VIRTUALFOLDER_RELATION (PARENT_Id, CHILD_Id) " +
                    $"VALUES ({parentId}, {childId})", Database))
            { await command.ExecuteReaderAsync(); }
        }

        public static async Task DeleteVirtualFolderAsync(int id)
        {
            using (SqliteCommand command = new SqliteCommand($"DELETE FROM VIRTUALFOLDER WHERE Id = {id}", Database))
            { await command.ExecuteReaderAsync(); }
        }
    }
}
