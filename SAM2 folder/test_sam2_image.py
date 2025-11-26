import cv2
import torch
import numpy as np
import json

from sam2.build_sam import build_sam2
from sam2.sam2_image_predictor import SAM2ImagePredictor

checkpoint = "./checkpoints/sam2.1_hiera_tiny.pt"
model_cfg = "configs/sam2.1/sam2.1_hiera_t.yaml"

def mask_to_polygon_json(mask, out_json_path):
    """
    mask: 0/255 Binary Image, np.ndarray, shape = (H, W)
    Export format：
    {
      "width": W,
      "height": H,
      "regions": [
        {
          "id": 0,
          "points": [x0, y0, x1, y1, ...]   // Pixel coordinates
        }
      ]
    }
    """
    import cv2
    h, w = mask.shape[:2]

    mask_u8 = mask.astype(np.uint8)

    contours, _ = cv2.findContours(mask_u8, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

    if not contours:
        print("No outlines found. Check if the mask is completely black.")
        return

    contour = max(contours, key=cv2.contourArea)

    epsilon = 0.005 * cv2.arcLength(contour, True)
    approx = cv2.approxPolyDP(contour, epsilon, True)

    points_flat = []
    for pt in approx:
        x, y = pt[0]
        points_flat.append(float(x))
        points_flat.append(float(y))

    data = {
        "width": int(w),
        "height": int(h),
        "regions": [
            {
                "id": 0,
                "points": points_flat
            }
        ]
    }

    with open(out_json_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print("JSON loading finished:", out_json_path)


def main():
    sam_model = build_sam2(model_cfg, checkpoint, device="cpu")
    predictor = SAM2ImagePredictor(sam_model)

    image_path = "test.jpg"  
    image_bgr = cv2.imread(image_path)
    if image_bgr is None:
        raise FileNotFoundError(f"couldn't find {image_path}，please make sure it is in the folder")

    image_rgb = cv2.cvtColor(image_bgr, cv2.COLOR_BGR2RGB)

    h, w, _ = image_rgb.shape
    print(f"Image Size：{w}x{h}")

    with torch.no_grad():
        predictor.set_image(image_rgb)

        input_point = np.array([[w // 2, h // 2]]) 
        input_label = np.array([1]) 

        masks, scores, _ = predictor.predict(
            point_coords=input_point,
            point_labels=input_label,
            multimask_output=False, 
        )

    mask = (masks[0].astype(np.uint8)) * 255

    overlay = image_bgr.copy()
    overlay[mask == 255] = (0, 0, 255) 

    alpha = 0.5
    result = cv2.addWeighted(overlay, alpha, image_bgr, 1 - alpha, 0)

    cv2.imwrite("test_mask_overlay.png", result)
    print("Already saved the highlight result to test_mask_overlay.png")

    cv2.imwrite("test_mask.png", mask)
    print("Binary mask has been saved to test_mask.png")

    mask_to_polygon_json(mask, "test_region.json")


if __name__ == "__main__":
    main()
