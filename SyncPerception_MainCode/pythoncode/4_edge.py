import cv2
import numpy as np
import pandas as pd
from shapely.geometry import Polygon, box
from itertools import combinations
import os
import ast
import logging

logging.basicConfig(level=logging.INFO)

# Convert polygon string format to a list of tuples
def convert_polygon_format(polygon_str):
    try:
        points_list = ast.literal_eval(polygon_str)
        return [(float(x), float(y)) for x, y in points_list]
    except Exception as e:
        logging.error(f"Error parsing polygon: {polygon_str} with error {e}")
        return None

# Process a single image and its corresponding CSV file to create edge data
def process_single_file(image_path, csv_path, output_folder_path):
    # Validate image path
    if not os.path.exists(image_path):
        raise FileNotFoundError(f"Image file does not exist: {image_path}")
    if not os.path.isfile(image_path):
        raise ValueError(f"Provided image path is not a file: {image_path}")
    
    # Validate CSV path
    if not os.path.exists(csv_path):
        raise FileNotFoundError(f"CSV file does not exist: {csv_path}")
    if not os.path.isfile(csv_path):
        raise ValueError(f"Provided CSV path is not a file: {csv_path}")

    logging.info(f"Processing image: {image_path}")
    logging.info(f"Processing CSV: {csv_path}")

    # Load image
    image = cv2.imread(image_path)
    if image is None:
        raise ValueError(f"Failed to load image. Ensure the file is a valid image: {image_path}")

    # Ensure output folder exists
    os.makedirs(output_folder_path, exist_ok=True)

    # Load CSV
    df = pd.read_csv(csv_path, encoding="latin")

    # Get image dimensions
    image_height, image_width, _ = image.shape

    # Parse polygons
    df['polygon'] = df['polygon'].apply(convert_polygon_format)
    df = df[df['polygon'].notnull()]
    df['polygon'] = df['polygon'].apply(lambda x: Polygon(x) if x else None)
    df = df[df['polygon'].notnull()]

    # Prepare buffer and image bounds
    connected_pairs = []
    kernel_size = 2
    image_bounds = box(0, 0, image_width, image_height)
    df['buffered_polygon'] = df['polygon'].apply(lambda x: x.buffer(kernel_size).intersection(image_bounds))

    # Find connected pairs
    for (idx1, row1), (idx2, row2) in combinations(df.iterrows(), 2):
        poly1 = row1['buffered_polygon']
        poly2 = row2['buffered_polygon']

        if poly1.intersects(poly2) or poly1.touches(poly2):
            connected_pairs.append((row1['Node_id'], row2['Node_id'], row1['centroid_x'], row1['centroid_y'], row2['centroid_x'], row2['centroid_y']))

    # Create edge list DataFrame
    connected_df = pd.DataFrame(connected_pairs, columns=['ID1', 'ID2', 'centroid_x1', 'centroid_y1', 'centroid_x2', 'centroid_y2'])
    connected_df['length'] = connected_df.apply(lambda row: np.sqrt((row['centroid_x2'] - row['centroid_x1'])**2 + (row['centroid_y2'] - row['centroid_y1'])**2), axis=1)
    connected_df = connected_df[['ID1', 'ID2', 'length']]

    # Save edge list to CSV
    output_filename = f"{os.path.splitext(os.path.basename(csv_path))[0]}_edgelist.csv"
    output_path = os.path.join(output_folder_path, output_filename)
    connected_df.to_csv(output_path, index=False)

    logging.info(f"Edge list CSV file saved at: {output_path}")
    return output_path