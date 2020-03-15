class QueryService:
    def prepare_args_to_call_select(self, argv):
        if len(argv) >= 3:
            comparator = '('
            args = [arg for arg in argv if arg != ' ']
            for index in range(1, len(args) - 1):
                comparator = comparator + str(args[index]) + ', '
            comparator = comparator + str(args[len(args) - 1]) + ')'
            return comparator
        else:
            return 'no_argv'
