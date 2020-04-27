from PIL import Image
from PIL.ExifTags import TAGS
from PIL.ExifTags import GPSTAGS
import piexif
import piexif.helper


def get_exif(filepath):
    image = Image.open(filepath)
    image.verify()
    return image._getexif()

def get_geotagging(exif):
    if not exif:
        raise ValueError("No EXIF metadata found")

    geotagging = {}
    for (idx, tag) in TAGS.items():
        if tag == 'GPSInfo':
            if idx not in exif:
                raise ValueError("No EXIF geotagging found")

            for (key, val) in GPSTAGS.items():
                if key in exif[idx]:
                    geotagging[val] = exif[idx][key]

    return geotagging

def get_decimal_from_dms(dms, ref):

    degrees = dms[0][0] / dms[0][1]
    minutes = dms[1][0] / dms[1][1] / 60.0
    seconds = dms[2][0] / dms[2][1] / 3600.0

    if ref in ['S', 'W']:
        degrees = -degrees
        minutes = -minutes
        seconds = -seconds

    return round(degrees + minutes + seconds, 5)

def get_coordinates(geotags):
    lat = get_decimal_from_dms(geotags['GPSLatitude'], geotags['GPSLatitudeRef'])

    lon = get_decimal_from_dms(geotags['GPSLongitude'], geotags['GPSLongitudeRef'])

    return (lat,lon)

def get_image_coordinates(filepath):
    exif = get_exif(filepath)
    geotags = get_geotagging(exif)
    coordinates = get_coordinates(geotags)
    return coordinates

def get_tags(filepath):
    exif_dict = piexif.load(filepath)
    if piexif.ExifIFD.UserComment in exif_dict["Exif"]:
        user_comment = piexif.helper.UserComment.load(exif_dict["Exif"][piexif.ExifIFD.UserComment])
        location = user_comment.split('#')[0].split(':')[1]
        holiday = user_comment.split('#')[1].split(':')[1]
        return location, holiday
    else:
        return 'None', 'None'
    

def change_location(filepath, location):
    exif_dict = piexif.load(filepath)
    
    if piexif.ExifIFD.UserComment in exif_dict["Exif"]:
        user_comment = piexif.helper.UserComment.load(exif_dict["Exif"][piexif.ExifIFD.UserComment])
        split = user_comment.split('#')
        user_comment_str = 'location:' + location + '#' + split[1]
    else:
        user_comment_str = 'location:' + location + '# holiday:None'
        
    new_user_comment = piexif.helper.UserComment.dump(user_comment_str)
    exif_dict["Exif"][piexif.ExifIFD.UserComment] = new_user_comment
    exif_bytes = piexif.dump(exif_dict)

    piexif.remove(filepath)
    piexif.insert(exif_bytes, filepath)


def change_holiday(filepath, holiday):
    exif_dict = piexif.load(filepath)
    
    if piexif.ExifIFD.UserComment in exif_dict["Exif"]:
        user_comment = piexif.helper.UserComment.load(exif_dict["Exif"][piexif.ExifIFD.UserComment])
        split = user_comment.split('#')
        user_comment_str = split[0] + '#holiday:' + holiday
    else:
        user_comment_str = 'location:None#holiday:' + holiday
        
    new_user_comment = piexif.helper.UserComment.dump(user_comment_str)
    exif_dict["Exif"][piexif.ExifIFD.UserComment] = new_user_comment
    exif_bytes = piexif.dump(exif_dict)

    piexif.remove(filepath)
    piexif.insert(exif_bytes, filepath)

