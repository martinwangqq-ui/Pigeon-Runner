import os
import json
import random
from collections import deque

import cv2
import numpy as np
import torch

from sam2.build_sam import build_sam2
from sam2.sam2_image_predictor import SAM2ImagePredictor

IMAGE_PATH = "level_image.jpg" 
OUTPUT_JSON = "level.json" 
CHECKPOINT = "./checkpoints/sam2.1_hiera_tiny.pt"
MODEL_CFG = "configs/sam2.1/sam2.1_hiera_t.yaml"

DEVICE = "cpu"
PIXEL_TO_WORLD = 0.01 

START_X = None
START_Y = None

image_bgr = cv2.imread(IMAGE_PATH)
if image_bgr is None:
    raise FileNotFoundError(f"can not read the image: {IMAGE_PATH}")

h, w, _ = image_bgr.shape
if START_X is None or START_Y is None:
    START_X = w // 2
    START_Y = h // 2

print("Loading SAM2...")
sam_model = build_sam2(MODEL_CFG, CHECKPOINT, device=DEVICE)
predictor = SAM2ImagePredictor(sam_model)
print("SAM2 loading finished")

image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)
predictor.set_image(image_rgb)

input_point = np.array([[START_X, START_Y]]) 
input_label = np.array([1])                

with torch.inference_mode():
    masks, scores, _ = predictor.predict(
        point_coords=input_point,
        point_labels=input_label,
        multimask_output=False,
    )

mask = (masks[0].astype(np.uint8)) 
mask_bin = (mask > 0).astype(np.uint8)

print("Loading reachable regions...")

def flood_fill(mask_bin, start_x, start_y):
    h, w = mask_bin.shape
    visited = np.zeros_like(mask_bin, dtype=np.uint8)
    q = deque()
    if mask_bin[start_y, start_x] == 0:
        print("Warning: The starting point is not in the foreground area. You may need to manually specify the starting point.")
        return visited

    q.append((start_x, start_y))
    visited[start_y, start_x] = 1

    dirs = [(-1,0), (1,0), (0,-1), (0,1)]
    while q:
        x, y = q.popleft()
        for dx, dy in dirs:
            nx, ny = x + dx, y + dy
            if 0 <= nx < w and 0 <= ny < h:
                if mask_bin[ny, nx] == 1 and visited[ny, nx] == 0:
                    visited[ny, nx] = 1
                    q.append((nx, ny))
    return visited

reachable = flood_fill(mask_bin, START_X, START_Y)

if reachable.sum() == 0:
    raise RuntimeError("There are no reachable areas from the starting point. Please change the map or change the starting point.")

ys, xs = np.where(reachable == 1)
idx = random.randint(0, len(xs) - 1)
GOAL_X = int(xs[idx])
GOAL_Y = int(ys[idx])
print(f"Randomly generate the endpoint: ({GOAL_X}, {GOAL_Y})")

reachable_u8 = (reachable * 255).astype(np.uint8)
contours, _ = cv2.findContours(reachable_u8, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

if len(contours) == 0:
    raise RuntimeError("No reachable region outline found.")

largest = max(contours, key=cv2.contourArea)

epsilon = 2.0 
approx = cv2.approxPolyDP(largest, epsilon, True)

polygon_points = []
for p in approx:
    x, y = int(p[0][0]), int(p[0][1])
    polygon_points.append({"x": x, "y": y})


print(f"polygon points: {len(polygon_points)}")

level_data = {
    "image": IMAGE_PATH,
    "width": int(w),
    "height": int(h),
    "pixel_to_world": PIXEL_TO_WORLD,
    "pigeon_start": {"x": int(START_X), "y": int(START_Y)},
    "goal": {"x": int(GOAL_X), "y": int(GOAL_Y)},
    "ground_polygon": polygon_points
}

with open(OUTPUT_JSON, "w", encoding="utf-8") as f:
    json.dump(level_data, f, ensure_ascii=False, indent=2)

print("Finished", OUTPUT_JSON)
