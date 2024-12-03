import os
import torch
import torch.nn.functional as F
from torch_geometric.loader import DataLoader
from torch_geometric.nn import GINConv, global_mean_pool
from torch.nn import Linear, Sequential, ReLU
import logging

# 确保日志初始化只有一次
if not logging.getLogger().hasHandlers():
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s [%(levelname)s]: %(message)s',
        handlers=[
            logging.FileHandler(r"D:\CV+GNN\application\gh_plugin_VS\PythonEnvChecker\debug_log.txt", mode='w'),
            logging.StreamHandler()
        ]
    )

logging.info("Debug logging initialized successfully.")
logging.info(f"Python environment: {os.environ}")
logging.info(f"Torch version: {torch.__version__}")

# 定义 GIN 模型
class GNN(torch.nn.Module):
    def __init__(self, num_features, embedding_size=64, dropout_rate=0.3, num_classes=5):
        super(GNN, self).__init__()
        nn1 = Sequential(Linear(num_features, embedding_size), ReLU(), Linear(embedding_size, embedding_size))
        self.initial_conv = GINConv(nn1)
        nn2 = Sequential(Linear(embedding_size, embedding_size), ReLU(), Linear(embedding_size, embedding_size))
        self.conv1 = GINConv(nn2)

        self.dropout = torch.nn.Dropout(p=dropout_rate)
        self.out = Linear(embedding_size, num_classes)

    def forward(self, x, edge_index, batch):
        logging.info(f"Forward pass with input shapes: x={x.shape}, edge_index={edge_index.shape}, batch={batch.shape}")
        x = F.relu(self.initial_conv(x, edge_index))
        x = self.dropout(x)
        x = F.relu(self.conv1(x, edge_index))
        x = self.dropout(x)
        x = global_mean_pool(x, batch)
        x = self.out(x)
        return x

# 动态加载模型
def load_model(model_path, device="cpu"):
    try:
        device = torch.device(device)  # 确保设备是 torch.device 类型
        logging.info(f"Attempting to load model from: {model_path}")

        if not os.path.exists(model_path):
            logging.error(f"Model file does not exist at: {model_path}")
            return None

        # 使用自定义上下文，显式提供 GNN 定义
        model = torch.load(model_path, map_location=device, weights_only=False, pickle_module=__import__('pickle'), globals={'GNN': GNN})

        if not isinstance(model, torch.nn.Module):
            logging.error(f"Loaded object is not a valid PyTorch model: {type(model)}")
            return None

        model.eval()
        logging.info("Model loaded successfully.")
        return model
    except Exception as e:
        logging.error(f"Error loading model: {e}")
        logging.exception("Traceback details:")
        return None

# 定义预测函数
def predict_probabilities(graph_load_path, model_path, device="cpu"):
    try:
        # 加载模型
        model = load_model(model_path, device)
        if model is None:
            logging.error("Failed to load the model.")
            return [0.0] * 5

        device = torch.device(device)
        logging.info(f"Trying to load graph data from: {graph_load_path}")

        if not os.path.exists(graph_load_path):
            logging.error("Graph file does not exist.")
            return [0.0] * 5

        standardized_graph = torch.load(graph_load_path)
        logging.info(f"Standardized graph content: {standardized_graph}")

        data_list = [standardized_graph]
        data_loader = DataLoader(data_list, batch_size=1, shuffle=False)

        # 模型和数据移动到设备
        model.to(device)
        logging.info(f"Model moved to device: {device}")

        for data in data_loader:
            logging.info(f"Processing data: {data}")
            data = data.to(device)
            with torch.no_grad():
                output = model(data.x, data.edge_index, data.batch)
                logging.info(f"Model output: {output}")
                probabilities = torch.softmax(output, dim=1).cpu().numpy()[0]

        probabilities = [round(float(prob), 6) for prob in probabilities]
        logging.info(f"Prediction probabilities: {probabilities}")
        return probabilities
    except Exception as e:
        logging.error(f"Error in prediction: {e}")
        logging.exception("Traceback details:")
        return [0.0] * 5