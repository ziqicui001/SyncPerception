import os
import cv2
import numpy as np
from openvino.runtime import Core
import matplotlib.cm

# Helper function: Normalize data
def normalize_minmax(data):
    min_val = data.min()
    max_val = data.max()
    return (data - min_val) / (max_val - min_val)

# Main function: Process an image and generate depth map
def process_image(image_path, save_folder_path, model_path="pythoncode\MiDaS_small.xml"):
    # Load OpenVINO model
    core = Core()
    core.set_property({'CACHE_DIR': '../cache'})
    if not os.path.exists(model_path):
        raise FileNotFoundError(f"Model file not found: {model_path}")
    
    model = core.read_model(model_path)
    compiled_model = core.compile_model(model)
    input_key = compiled_model.input(0)
    output_key = compiled_model.output(0)

    network_input_shape = input_key.shape
    network_image_height = int(network_input_shape[2])
    network_image_width = int(network_input_shape[3])

    # Load and preprocess the image
    image = cv2.imread(image_path)
    resized_image = cv2.resize(image, (network_image_width, network_image_height))
    input_image = np.expand_dims(np.transpose(resized_image, (2, 0, 1)), axis=0)

    # Run model inference
    result = compiled_model([input_image])[output_key]

    # Post-process the result
    result_image = normalize_minmax(result.squeeze(0))
    cmap = matplotlib.cm.get_cmap("viridis")
    result_rgb = (cmap(result_image)[:, :, :3] * 255).astype(np.uint8)
    result_resized = cv2.resize(result_rgb, (image.shape[1], image.shape[0]))
    grayscale_image = cv2.cvtColor(result_resized, cv2.COLOR_BGR2GRAY)

    # Save the resulting depth map
    if not os.path.exists(save_folder_path):
        os.makedirs(save_folder_path)

    output_path = os.path.join(save_folder_path, os.path.splitext(os.path.basename(image_path))[0] + "_depth.jpg")
    cv2.imwrite(output_path, grayscale_image)
    return output_path