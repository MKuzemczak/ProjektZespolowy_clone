from PythonScripts.db.db_creator import DBCreator
import sqlite3
import sys


class DBService:
    select_all = 'SELECT * FROM '
    select = 'SELECT'
    from_ = 'from'
    where = 'WHERE '
    condition = ' IN '
    insert = 'INSERT INTO '
    values = 'VALUES'

    def __init__(self, db_path=DBCreator.path_to_db):
        self.db_path = db_path

    def create_conn(self, db_path=DBCreator.path_to_db):
        conn = None
        try:
            conn = sqlite3.connect(db_path)
        except sqlite3.Error as e:
            print(e)
        return conn

    def create_select(self, table, record, comparator, response_records='*'):
        """
         @response_record - zwracane recordy ,string w formie: wanted_record, wanted_record2, wanted_record3
         @table - tabela np IMAGE
         @record - rekord który porównujemy np Id
         @comparator - String w formie: (warunek, warunek2, warunek3,....)

         return - tablica 2d, 1 wymiar to zwrocone rekordy, 2 wymiar to dane w kolejności  @response_record
        """

        conn = self.create_conn()
        conn.execute(
            DBService.select +
            response_records +
            DBService.from_ +
            table +
            DBService.where +
            record +
            DBService.condition +
            comparator
        )

        records = conn.fetchall()
        conn.close()
        return records

    def create_insert(self, table, records, values):
        """"
            @ table - nazwa tabeli
            @ records - rekordy,( w kolejności) do którch wrzucamy dane - string w formie: (column, column 1,...)
            @ values wartosci dodawane, string w formie:
                (value1,value2 ,...),
                (value1,value2 ,...),
                    ...
                (value1,value2 ,...)

            void
        """
        conn = self.create_conn()
        conn.execute(
            DBService.insert +
            table +
            records +
            DBService.values +
            values +
            ';'
        )

    def prepare_args_to_call_select(self):
        if len(sys.argv) >= 3:
            comparator = '('
            args = [arg for arg in sys.argv if arg != ' ']
            for arg in range(len(args) - 1):
                comparator = comparator + str(arg) + ', '
            comparator = comparator + str(args[len(args)-1]) + ');'
            return comparator
        else:
            return 'no_argv'


