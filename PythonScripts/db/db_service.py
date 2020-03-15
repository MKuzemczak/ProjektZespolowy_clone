from PythonScripts.db.db_creator import DBCreator
import sqlite3


class DBService:
    select = 'SELECT FROM '
    all = ' * '
    where = 'WHERE '
    condition = ' = '

    def __init__(self, db_path=DBCreator.path_to_db):
        self.db_path

    def create_conn(self, db_path=DBCreator.path_to_db):
        conn = None
        try:
            conn = sqlite3.connect(db_path)
        except sqlite3.Error as e:
            print(e)
        return conn

    def create_select(self, table, record, comparator):
        conn = self.create_conn()
        response = conn.execute(
            DBService.select + table + DBService.all + DBService.where + record + DBService.condition + comparator)
        conn.close()
        return response
