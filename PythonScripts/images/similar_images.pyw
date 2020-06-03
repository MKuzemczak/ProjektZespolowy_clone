
import statistics
import gc
import cv2
import numpy as np
from skimage import io, color
from skimage.feature import local_binary_pattern as lbp
from scipy.spatial.distance import euclidean


class SimilarImageRecognizer:

    @staticmethod
    def __compare_orb_bd(filepath, filepath2):
        # 0 oznacza wczytanie tylko czarno biale
        image = cv2.imread(filepath, 0)
        image2 = cv2.imread(filepath2, 0)

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

    @staticmethod
    def __compare_histogram_and_probability(filepath, filepath2):
        image = cv2.imread(filepath, 0)
        image2 = cv2.imread(filepath2, 0)

        hist = cv2.calcHist([image], [0], None, [256], [0, 256])
        hist2 = cv2.calcHist([image2], [0], None, [256], [0, 256])

        hist_diff = cv2.compareHist(hist, hist2, cv2.HISTCMP_BHATTACHARYYA)
        probability_match = cv2.matchTemplate(hist, hist2, cv2.TM_CCOEFF_NORMED)[0][0]

        img_template_diff = 1 - probability_match

        # srednia wazona bo histogram jest mniej miarodajny ale wnosi informacje
        # commutative_image_diff = (hist_diff + img_template_diff) / 2

        commutative_image_diff = (hist_diff + 10 * img_template_diff) / 11

        # 20% odchyłu
        if commutative_image_diff < 0.2:
            return True
        return False

    @staticmethod
    def group_by_histogram_and_probability(ids_filepaths, threshold: float = 0.1):
        def compare(hist, hist2) -> int:
            img_hist_diff = 1 - cv2.compareHist(hist, hist2, cv2.HISTCMP_CORREL)
            img_template_probability_match = cv2.matchTemplate(hist, hist2, cv2.TM_CCOEFF_NORMED)[0][0]
            img_template_diff = 1 - img_template_probability_match
            [qual] = max(cv2.matchTemplate(hist, hist2, cv2.TM_CCOEFF_NORMED))
            qual = 1 - qual
            commutative_image_diff = (img_hist_diff + img_hist_diff * 10) / 11


            return commutative_image_diff

        m = cv2.DescriptorMatcher_create(cv2.DESCRIPTOR_MATCHER_BRUTEFORCE_HAMMING)

        def compare2(desc1, desc2, matcher=m):
            matches = matcher.match(desc1, desc2, None)
            matches = [m for m in matches if m.distance < 30]
            return len(matches)

        histograms_and_id_paths = []
        groups = []
        detector = cv2.ORB_create()
        for id_path in ids_filepaths:
            img = cv2.imread(id_path[1])

            hist = cv2.calcHist([img], [0], None, [256], [0, 256])
            histograms_and_id_paths.append((hist, id_path))

        print(histograms_and_id_paths)
        while len(histograms_and_id_paths) > 1:
            tmp_group = []

            for i in range(1, len(histograms_and_id_paths)):
                tmp_th = compare(histograms_and_id_paths[i][0], histograms_and_id_paths[0][0])
                if tmp_th < threshold:
                    if len(tmp_group) > 1:
                        tmp_group.append(histograms_and_id_paths[i][1][0])
                    else:
                        tmp_group.append(histograms_and_id_paths[i][1][0])
                        tmp_group.append(histograms_and_id_paths[0][1][0])

            if len(tmp_group) > 0:
                for id in tmp_group:
                    histograms_and_id_paths = list(filter(lambda x: x[1][0] != id, histograms_and_id_paths))
                groups.append(tmp_group)
            else:
                del histograms_and_id_paths[0]
        return groups

    @staticmethod
    def grpup_by_segment_hist(ids_paths):
        def take_seg_hist(img, x=6):
            img = cv2.resize(img, (x * 100, x * 100))
            hists = []
            for i in range(1, x + 1):
                seg = img[0:x * 100, 0:i * 100]
                his = cv2.calcHist([seg], [0], None, [256], [0, 256])
                hists.append(his)
            return hists

        def compare(hists_1, hists_2):
            result = []
            for i in range(len(hists_1)):
                r = 1 - cv2.compareHist(hists_1[i], hists_2[i], cv2.HISTCMP_CORREL)
                result.append(r)
            return result

        groups = []
        while len(ids_paths) > 1:

            tmp_group = []
            img = cv2.imread(ids_paths[0][1])
            pivot_his = take_seg_hist(img)
            for i in range(1, len(ids_paths)):
                print(ids_paths[i][1])
                tmp = cv2.imread(ids_paths[i][1])
                tmp_hist = take_seg_hist(tmp)
                comparation = compare(pivot_his, tmp_hist)

                print(comparation)
                if statistics.mean(comparation) < 0.4 and max(comparation) < 1:
                    if len(tmp_group) > 1:
                        tmp_group.append(ids_paths[i][0])
                    else:
                        tmp_group.append(ids_paths[i][0])
                        tmp_group.append(ids_paths[0][0])

            if len(tmp_group) > 0:
                for id in tmp_group:
                    ids_paths = list(filter(lambda x: x[0] != id, ids_paths))
                groups.append(tmp_group)
            else:
                del ids_paths[0]
        print(groups)
        return groups

    @staticmethod
    def group_by_local_binary_patters(ids_pahts: [], thr: float = 0.015):
        def lbp_histogram(color_image):
            img = color.rgb2gray(color_image)
            w, h = img.shape[:2]
            if w * h > 10000000:
                img = cv2.resize(img, (w // 4, h // 4))
            elif w * h > 4750000:
                img = cv2.resize(img, (w // 2, h // 2))

            patterns = lbp(img, 8, 1)
            hist, _ = np.histogram(patterns, bins=np.arange(2 ** 8 + 1), density=True)
            return hist

        def compare_filename(path_1: str, path_2: str) -> bool:
            name_1: str = path_1.rsplit('\\', 1)[1]
            name_2: str = path_2.rsplit('\\', 1)[1]
            length = min(len(name_2), len(name_1), 3)
            for i in range(length):
                if name_1[i] != name_2[i]:
                    return False
            return True
            # diff = len([li for li in difflib.ndiff(name_1, name_2) if li[0] != ' '])
            # return diff <= 5

        def compare_his(his_1, his_2) -> bool:
            return euclidean(his_1, his_2) < thr

        groups = []
        while len(ids_pahts) > 1:
            tmp_group = []
            pivot = lbp_histogram(io.imread(ids_pahts[0][1]))
            pivot_id_path = ids_pahts[0]
            for i in range(1, len(ids_pahts)):
                if compare_filename(pivot_id_path[1], ids_pahts[i][1]):
                    tmp = lbp_histogram(io.imread(ids_pahts[i][1]))
                    if compare_his(pivot, tmp):
                        print(pivot_id_path[1], ids_pahts[i][1])
                        if len(tmp_group) > 1:
                            tmp_group.append(ids_pahts[i][0])
                        else:
                            tmp_group.append(ids_pahts[i][0])
                            tmp_group.append(pivot_id_path[0])
                    del tmp
                    gc.collect()

            if len(tmp_group) > 0:
                for id in tmp_group:
                    ids_pahts = list(filter(lambda x: x[0] != id, ids_pahts))
                groups.append(tmp_group)
            else:
                del ids_pahts[0]
        return groups

    # @staticmethod
    # def group_by_binary_desc(ids_filepaths):
    #     # print('group f')
    #
    #     def compare(flann, desc_1, desc_2):
    #         matches = flann.knnMatch(desc_1, desc_2, k=2)
    #         return matches
    #
    #     descriptors = []
    #     sift = cv2.xfeatures2d.SIFT_create()
    #     for id_path in ids_filepaths:
    #         img = cv2.imread(id_path[1], 0)
    #         _, desc = sift.detectAndCompute(img, None)
    #         descriptors.append((desc, id_path))
    #         groups = []
    #
    #     while len(descriptors) > 1:
    #         tmp_group = []
    #         for i in range(1, len(descriptors)):
    #             if len(compare(descriptors[i][0], descriptors[0][0])) > 5:
    #                 if len(tmp_group) > 1:
    #                     tmp_group.append(descriptors[i][1][0])
    #                 else:
    #                     tmp_group.append(descriptors[i][1][0])
    #                     tmp_group.append(descriptors[0][1][0])
    #
    #         if len(tmp_group) > 0:
    #             for id in tmp_group:
    #                 descriptors = list(filter(lambda x: x[1][0] != id, descriptors))
    #             groups.append(tmp_group)
    #         else:
    #             del descriptors[0]
    #     return groups

    # @staticmethod
    # def __find_similar_in_list(list_of_db_image):
    #     main_img = list_of_db_image[0][1]
    #     similar = []
    #     for index in range(1, len(list_of_db_image)):
    #
    #         if SimilarImageRecognizer.__compare_histogram_and_probability(main_img, list_of_db_image[index][1]):
    #             similar.append(list_of_db_image[index][0])
    #
    #     return similar

    # def compare_images(self, images_id):
    #     c1, c2 = self.qs.prepare_args_to_two_selct(images_id)
    #     if c1 == 'no_arg':
    #         return False
    #
    #     main_image = self.db_service.create_select('IMAGE', 'Id', c1)
    #     # list of (Id,path)
    #     images = [*main_image]
    #     images = images + self.db_service.create_select('IMAGE', 'Id', c2)
    #     similar = self.__find_similar_in_list(images)
    #     func = map(lambda x: (images[0][0], x), similar)
    #     values = list(func)
    #
    #     self.db_service.create_insert("SIMILAR_IMAGES", '(FIRST_IMAGE_Id, SECOND_IMAGE_Id)', '(?,?)', values)
    #
    #     return True

#
# if __name__ == '__main__':
#     print(sys.argv)
#     sir = SimilarImageRecognizer(DBCreator.path_to_db)
#     isDone = sir.compare(sys.argv)
#     print(isDone)
