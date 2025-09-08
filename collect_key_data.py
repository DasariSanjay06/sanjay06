#!/usr/bin/env python3
"""
Data Collection Script for Key Detection
This script helps collect training images for two different keys
"""

import cv2
import os
import time
from datetime import datetime

class KeyDataCollector:
    def __init__(self, data_dir="key_dataset"):
        self.data_dir = data_dir
        self.key1_dir = os.path.join(data_dir, "key1")
        self.key2_dir = os.path.join(data_dir, "key2")
        
        # Create directories if they don't exist
        os.makedirs(self.key1_dir, exist_ok=True)
        os.makedirs(self.key2_dir, exist_ok=True)
        
        # Initialize camera
        self.cap = cv2.VideoCapture(0)
        if not self.cap.isOpened():
            print("Error: Could not open camera")
            exit()
        
        # Set camera properties for better quality
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
        
    def collect_samples(self):
        """Main collection loop"""
        print("\n" + "="*60)
        print("KEY DATA COLLECTION SYSTEM")
        print("="*60)
        print("\nInstructions:")
        print("- Press '1' to capture images of Key 1")
        print("- Press '2' to capture images of Key 2")
        print("- Press 'SPACE' to capture current frame")
        print("- Press 'q' to quit")
        print("- Move the key to different positions and angles")
        print("- Collect at least 50 images per key for best results")
        print("\n" + "="*60)
        
        current_key = None
        key1_count = len(os.listdir(self.key1_dir))
        key2_count = len(os.listdir(self.key2_dir))
        
        while True:
            ret, frame = self.cap.read()
            if not ret:
                print("Error: Failed to grab frame")
                break
            
            # Display frame with overlay information
            display_frame = frame.copy()
            
            # Add status text
            cv2.putText(display_frame, f"Key 1 samples: {key1_count}", 
                       (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
            cv2.putText(display_frame, f"Key 2 samples: {key2_count}", 
                       (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.7, (0, 255, 0), 2)
            
            if current_key:
                cv2.putText(display_frame, f"Currently collecting: Key {current_key}", 
                           (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (0, 0, 255), 2)
                cv2.putText(display_frame, "Press SPACE to capture", 
                           (10, 130), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 0), 2)
            else:
                cv2.putText(display_frame, "Press 1 or 2 to select key", 
                           (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 0, 0), 2)
            
            # Draw center crosshair for positioning
            h, w = display_frame.shape[:2]
            cv2.line(display_frame, (w//2 - 20, h//2), (w//2 + 20, h//2), (0, 255, 0), 2)
            cv2.line(display_frame, (w//2, h//2 - 20), (w//2, h//2 + 20), (0, 255, 0), 2)
            
            cv2.imshow('Key Data Collection', display_frame)
            
            key = cv2.waitKey(1) & 0xFF
            
            if key == ord('q'):
                break
            elif key == ord('1'):
                current_key = 1
                print(f"\nNow collecting data for Key 1")
                print(f"Current samples: {key1_count}")
            elif key == ord('2'):
                current_key = 2
                print(f"\nNow collecting data for Key 2")
                print(f"Current samples: {key2_count}")
            elif key == ord(' ') and current_key:
                # Capture frame
                timestamp = datetime.now().strftime("%Y%m%d_%H%M%S_%f")
                
                if current_key == 1:
                    filename = os.path.join(self.key1_dir, f"key1_{timestamp}.jpg")
                    cv2.imwrite(filename, frame)
                    key1_count += 1
                    print(f"Captured Key 1 image #{key1_count}")
                elif current_key == 2:
                    filename = os.path.join(self.key2_dir, f"key2_{timestamp}.jpg")
                    cv2.imwrite(filename, frame)
                    key2_count += 1
                    print(f"Captured Key 2 image #{key2_count}")
                
                # Flash effect for feedback
                cv2.imshow('Key Data Collection', 255 - display_frame)
                cv2.waitKey(100)
        
        self.cleanup()
        
    def cleanup(self):
        """Release resources"""
        self.cap.release()
        cv2.destroyAllWindows()
        
        # Print summary
        key1_count = len(os.listdir(self.key1_dir))
        key2_count = len(os.listdir(self.key2_dir))
        
        print("\n" + "="*60)
        print("DATA COLLECTION COMPLETE")
        print("="*60)
        print(f"Key 1 samples collected: {key1_count}")
        print(f"Key 2 samples collected: {key2_count}")
        
        if key1_count < 30 or key2_count < 30:
            print("\nWARNING: You should collect at least 30 samples per key")
            print("for better accuracy. Run this script again to add more samples.")
        else:
            print("\nGreat! You have enough samples to train the model.")
            print("Run 'python train_key_model.py' to train the classifier.")
        print("="*60)

if __name__ == "__main__":
    collector = KeyDataCollector()
    collector.collect_samples()