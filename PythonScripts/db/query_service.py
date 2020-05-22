class QueryService:
    # def prepare_args_to_call_select(self, argv) -> str:
    #     if len(argv) >= 3:
    #         comparator = '('
    #         args = [arg for arg in argv if arg != ' ']
    #         for index in range(1, len(args) - 1):
    #             comparator = comparator + str(args[index]) + ', '
    #         comparator = comparator + str(args[len(args) - 1]) + ')'
    #         return comparator
    #     else:
    #         return 'no_argv'

    def prepare_args_to_two_selct(self, images_id) -> (str, str):
        """"
            SQLite nie zwraca w zadanej obiektów, więc similar_images.py potrzebuje wysłać dwa selecty
            zeby otrzymac glowny obrazek oraz porownywane
        """
        if len(images_id) < 3:
            return 'no_argv', ''
        c1 = '(' + str(images_id[0]) + ')'
        args = [arg for arg in images_id if arg != ' ']
        c2 = '('
        for index in range(1, len(args) - 1):
            c2 = c2 + str(args[index]) + ', '
        c2 = c2 + str(args[len(args) - 1]) + ')'

        return c1, c2
