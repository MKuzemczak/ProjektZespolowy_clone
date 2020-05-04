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

        public static string DatabaseFilePath { get; private set; }

        public async static Task InitializeDatabaseAsync()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            DatabaseFilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            Database = new SqliteConnection($"Filename={DatabaseFilePath}");
            
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
                        "(IMAGE_Id REFERENCES IMAGE(Id) NOT NULL, VIRTUALFOLDER_Id REFERENCES VIRTUALFOLDER(Id) NOT NULL)", Database))
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

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS SIMILARITYGROUP" +
                        "(Id INTEGER PRIMARY KEY NOT NULL, name text NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS SIMILARITYGROUP_IMAGE" +
                        "(SIMILARITYGROUP_Id REFERENCES SIMILARITYGROUP(Id) NOT NULL, IMAGE_Id REFERENCES IMAGE(Id) NOT NULL)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS TAG" +
                        "(Id INTEGER PRIMARY KEY NOT NULL, tag text NOT NULL UNIQUE)", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS IMAGE_TAG" +
                        "(IMAGE_Id REFERENCES IMAGE(Id) NOT NULL UNIQUE, TAG_Id REFERENCES TAG(Id) NOT NULL UNIQUE)", Database))
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
                        "DELETE FROM SIMILARITYGROUP_IMAGE WHERE IMAGE_Id = OLD.Id; " +
                        "DELETE FROM IMAGE_TAG WHERE IMAGE_Id = OLD.Id; " +
                    "END ; ", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_similaritygroup_image_delete " +
                    "AFTER DELETE ON SIMILARITYGROUP_IMAGE " +
                    "BEGIN " +
                        "DELETE FROM SIMILARITYGROUP WHERE Id NOT IN (SELECT SIMILARITYGROUP_Id FROM SIMILARITYGROUP_IMAGE); " +
                    "END ; ", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_image_tag_delete " +
                    "AFTER DELETE ON IMAGE_TAG " +
                    "BEGIN " +
                        "DELETE FROM TAG WHERE Id NOT IN (SELECT TAG_Id FROM IMAGE_TAG); " +
                    "END ; ", Database))
            { await command.ExecuteReaderAsync(); }

            using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_virtualfolder_delete " +
                    "AFTER DELETE ON VIRTUALFOLDER " +
                    "BEGIN " +
                        "DELETE FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id = OLD.Id; " +
                        "DELETE FROM IMAGE WHERE Id NOT IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE); " +
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

        public static async Task<List<Tuple<int, string>>> GetImagesInFolderAsync(string id)
        {
            var result = new List<Tuple<int, string>>();

            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM IMAGE WHERE Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={id})", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();


            while (query.Read())
            {
                result.Add(new Tuple<int, string>(query.GetInt32(0), query.GetString(1)));
            }

            return result;
        }

        public static async Task<List<Tuple<int,string>>> GetImagesInFolderAsync(int id)
        {
            return await GetImagesInFolderAsync(id.ToString());
        }

        public static async Task<List<Tuple<int, string>>> GetImagesInFolderAsync(DatabaseVirtualFolder folder)
        {
            return await GetImagesInFolderAsync(folder.Id);
        }

        public static async Task<int> GetImagesCountInFolderAsync(string id)
        {
            int result = -1;
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT COUNT(*) FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={id}", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

                if (!query.HasRows)
                {
                    throw new SqliteException("SQLite access exception: Something went wrong!", 1);
                }

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

        public static async Task<int> InsertAccessedFolderAsync(string token)
        {
            using (SqliteCommand command = new SqliteCommand("INSERT INTO ACCESSEDFOLDER (token) " +
                $"VALUES ('{token}')", Database))
            { await command.ExecuteReaderAsync(); }

            Int64 rowid = 0;

            using (SqliteCommand command = new SqliteCommand("SELECT last_insert_rowid()", Database))
            { rowid = (Int64)await command.ExecuteScalarAsync(); }

            if (rowid == 0)
            {
                throw new SqliteException("SQLite access exception: Something went wrong!", 1);
            }

            return (int)rowid;
        }

        public static async Task<List<Tuple<int,string>>> GetAccessedFoldersAsync()
        {
            var result = new List<Tuple<int,string>>();
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM ACCESSEDFOLDER", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (await query.ReadAsync())
            {
                var tuple = new Tuple<int, string>(query.GetInt32(0), query.GetString(1));
                result.Add(tuple);
            }

            return result;
        }

        public static async Task DeleteAccessedFolderAsync(int id)
        {
            using (SqliteCommand command = new SqliteCommand($"DELETE FROM ACCESSEDFOLDER WHERE Id = {id}", Database))
            { await command.ExecuteReaderAsync(); }
        }

        public static async Task<DatabaseVirtualFolder> InsertVirtualFolderAsync(string name, int parentId = -1)
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

            if (rowid == 0)
            {
                throw new SqliteException("SQLite access exception: Something went wrong!", 1);
            }
            if (parentId > -1)
            {
                using (SqliteCommand command = new SqliteCommand("INSERT INTO VIRTUALFOLDER_RELATION (PARENT_Id, CHILD_Id) " +
                    $"VALUES ({parentId}, {rowid})", Database))
                { await command.ExecuteReaderAsync(); }
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

        public static async Task<int> InsertImageAsync(string path, int parentId)
        {
            if (parentId < 1)
                throw new ArgumentException("Error: Parent ID smaller than 1 - doesn't exist.");

            using (SqliteCommand command = new SqliteCommand("INSERT INTO IMAGE (path) " +
                        $"VALUES ('{path}')", Database))
            { await command.ExecuteReaderAsync(); }

            Int64 rowid = 0;

            using (SqliteCommand command = new SqliteCommand("SELECT last_insert_rowid()", Database))
            { rowid = (Int64)await command.ExecuteScalarAsync(); }

            if (rowid == 0)
            {
                throw new SqliteException("SQLite access exception: Something went wrong!", 1);
            }

            using (SqliteCommand command = new SqliteCommand("INSERT INTO VIRTUALFOLDER_IMAGE (IMAGE_Id, VIRTUALFOLDER_Id) " +
                $"VALUES ({rowid}, {parentId})", Database))
            { await command.ExecuteReaderAsync(); }

            return (int)rowid;
        }

        public static async Task MoveImageToVirtualfolderAsync(int imageId, int virtualfolderId)
        {
            using (SqliteCommand command = new SqliteCommand($@"DELETE FROM VIRTUALFOLDER_IMAGE
                WHERE IMAGE_Id = {imageId}; 
                INSERT INTO VIRTUALFOLDER_iMAGE (IMAGE_Id, VIRTUALFOLDER_Id)
                VALUES ({imageId}, {virtualfolderId})", Database))
            { await command.ExecuteReaderAsync(); }
        }

        public static async Task DeleteImageAsync(int imageId)
        {
            using (SqliteCommand command = new SqliteCommand($"DELETE FROM IMAGE WHERE Id = {imageId}", Database))
            { await command.ExecuteReaderAsync(); }
        }

        /// <summary>
        /// Removes the relation between an image and a virtualfolder
        /// </summary>
        /// <param name="imageId"></param>
        /// <param name="virtualfolderId"></param>
        /// <returns></returns>
        //public static async Task RemoveImageRelationsFromVritualfolderAsync(int imageId, int virtualfolderId)
        //{
        //    using (SqliteCommand command = new SqliteCommand($@"DELETE FROM VIRTUALFOLDER_IMAGE
        //        WHERE IMAGE_Id = {imageId} AND VIRTUALFOLDER_Id = {virtualfolderId}", Database))
        //    { await command.ExecuteReaderAsync(); }
        //}

        /// <summary>
        /// Deletes those images from database, that have a relation with given virtual folder
        /// </summary>
        /// <param name="virtualfolderId"></param>
        /// <returns></returns>
        public static async Task DeleteAllImagesInVirtualFolderAsync(int virtualfolderId)
        {
            using (SqliteCommand command = new SqliteCommand($@"DELETE FROM IMAGE WHERE Id IN 
                (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={virtualfolderId})", Database))
            { await command.ExecuteReaderAsync(); }
        }

        public static async Task<List<Triple<int, string, Helpers.GroupPosition>>> GetImagesInVirtualfolderGroupedBySimilarityAsync(int virtualfolderId)
        {
            var result = new List<Triple<int, string, Helpers.GroupPosition>>();

            SqliteCommand selectCommand = new SqliteCommand
                ($@"SELECT * FROM IMAGE WHERE Id IN
                    (SELECT DISTINCT FIRST_IMAGE_Id FROM SIMILAR_IMAGES 
                    WHERE 
                    FIRST_IMAGE_Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={virtualfolderId}) 
                    AND
                    SECOND_IMAGE_Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={virtualfolderId}))", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();


            while (query.Read())
            {
                result.Add(new Triple<int, string, Helpers.GroupPosition>(query.GetInt32(0), query.GetString(1), Helpers.GroupPosition.Start));
            }

            int count = result.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                SqliteCommand cmd = new SqliteCommand
                ($@"SELECT * FROM IMAGE WHERE Id IN
                    (SELECT SECOND_IMAGE_Id FROM SIMILAR_IMAGES 
                    WHERE
                    FIRST_IMAGE_Id = {result[i].Item1}
                    AND
                    SECOND_IMAGE_Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={virtualfolderId}))", Database);

                SqliteDataReader q = await cmd.ExecuteReaderAsync();

                bool wasFirst = false;
                while (q.Read())
                {
                    Helpers.GroupPosition option = Helpers.GroupPosition.None;
                    if (!wasFirst)
                    {
                        option = Helpers.GroupPosition.End;
                        wasFirst = true;
                    }
                    else
                    {
                        option = Helpers.GroupPosition.Middle;
                    }
                    result.Insert(i + 1, new Triple<int, string, Helpers.GroupPosition>(q.GetInt32(0), q.GetString(1), option));
                }
            }

            selectCommand = new SqliteCommand
                ($@"SELECT * FROM IMAGE WHERE
                    Id NOT IN (SELECT FIRST_IMAGE_Id FROM SIMILAR_IMAGES)
                    AND
                    Id NOT IN (SELECT SECOND_IMAGE_Id FROM SIMILAR_IMAGES)
                    AND
                    Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={virtualfolderId})", Database);

            query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                result.Add(new Triple<int, string, Helpers.GroupPosition>(query.GetInt32(0), query.GetString(1), Helpers.GroupPosition.None));
            }

            return result;
        }

        public static async Task<List<DatabaseImage>> GetVirtualfolderImagesWithGroupsAndTags(int virtualfolderId)
        {
            var result = new List<DatabaseImage>();

            SqliteCommand selectCommand = new SqliteCommand
                ($@"SELECT * FROM IMAGE WHERE Id IN
                    (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id = {virtualfolderId})", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                result.Add(new DatabaseImage() { Id = query.GetInt32(0), Path = query.GetString(1) });

                var comm = new SqliteCommand
                ($@"SELECT * FROM SIMILARITYGROUP WHERE Id IN
                    (SELECT SIMILARITYGROUP_Id FROM SIMILARITYGROUP_IMAGE WHERE IMAGE_Id = {result.Last().Id})", Database);

                var q = await comm.ExecuteReaderAsync();

                while (q.Read())
                {
                    result.Last().Group.Id = q.GetInt32(0);
                    result.Last().Group.Name = q.GetString(1);
                }

                var comm1 = new SqliteCommand
                ($@"SELECT tag FROM TAG WHERE Id IN
                    (SELECT TAG_Id FROM IMAGE_TAG WHERE IMAGE_Id = {result.Last().Id})", Database);

                var q1 = await comm1.ExecuteReaderAsync();

                while (q1.Read())
                {
                    result.Last().Tags.Add(q1.GetString(0));
                }
            }

            return result;
        }

        public static async Task<List<string>> GetVirtualfolderTags(int virtualfolderId)
        {
            var result = new List<string>();

            SqliteCommand selectCommand = new SqliteCommand
                ($@"SELECT tag FROM TAG WHERE Id IN
                    (SELECT TAG_Id FROM IMAGE_TAG WHERE IMAGE_Id IN
                        (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id = {virtualfolderId}))", Database);

            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                result.Add(query.GetString(0));
            }

            return result;
        }
        public static async Task<string> GetImagePathAsync(string id)
        {
            string result = null;
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM IMAGE WHERE Id={id}", Database);
            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();
            while (query.Read())
            {
                result = query.GetString(1);
            }
            return result;
        }

        public static async Task<string> GetImagePathAsync(int id)
        {
            return await GetImagePathAsync(id.ToString());
        }

        public static async Task<Tuple<int, string>> GetTagAsync(string tag)
        {
            Tuple<int, string> result = null;
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM TAG WHERE tag = {tag}", Database);
            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();

            while (query.Read())
            {
                result = new Tuple<int, string>(query.GetInt32(0), query.GetString(1));
            }

            return result;
        }
        public static async Task<bool> ImageTagExistsAsync(string ImageId, string TagId)
        {
            SqliteCommand selectCommand = new SqliteCommand
                ($"SELECT * FROM IMAGE_TAG " +
                $"WHERE IMAGE_Id = {ImageId} " +
                $"AND TAG_Id = {TagId}", Database);
            SqliteDataReader query = await selectCommand.ExecuteReaderAsync();
            while (query.Read())
            {
                return true;
            }
            return false;
        }
        public static async Task<Tuple<int, string>> InsertTagAsync(string tag)
        {
            Tuple<int, string> tagindb = await GetTagAsync(tag);
            if (tagindb is null)
            {
                using (SqliteCommand command = new SqliteCommand("INSERT INTO TAG (tag) " +
                        $"VALUES ('{tag}')", Database))
                { await command.ExecuteReaderAsync(); }

                Int64 rowid = 0;
                using (SqliteCommand command = new SqliteCommand("SELECT last_insert_rowid()", Database))
                { rowid = (Int64)await command.ExecuteScalarAsync(); }
                if (rowid == 0)
                {
                    throw new SqliteException("SQLite access exception: Something went wrong!", 1);
                }
                return new Tuple<int, string>((int)rowid, tag);
            }
            else
            {
                return tagindb;
            }
        }
        public static async Task<Tuple<int, int>> InsertImageTagAsync(string ImageId, string tag)
        {
            Tuple<int, string> tagindb = await InsertTagAsync(tag);
            if (!await ImageTagExistsAsync(ImageId, tagindb.Item1.ToString()))
            {
                using (SqliteCommand command = new SqliteCommand("INSERT INTO IMAGE_TAG (IMAGE_Id, TAG_Id) " +
                        $"VALUES ('{ImageId},{tagindb.Item1}')", Database))
                { await command.ExecuteReaderAsync(); }
                Int64 rowid = 0;
                using (SqliteCommand command = new SqliteCommand("SELECT last_insert_rowid()", Database))
                { rowid = (Int64)await command.ExecuteScalarAsync(); }
                if (rowid == 0)
                {
                    throw new SqliteException("SQLite access exception: Something went wrong!", 1);
                }
            }
            return new Tuple<int, int>(int.Parse(ImageId), tagindb.Item1);
        }
    }

    public class Triple<T1, T2, T3>
    {
        public T1 Item1 { get; }
        public T2 Item2 { get; }
        public T3 Item3 { get; }

        public Triple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public static bool operator ==(Triple<T1, T2, T3> t1, Triple<T1, T2, T3> t2)
        {
            return EqualityComparer<T1>.Default.Equals(t1.Item1, t2.Item1) &&
                   EqualityComparer<T2>.Default.Equals(t1.Item2, t2.Item2) &&
                   EqualityComparer<T3>.Default.Equals(t1.Item3, t2.Item3);
        }

        public static bool operator !=(Triple<T1, T2, T3> t1, Triple<T1, T2, T3> t2)
        {
            return !(EqualityComparer<T1>.Default.Equals(t1.Item1, t2.Item1) &&
                   EqualityComparer<T2>.Default.Equals(t1.Item2, t2.Item2) &&
                   EqualityComparer<T3>.Default.Equals(t1.Item3, t2.Item3));
        }

        public override bool Equals(object obj)
        {
            return obj is Triple<T1, T2, T3> triple &&
                   EqualityComparer<T1>.Default.Equals(Item1, triple.Item1) &&
                   EqualityComparer<T2>.Default.Equals(Item2, triple.Item2) &&
                   EqualityComparer<T3>.Default.Equals(Item3, triple.Item3);
        }

        public override int GetHashCode()
        {
            int hashCode = 341329424;
            hashCode = hashCode * -1521134295 + EqualityComparer<T1>.Default.GetHashCode(Item1);
            hashCode = hashCode * -1521134295 + EqualityComparer<T2>.Default.GetHashCode(Item2);
            hashCode = hashCode * -1521134295 + EqualityComparer<T3>.Default.GetHashCode(Item3);
            return hashCode;
        }
    }
}
