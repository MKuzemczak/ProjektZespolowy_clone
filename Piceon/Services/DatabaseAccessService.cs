using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace Piceon.Services
{
    public static class DatabaseAccessService
    {
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
                         "(PARENT_Id REFERENCES IMAGE(Id)NOT NULL, CHILD_Id REFERENCES IMAGE(Id)NOT NULL)", db))
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
        }


    }
}
