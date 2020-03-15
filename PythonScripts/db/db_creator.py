import sqlite3


class DBCreator:
    path_to_db = 'projekt_zesp.db'

    # path_to_db = 'D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db'

    @staticmethod
    def create_database():
        conn = sqlite3.connect(DBCreator.path_to_db)
        c = conn.cursor()
        # Create table
        c.execute('''CREATE TABLE IF NOT EXISTS IMAGE
                         (Id INTEGER PRIMARY KEY NOT NULL ,path text NOT NULL)''')
        c.execute('''CREATE TABLE IF NOT EXISTS VIRTUALFOLDER
                         (Id INTEGER PRIMARY KEY NOT NULL , name text NOT NULL)''')
        c.execute('''CREATE TABLE IF NOT EXISTS VIRTUALFOLDER_IMAGE
                         (IMAGE_Id REFERENCES IMAGE(Id) NOT NULL ,VIRTUALFOLDER_Id REFERENCES VIRTUALFOLDER(Id)NOT NULL )''')
        c.execute('''CREATE TABLE IF NOT EXISTS SIMILAR_IMAGES
                         (FIRST_IMAGE_Id REFERENCES IMAGE(Id)NOT NULL ,SECOND_IMAGE_Id REFERENCES IMAGE(Id)NOT NULL )''')
        c.execute('''CREATE TRIGGER IF NOT EXISTS path_validator
                         BEFORE INSERT ON IMAGE
                         BEGIN
                             SELECT
                                CASE
                                WHEN NEW.path NOT LIKE '%_\_%' THEN
                                RAISE (ABORT,'Invalid path')
                                END;
                         END
                         ;
                         ''')
        c.execute('''CREATE TRIGGER IF NOT EXISTS after_image_delete
                         AFTER DELETE ON IMAGE
                         BEGIN
                         DELETE FROM VIRTUALFOLDER_IMAGE WHERE IMAGE_id = OLD.Id ;
                         DELETE FROM SIMILAR_IMAGES WHERE FIRST_IMAGE_Id = OLD.Id ;
                         DELETE FROM SIMILAR_IMAGES WHERE SECOND_IMAGE_Id = OLD.Id ;
                         END
                         ;
                         ''')

        c.execute('''CREATE TRIGGER IF NOT EXISTS after_virtualfolder_delete
                         AFTER DELETE ON VIRTUALFOLDER
                         BEGIN
                         DELETE FROM VIRTUALFOLDER_IMAGE WHERE VIRTUALFOLDER_Id = OLD.Id ;
                         END
                         ;
                         ''')

        c.execute('''CREATE TRIGGER IF NOT EXISTS block_imageid_change
                         BEFORE UPDATE OF Id ON IMAGE
                         FOR EACH ROW
                         BEGIN
                              SELECT RAISE(ABORT, 'NIE ZMIENIAJ ID OBRAZU');
                         END
                         ;
                         ''')

        c.execute('''CREATE TRIGGER IF NOT EXISTS block_virtualfolderid_change
                         BEFORE UPDATE OF Id ON VIRTUALFOLDER
                         FOR EACH ROW
                         BEGIN
                             SELECT RAISE(ABORT, 'NIE ZMIENIAJ ID FOLDERU');
                         END
                         ;
                         ''')

        conn.commit()
        conn.close()


DBCreator.create_database();
