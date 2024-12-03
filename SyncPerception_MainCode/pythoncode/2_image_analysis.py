# image_analysis.py
import os
import cv2
import numpy as np
import pandas as pd
from shapely.geometry import Polygon

def extract_color_polygons(image, fixed_color, tolerance=2):
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    lower_bound = np.array(fixed_color) - tolerance
    upper_bound = np.array(fixed_color) + tolerance
    mask = cv2.inRange(image_rgb, lower_bound, upper_bound)
    kernel = np.ones((5, 5), np.uint8)
    mask = cv2.morphologyEx(mask, cv2.MORPH_CLOSE, kernel)
    mask = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)
    contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
    polygons = [contour.reshape(-1, 2).tolist() for contour in contours]
    return polygons

def calculate_centroid(polygon):
    shapely_polygon = Polygon(polygon)
    if not shapely_polygon.is_valid:
        return None, None
    centroid = shapely_polygon.centroid
    return int(centroid.x), int(centroid.y)

def calculate_polygon_area(polygon):
    shapely_polygon = Polygon(polygon)
    if not shapely_polygon.is_valid:
        return 0
    return shapely_polygon.area

def process_single_image(image_path, output_path, labels):
    image = cv2.imread(image_path)
    image_shape = image.shape
    image_area = image_shape[0] * image_shape[1]
    data = []
    count = 0
    for label in labels:
        polygons = extract_color_polygons(image, label['colorRGB'])
        for polygon in polygons:
            centroid_x, centroid_y = calculate_centroid(polygon)
            if centroid_x is None or centroid_y is None:
                continue
            polygon_area = calculate_polygon_area(polygon)
            percentage_area = (polygon_area / image_area) #* 100
            data.append({
                'Node_id': count,
                'name': label['label_name'],
                'Item_id': label['class_id'],
                'centroid_x': centroid_x,
                'centroid_y': centroid_y,
                'proportion': percentage_area,
                'polygon': polygon
            })
            count += 1
    df = pd.DataFrame(data)
    csv_path = os.path.join(output_path, os.path.splitext(os.path.basename(image_path))[0] + '.csv')
    df.to_csv(csv_path, index=False)
    return csv_path