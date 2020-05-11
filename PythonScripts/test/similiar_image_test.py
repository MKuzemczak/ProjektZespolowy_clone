import unittest

from images.similar_images import SimilarImageRecognizer
from db.db_service import DBService
import json
from collections import namedtuple


class SimiliarImageTest(unittest.TestCase):
    def test_quality(self):
        sir = SimilarImageRecognizer('D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db')
        images_id = [5, 1, 2, 3, 4, 11, 6, 7, 8, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]
        c1, c2 = sir.qs.prepare_args_to_two_selct(images_id)
        if c1 == 'no_arg':
            return False

        main_image = sir.db_service.create_select('IMAGE', 'Id', c1)
        # list of (Id,path)
        images = [*main_image]
        images = images + sir.db_service.create_select('IMAGE', 'Id', c2)

        # similar = sir.__find_similar_in_list(images)
        # print(similar)

    def test_compare(self):
        sir = SimilarImageRecognizer('D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db')
        images_id = [5, 1, 2, 3, 4, 11, 6, 7, 8, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]
        sir.compare_images(images_id)

    def test_group_images(self):
        db = DBService('D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db')
        images_id = [5, 1, 2, 3, 4, 11, 6, 7, 8, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]
        sel = '('
        for index in range(len(images_id) - 1):
            sel = sel + str(images_id[index]) + ', '
        sel = sel + str(images_id[len(images_id) - 1]) + ')'
        images_path = db.create_select('IMAGE', 'Id', sel)
        #images_path = list(map(lambda x: x[1], images_path))
        images_path = list(map(lambda x: [x[0], x[1]], images_path))
        print(images_path)
        groups = SimilarImageRecognizer.group_by_histogram_and_probability(images_path)
        print(groups)

    def test_json(self):
        json_init = json.loads('{"taskid":123,"type":0,"images":1}')
        json_init = json.dumps(json_init)
        x = json.loads(json_init, object_hook=lambda d: namedtuple('X', d.keys())(*d.values()))

        # return arr[0], arr[1], arr[2:]
        print(x.taskid, x.type, x.images)
        print(json_init)
