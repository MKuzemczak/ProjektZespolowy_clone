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
    public static class DatabaseAccessService
    {
        public static bool Initialized = false;

        public async static void InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync("sqliteSample.db", CreationCollisionOption.OpenIfExists);
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS IMAGE" +
                         "(Id INTEGER PRIMARY KEY NOT NULL, path text NOT NULL)", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS VIRTUALFOLDER" +
                         "(Id INTEGER PRIMARY KEY NOT NULL, name text NOT NULL)", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS VIRTUALFOLDER_IMAGE" +
                         "(IMAGE_Id REFERENCES IMAGE(Id) NOT NULL, VIRTUALFOLDER_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL)", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS SIMILAR_IMAGES" +
                         "(FIRST_IMAGE_Id REFERENCES IMAGE(Id)NOT NULL, SECOND_IMAGE_Id REFERENCES IMAGE(Id)NOT NULL)", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS VIRTUALFOLDER_RELATION" +
                         "(PARENT_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL, CHILD_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL)", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TABLE IF NOT EXISTS ACCESSEDFOLDER" +
                         "(Id INTEGER PRIMARY KEY NOT NULL, token text NOT NULL)", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS path_validator " +
                        "BEFORE INSERT ON IMAGE " +
                        "BEGIN " +
                            "SELECT " +
                            "CASE " +
                            "WHEN NEW.path NOT LIKE '%_\\_%' THEN " +
                            "RAISE(ABORT, 'Invalid path') " +
                            "END; " +
                        "END;", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_image_delete " +
                        "AFTER DELETE ON IMAGE " +
                        "BEGIN " +
                            "DELETE FROM VIRTUALFOLDER_IMAGE WHERE IMAGE_id = OLD.Id; " +
                            "DELETE FROM SIMILAR_IMAGES WHERE FIRST_IMAGE_Id = OLD.Id; " +
                            "DELETE FROM SIMILAR_IMAGES WHERE SECOND_IMAGE_Id = OLD.Id; " +
                        "END ; ", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS after_virtualfolder_delete " +
                        "AFTER DELETE ON VIRTUALFOLDER " +
                        "BEGIN " +
                            "DELETE FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id = OLD.Id; " +
                        "END; ", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS block_imageid_change " +
                        "BEFORE UPDATE OF Id ON IMAGE " +
                        "FOR EACH ROW " +
                        "BEGIN " +
                            "SELECT RAISE(ABORT, 'NIE ZMIENIAJ ID OBRAZU'); " +
                        "END;", db))
                { command.ExecuteReader(); }

                using (SqliteCommand command = new SqliteCommand("CREATE TRIGGER IF NOT EXISTS block_virtualfolderid_change " +
                        "BEFORE UPDATE OF Id ON VIRTUALFOLDER " +
                        "FOR EACH ROW " +
                        "BEGIN " +
                            "SELECT RAISE(ABORT, 'NIE ZMIENIAJ ID FOLDERU'); " +
                        "END; ", db))
                { command.ExecuteReader(); }
            }

            Initialized = true;
        }

        public static List<DatabaseVirtualFolder> GetRootVirtualFolders()
        {
            List<DatabaseVirtualFolder> result = new List<DatabaseVirtualFolder>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * FROM VIRTUALFOLDER WHERE Id NOT IN (SELECT CHILD_Id FROM VIRTUALFOLDER_RELATION)", db);

                SqliteDataReader query = selectCommand.ExecuteReader();


                while (query.Read())
                {
                    DatabaseVirtualFolder folder = new DatabaseVirtualFolder()
                    {
                        Id = query.GetInt32(0),
                        Name = query.GetString(1)
                    };
                    
                    result.Add(folder);
                }

                db.Close();
            }

            return result;
        }

        public static List<DatabaseVirtualFolder> GetChildrenOfFolder(string id)
        {
            List<DatabaseVirtualFolder> result = new List<DatabaseVirtualFolder>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM VIRTUALFOLDER WHERE Id IN (SELECT CHILD_Id FROM VIRTUALFOLDER_RELATION WHERE PARENT_Id={id})", db);

                SqliteDataReader query = selectCommand.ExecuteReader();


                while (query.Read())
                {
                    DatabaseVirtualFolder folder = new DatabaseVirtualFolder()
                    {
                        Id = query.GetInt32(0),
                        Name = query.GetString(1)
                    };

                    result.Add(folder);
                }

                db.Close();
            }

            return result;
        }

        public static List<DatabaseVirtualFolder> GetChildrenOfFolder(int id)
        {
            return GetChildrenOfFolder(id.ToString());
        }

        public static List<DatabaseVirtualFolder> GetChildrenOfFolder(DatabaseVirtualFolder folder)
        {
            return GetChildrenOfFolder(folder.Id);
        }

        public static DatabaseVirtualFolder GetParentOfFolder(string id)
        {
            DatabaseVirtualFolder result = new DatabaseVirtualFolder();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM VIRTUALFOLDER WHERE Id IN (SELECT PARENT_Id FROM VIRTUALFOLDER_RELATION WHERE CHILD_Id={id})", db);

                SqliteDataReader query = selectCommand.ExecuteReader();


                while (query.Read())
                {
                    DatabaseVirtualFolder folder = new DatabaseVirtualFolder()
                    {
                        Id = query.GetInt32(0),
                        Name = query.GetString(1)
                    };

                    result = folder;
                }

                db.Close();
            }

            return result;
        }

        public static DatabaseVirtualFolder GetParentOfFolder(int id)
        {
            return GetParentOfFolder(id.ToString());
        }

        public static DatabaseVirtualFolder GetParentOfFolder(DatabaseVirtualFolder folder)
        {
            return GetParentOfFolder(folder.Id);
        }

        public static List<string> GetImagesInFolder(string id)
        {
            var result = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT * FROM IMAGE WHERE Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={id})", db);

                SqliteDataReader query = selectCommand.ExecuteReader();


                while (query.Read())
                {
                    result.Add(query.GetString(1));
                }

                db.Close();
            }

            return result;
        }

        public static List<string> GetImagesInFolder(int id)
        {
            return GetImagesInFolder(id.ToString());
        }

        public static List<string> GetImagesInFolder(DatabaseVirtualFolder folder)
        {
            return GetImagesInFolder(folder.Id);
        }

        public static int GetImagesCountInFolder(string id)
        {
            int result = -1;

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT COUNT(*) FROM IMAGE WHERE Id IN (SELECT IMAGE_Id FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id={id})", db);

                SqliteDataReader query = selectCommand.ExecuteReader();


                while (query.Read())
                {
                    result = query.GetInt32(0);
                }

                db.Close();
            }

            return result;
        }

        public static int GetImagesCountInFolder(int id)
        {
            return GetImagesCountInFolder(id.ToString());
        }

        public static int GetImagesCountInFolder(DatabaseVirtualFolder folder)
        {
            return GetImagesCountInFolder(folder.Id);
        }

        public static void AddAccessedFolder(string token)
        {
            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                using (SqliteCommand command = new SqliteCommand("INSERT INTO ACCESSEDFOLDER (token) " +
                         $"VALUES ('{token}')", db))
                { command.ExecuteReader(); }
            }
        }

        public static List<string> GetAccessedFolders()
        {
            var result = new List<string>();

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "sqliteSample.db");
            using (SqliteConnection db =
               new SqliteConnection($"Filename={dbpath}"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ($"SELECT token FROM ACCESSEDFOLDER", db);

                SqliteDataReader query = selectCommand.ExecuteReader();


                while (query.Read())
                {
                    result.Add(query.GetString(0));
                }

                db.Close();
            }

            return result;
        }
    }
}
