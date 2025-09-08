#!/usr/bin/env python3
"""
Real-time Key Detection and Announcement System
This script uses the trained model to detect and announce which key is shown
"""

import cv2
import numpy as np
import os
import pyttsx3
import threading
import time
import joblib
import tensorflow as tf
from collections import deque

class KeyDetector:
    def __init__(self, model_dir="models"):
        self.model_dir = model_dir
        
        # Initialize text-to-speech engine
        self.tts_engine = pyttsx3.init()
        self.tts_engine.setProperty('rate', 150)  # Speed of speech
        self.tts_engine.setProperty('volume', 0.9)  # Volume
        
        # Load model and encoder
        self.model = None
        self.label_encoder = None
        self.load_model()
        
        # Initialize camera
        self.cap = cv2.VideoCapture(0)
        if not self.cap.isOpened():
            print("Error: Could not open camera")
            exit()
        
        self.cap.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        self.cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
        
        # Detection parameters
        self.confidence_threshold = 0.7
        self.detection_history = deque(maxlen=10)  # Store last 10 predictions
        self.last_announcement = None
        self.last_announcement_time = 0
        self.announcement_cooldown = 2.0  # Seconds between announcements
        
        # For FPS calculation
        self.fps_start_time = time.time()
        self.fps_frame_count = 0
        self.fps = 0
        
    def load_model(self):
        """Load the trained model and label encoder"""
        model_path = os.path.join(self.model_dir, "key_classifier.h5")
        encoder_path = os.path.join(self.model_dir, "label_encoder.pkl")
        
        if not os.path.exists(model_path) or not os.path.exists(encoder_path):
            print("\nERROR: Model not found!")
            print("Please run 'python train_key_model.py' first to train the model.")
            exit()
        
        print("Loading model...")
        self.model = tf.keras.models.load_model(model_path)
        self.label_encoder = joblib.load(encoder_path)
        print("Model loaded successfully!")
        
    def speak(self, text):
        """Announce text using text-to-speech in a separate thread"""
        def _speak():
            self.tts_engine.say(text)
            self.tts_engine.runAndWait()
        
        # Run TTS in a separate thread to avoid blocking
        tts_thread = threading.Thread(target=_speak)
        tts_thread.daemon = True
        tts_thread.start()
    
    def preprocess_frame(self, frame):
        """Preprocess frame for model input"""
        # Resize to model input size
        resized = cv2.resize(frame, (128, 128))
        # Normalize pixel values
        normalized = resized.astype(np.float32) / 255.0
        # Add batch dimension
        return np.expand_dims(normalized, axis=0)
    
    def get_stable_prediction(self):
        """Get stable prediction from detection history"""
        if len(self.detection_history) < 5:
            return None, 0
        
        # Count occurrences of each class in recent history
        recent_predictions = list(self.detection_history)[-5:]
        class_counts = {}
        
        for pred_class, confidence in recent_predictions:
            if pred_class not in class_counts:
                class_counts[pred_class] = []
            class_counts[pred_class].append(confidence)
        
        # Find most common class
        if class_counts:
            most_common_class = max(class_counts.keys(), 
                                   key=lambda k: len(class_counts[k]))
            avg_confidence = np.mean(class_counts[most_common_class])
            
            # Only return if it appears in at least 3 of the last 5 frames
            if len(class_counts[most_common_class]) >= 3:
                return most_common_class, avg_confidence
        
        return None, 0
    
    def calculate_fps(self):
        """Calculate frames per second"""
        self.fps_frame_count += 1
        elapsed = time.time() - self.fps_start_time
        
        if elapsed > 1.0:
            self.fps = self.fps_frame_count / elapsed
            self.fps_frame_count = 0
            self.fps_start_time = time.time()
    
    def detect(self):
        """Main detection loop"""
        print("\n" + "="*60)
        print("KEY DETECTION SYSTEM STARTED")
        print("="*60)
        print("\nInstructions:")
        print("- Show Key 1 or Key 2 to the camera")
        print("- The system will detect and announce which key it sees")
        print("- Press 'q' to quit")
        print("- Press 's' to toggle sound on/off")
        print("\n" + "="*60)
        
        sound_enabled = True
        
        # Initial announcement
        self.speak("Key detection system ready. Show me a key.")
        
        while True:
            ret, frame = self.cap.read()
            if not ret:
                print("Error: Failed to grab frame")
                break
            
            # Calculate FPS
            self.calculate_fps()
            
            # Preprocess frame for prediction
            input_data = self.preprocess_frame(frame)
            
            # Make prediction
            predictions = self.model.predict(input_data, verbose=0)
            predicted_class_idx = np.argmax(predictions[0])
            confidence = predictions[0][predicted_class_idx]
            
            # Get class name
            predicted_class = self.label_encoder.inverse_transform([predicted_class_idx])[0]
            
            # Add to detection history
            if confidence > self.confidence_threshold:
                self.detection_history.append((predicted_class, confidence))
            else:
                self.detection_history.append((None, 0))
            
            # Get stable prediction
            stable_class, stable_confidence = self.get_stable_prediction()
            
            # Prepare display
            display_frame = frame.copy()
            
            # Draw detection info
            if stable_class:
                # Draw bounding box around center region
                h, w = frame.shape[:2]
                box_size = 200
                x1, y1 = w//2 - box_size//2, h//2 - box_size//2
                x2, y2 = w//2 + box_size//2, h//2 + box_size//2
                
                # Color based on detected key
                if stable_class == "key1":
                    color = (0, 255, 0)  # Green for Key 1
                    display_text = "KEY 1 DETECTED"
                else:
                    color = (0, 0, 255)  # Red for Key 2
                    display_text = "KEY 2 DETECTED"
                
                cv2.rectangle(display_frame, (x1, y1), (x2, y2), color, 3)
                
                # Add text
                cv2.putText(display_frame, display_text, 
                           (w//2 - 100, 50), cv2.FONT_HERSHEY_BOLD, 1, color, 2)
                cv2.putText(display_frame, f"Confidence: {stable_confidence:.1%}", 
                           (w//2 - 80, 80), cv2.FONT_HERSHEY_SIMPLEX, 0.7, color, 2)
                
                # Announce detection (with cooldown)
                current_time = time.time()
                if sound_enabled and stable_class != self.last_announcement:
                    if current_time - self.last_announcement_time > self.announcement_cooldown:
                        if stable_class == "key1":
                            self.speak("Key one detected")
                        else:
                            self.speak("Key two detected")
                        self.last_announcement = stable_class
                        self.last_announcement_time = current_time
            else:
                # No stable detection
                cv2.putText(display_frame, "Show a key to the camera", 
                           (150, 50), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (255, 255, 0), 2)
                
                # Draw guide box
                h, w = frame.shape[:2]
                box_size = 200
                x1, y1 = w//2 - box_size//2, h//2 - box_size//2
                x2, y2 = w//2 + box_size//2, h//2 + box_size//2
                cv2.rectangle(display_frame, (x1, y1), (x2, y2), (128, 128, 128), 1)
                
                # Reset last announcement if no detection
                if len(self.detection_history) == 10 and all(d[0] is None for d in self.detection_history):
                    self.last_announcement = None
            
            # Add status info
            status_text = f"FPS: {self.fps:.1f} | Sound: {'ON' if sound_enabled else 'OFF'}"
            cv2.putText(display_frame, status_text, 
                       (10, frame.shape[0] - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 255, 0), 2)
            
            # Add current prediction info (for debugging)
            if confidence > self.confidence_threshold:
                cv2.putText(display_frame, f"Current: {predicted_class} ({confidence:.1%})", 
                           (10, 30), cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 2)
            
            # Show frame
            cv2.imshow('Key Detection System', display_frame)
            
            # Handle key presses
            key = cv2.waitKey(1) & 0xFF
            if key == ord('q'):
                break
            elif key == ord('s'):
                sound_enabled = not sound_enabled
                print(f"Sound {'enabled' if sound_enabled else 'disabled'}")
        
        self.cleanup()
    
    def cleanup(self):
        """Release resources"""
        self.cap.release()
        cv2.destroyAllWindows()
        print("\nKey detection system stopped.")

class SimpleKeyDetector:
    """Alternative detector using traditional CV methods"""
    
    def __init__(self):
        # Initialize camera
        self.cap = cv2.VideoCapture(0)
        if not self.cap.isOpened():
            print("Error: Could not open camera")
            exit()
        
        # Initialize TTS
        self.tts_engine = pyttsx3.init()
        self.tts_engine.setProperty('rate', 150)
        
        # ORB detector for feature matching
        self.orb = cv2.ORB_create()
        self.matcher = cv2.BFMatcher(cv2.NORM_HAMMING, crossCheck=True)
        
        # Reference images will be loaded if available
        self.key1_descriptors = None
        self.key2_descriptors = None
        self.load_references()
        
    def load_references(self):
        """Load reference images for keys"""
        key1_path = "key_dataset/key1"
        key2_path = "key_dataset/key2"
        
        if os.path.exists(key1_path):
            key1_files = os.listdir(key1_path)[:5]  # Use first 5 images
            for f in key1_files:
                img = cv2.imread(os.path.join(key1_path, f), 0)
                if img is not None:
                    kp, des = self.orb.detectAndCompute(img, None)
                    if des is not None:
                        if self.key1_descriptors is None:
                            self.key1_descriptors = des
                        else:
                            self.key1_descriptors = np.vstack([self.key1_descriptors, des])
        
        if os.path.exists(key2_path):
            key2_files = os.listdir(key2_path)[:5]
            for f in key2_files:
                img = cv2.imread(os.path.join(key2_path, f), 0)
                if img is not None:
                    kp, des = self.orb.detectAndCompute(img, None)
                    if des is not None:
                        if self.key2_descriptors is None:
                            self.key2_descriptors = des
                        else:
                            self.key2_descriptors = np.vstack([self.key2_descriptors, des])
    
    def detect_simple(self):
        """Simple detection using feature matching"""
        print("\nRunning simple detector (feature matching)...")
        
        while True:
            ret, frame = self.cap.read()
            if not ret:
                break
            
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
            kp, des = self.orb.detectAndCompute(gray, None)
            
            display_frame = frame.copy()
            
            if des is not None and self.key1_descriptors is not None:
                # Match with key1
                matches1 = self.matcher.match(des, self.key1_descriptors)
                matches1 = sorted(matches1, key=lambda x: x.distance)
                
                # Match with key2
                matches2 = []
                if self.key2_descriptors is not None:
                    matches2 = self.matcher.match(des, self.key2_descriptors)
                    matches2 = sorted(matches2, key=lambda x: x.distance)
                
                # Determine which key based on match quality
                if len(matches1) > 20 and (not matches2 or len(matches1) > len(matches2)):
                    cv2.putText(display_frame, "KEY 1 DETECTED", 
                               (200, 50), cv2.FONT_HERSHEY_BOLD, 1, (0, 255, 0), 2)
                elif len(matches2) > 20:
                    cv2.putText(display_frame, "KEY 2 DETECTED", 
                               (200, 50), cv2.FONT_HERSHEY_BOLD, 1, (0, 0, 255), 2)
            
            cv2.imshow('Simple Key Detection', display_frame)
            
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
        
        self.cap.release()
        cv2.destroyAllWindows()

if __name__ == "__main__":
    # Check if model exists
    if os.path.exists("models/key_classifier.h5"):
        detector = KeyDetector()
        detector.detect()
    else:
        print("\nModel not found. Trying simple detector...")
        simple_detector = SimpleKeyDetector()
        simple_detector.detect_simple()