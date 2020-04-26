#!/usr/bin/env python

import ExifManipulation as em
import geocoder


def get_address(coordinates):
    g = geocoder.arcgis(coordinates, method='reverse')
    if g.ok:
        return (g.city + ', ' + g.country)
        #return (g.postal + ', ' + g.city + ', ' + g.region + ', ' + g.country)
    else:
        return -1


paths = []
#kod który wrzuci nam ścieżki pobrane z bazy/argumentów do listy ścieżek

paths.append(r'D:\studia\Projekt Zespołowy\tagowanie lokalizacji\TagowanieLokalizacji\sowa.jpg')

for path in paths:
    if em.get_tags(path)[0] == 'None':
        coordinates = em.get_image_coordinates(path)
        address = get_address(coordinates)
        em.change_location(path, address)

print(em.get_tags('sowa.jpg'))
