import os
import pandas as pd
import torch
from torch_geometric.data import Data
import joblib

# 加载 MinMaxScaler
try:
    print(f"Loading node scaler from: {os.path.abspath('x_scaler.pkl')}")
    print(f"Loading edge scaler from: {os.path.abspath('edge_scaler.pkl')}")
    node_scaler = joblib.load('x_scaler.pkl')
    edge_scaler = joblib.load('edge_scaler.pkl')
except Exception as e:
    raise RuntimeError(f"Error loading scaler: {e}")

class CustomDataset:
    def __init__(self, node_file_path, edge_file_path):
        self.node_file_path = node_file_path
        self.edge_file_path = edge_file_path

    def get(self):
        nodes_df = pd.read_csv(self.node_file_path)
        edges_df = pd.read_csv(self.edge_file_path)

        nodes_df = nodes_df.drop(['Node_id', 'name', 'polygon'], axis=1, errors='ignore')
        nodes_df = nodes_df.apply(pd.to_numeric, errors='coerce').fillna(0)

        edges_df = edges_df.apply(pd.to_numeric, errors='coerce').fillna(0)

        node_features = torch.tensor(nodes_df.values, dtype=torch.float)
        edge_index = torch.tensor(edges_df[['ID1', 'ID2']].values, dtype=torch.long).t().contiguous()
        edge_attr = torch.tensor(edges_df[['length']].values, dtype=torch.float)

        graph_id = os.path.splitext(os.path.basename(self.node_file_path))[0]
        return Data(x=node_features, edge_index=edge_index, edge_attr=edge_attr, graph_id=graph_id)

def apply_normalization(features, scaler):
    if not isinstance(features, torch.Tensor):
        features = torch.tensor(features)
    normalized_features = scaler.transform(features.numpy())
    return torch.tensor(normalized_features, dtype=torch.float)

def process_graph(node_csv_path, edge_csv_path, save_folder):
    try:
        dataset = CustomDataset(node_csv_path, edge_csv_path)
        graph_data = dataset.get()

        normalized_x = apply_normalization(graph_data.x, node_scaler)
        normalized_edge_attr = apply_normalization(graph_data.edge_attr, edge_scaler)

        normalized_graph = Data(
            x=normalized_x,
            edge_index=graph_data.edge_index,
            edge_attr=normalized_edge_attr,
            graph_id=graph_data.graph_id
        )

        if not os.path.exists(save_folder):
            os.makedirs(save_folder)

        graph_filename = os.path.splitext(os.path.basename(node_csv_path))[0] + ".pt"
        save_path = os.path.join(save_folder, graph_filename)
        torch.save(normalized_graph, save_path)

        print(f"Graph saved to: {save_path}")
        return str(save_path)
    except Exception as e:
        print(f"Error in process_graph: {e}")
        raise