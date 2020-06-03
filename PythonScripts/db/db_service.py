import sqlite3


class DBService:
    select_all = 'SELECT * FROM '
    select = 'SELECT '
    from_ = ' FROM '
    where = ' WHERE '
    condition = ' IN '
    insert = 'INSERT INTO '
    values = 'VALUES'
    order_by = 'ORDER BY Id DESC;'

    def __init__(self, db_path):
        self.db_path = db_path

    def create_conn(self):
        conn = None
        try:
            conn = sqlite3.connect(self.db_path)
        except sqlite3.Error as e:
            print(e)
        return conn

    def create_select(self, table, record, comparator, response_records='*'):
        """
         @ response_record - zwracane recordy ,string w formie: wanted_record, wanted_record2, wanted_record3
         @ table - tabela np IMAGE
         @ record - rekord który porównujemy np Id
         @ comparator - String w formie: (warunek, warunek2, warunek3,....)

         return - tablica 2d, 1 wymiar to zwrocone rekordy, 2 wymiar to dane w kolejności  @response_record
        """

        conn = self.create_conn()
        cur = conn.cursor()
        query = DBService.select + response_records + DBService.from_ + table + DBService.where + record + DBService.condition + comparator + DBService.order_by
        cur.execute(
            query
        )

        records = cur.fetchall()
        cur.close()
        conn.close()
        return records

    def create_insert(self, table, records, values, data):
        """"
            @ table - nazwa tabeli
            @ records - rekordy,( w kolejności) do którch wrzucamy dane - string w formie: (column, column 1,...)
            @ values ilosc jak dane sa wstawiane np jezeli dwie kolumny: (?,?)
            @ data wartosci dodawane, Tablica krotek

            :return void
        """
        conn = self.create_conn()
        cur = conn.cursor()
        query = DBService.insert + table + ' ' + records + ' ' + DBService.values + values
        cur.executemany(
            query, data
        )
        conn.commit()
        cur.close()
        conn.close()
