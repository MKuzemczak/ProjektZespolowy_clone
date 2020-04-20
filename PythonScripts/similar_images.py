import cv2
import sys

from PythonScripts.db.db_creator import DBCreator
from PythonScripts.db.db_service import DBService
from PythonScripts.db.query_service import QueryService


class SimilarImageRecognizer:
    def __init__(self, path_to_db):
        self.qs = QueryService()
        self.db_service = DBService(db_path=path_to_db)

    def __compare_orb_bd(self, filepath, filepath2):
        # 0 oznacza wczytanie tylko czarno biale
        image = cv2.imread(filepath, 0)
        image2 = cv2.imread(filepath2, 0)
        # TODO normalizacja
        # moze jakas normalizacja by wpadła

        # połaczenie alg fastkeypoints i brief deskryptor
        # number_of_featue => zeby nie trawalo to wiekow
        number_of_feature = 1000
        orb = cv2.ORB_create(number_of_feature)
        # kp - punkty kluczowe, desc - deskryptor
        kp, desc = orb.detectAndCompute(image, None)
        kp2, desc2 = orb.detectAndCompute(image2, None)

        index_param = dict(algorithm=0, trees=0)
        search_params = dict()

        flann = cv2.FlannBasedMatcher(index_param, search_params)
        matcher = cv2.DescriptorMatcher_create(cv2.DESCRIPTOR_MATCHER_BRUTEFORCE_SL2)
        matches = matcher.match(desc, desc2, None)

        # w zależności od odlgełosci humminga
        matches = [m for m in matches if m.distance < 30]

        # powiedzmy 5 maczy które są dokładne
        if len(matches) > 5:
            return True
        return False

    def __compare_histogram_and_probability(self, filepath, filepath2):
        image = cv2.imread(filepath, 0)
        image2 = cv2.imread(filepath2, 0)

        hist = cv2.calcHist([image], [0], None, [256], [0, 256])
        hist2 = cv2.calcHist([image2], [0], None, [256], [0, 256])

        hist_diff = cv2.compareHist(hist, hist2, cv2.HISTCMP_BHATTACHARYYA)
        probability_match = cv2.matchTemplate(hist, hist2, cv2.TM_CCOEFF_NORMED)[0][0]

        img_template_diff = 1 - probability_match

        # srednia wazona bo histogram jest mniej miarodajny ale wnosi informacje
        commutative_image_diff = (hist_diff + img_template_diff) / 2

        # commutative_image_diff = (hist_diff / 10) + img_template_diff

        # 20% odchyłu
        if commutative_image_diff < 0.2:
            return True
        return False

    def __find_similar_in_list(self, list_of_db_image):
        main_img = list_of_db_image[0][1]
        similar = []
        for index in range(1, len(list_of_db_image)):

            if self.__compare_histogram_and_probability(main_img, list_of_db_image[index][1]):
                similar.append(list_of_db_image[index][0])

        return similar

    def compare_images(self, images_id):
        c1, c2 = self.qs.prepare_args_to_two_selct(images_id)
        if c1 == 'no_arg':
            return False

        main_image = self.db_service.create_select('IMAGE', 'Id', c1)
        # list of (Id,path)
        images = [*main_image]
        images = images + self.db_service.create_select('IMAGE', 'Id', c2)
        similar = self.__find_similar_in_list(images)
        func = map(lambda x: (images[0][0], x), similar)
        values = list(func)
        self.db_service.create_insert("SIMILAR_IMAGES", '(FIRST_IMAGE_Id, SECOND_IMAGE_Id)', '(?,?)', values)

        return True

#
# if __name__ == '__main__':
#     print(sys.argv)
#     sir = SimilarImageRecognizer(DBCreator.path_to_db)
#     isDone = sir.compare(sys.argv)
#     print(isDone)
