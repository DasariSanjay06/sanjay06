#!/usr/bin/env python3
"""
Training Script for Key Classification Model
This script trains a CNN model to distinguish between two keys
"""

import os
import cv2
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers
import joblib

class KeyModelTrainer:
    def __init__(self, data_dir="key_dataset", model_dir="models"):
        self.data_dir = data_dir
        self.model_dir = model_dir
        os.makedirs(self.model_dir, exist_ok=True)
        
        self.img_size = (128, 128)
        self.model = None
        self.label_encoder = LabelEncoder()
        
    def load_and_preprocess_data(self):
        """Load images and prepare training data"""
        print("\n" + "="*60)
        print("LOADING AND PREPROCESSING DATA")
        print("="*60)
        
        images = []
        labels = []
        
        # Load Key 1 images
        key1_dir = os.path.join(self.data_dir, "key1")
        if os.path.exists(key1_dir):
            key1_files = os.listdir(key1_dir)
            print(f"Loading {len(key1_files)} images for Key 1...")
            for filename in key1_files:
                img_path = os.path.join(key1_dir, filename)
                img = cv2.imread(img_path)
                if img is not None:
                    img = cv2.resize(img, self.img_size)
                    images.append(img)
                    labels.append("key1")
        
        # Load Key 2 images
        key2_dir = os.path.join(self.data_dir, "key2")
        if os.path.exists(key2_dir):
            key2_files = os.listdir(key2_dir)
            print(f"Loading {len(key2_files)} images for Key 2...")
            for filename in key2_files:
                img_path = os.path.join(key2_dir, filename)
                img = cv2.imread(img_path)
                if img is not None:
                    img = cv2.resize(img, self.img_size)
                    images.append(img)
                    labels.append("key2")
        
        if len(images) == 0:
            print("\nERROR: No images found!")
            print("Please run 'python collect_key_data.py' first to collect training data.")
            return None, None, None, None
        
        # Convert to numpy arrays
        X = np.array(images, dtype=np.float32) / 255.0  # Normalize pixel values
        y = np.array(labels)
        
        # Encode labels
        y_encoded = self.label_encoder.fit_transform(y)
        
        # Split data
        X_train, X_test, y_train, y_test = train_test_split(
            X, y_encoded, test_size=0.2, random_state=42, stratify=y_encoded
        )
        
        print(f"\nData split:")
        print(f"Training samples: {len(X_train)}")
        print(f"Testing samples: {len(X_test)}")
        print(f"Classes: {self.label_encoder.classes_}")
        
        return X_train, X_test, y_train, y_test
    
    def create_model(self):
        """Create CNN model for key classification"""
        model = keras.Sequential([
            # Convolutional layers
            layers.Conv2D(32, (3, 3), activation='relu', input_shape=(128, 128, 3)),
            layers.MaxPooling2D((2, 2)),
            layers.Conv2D(64, (3, 3), activation='relu'),
            layers.MaxPooling2D((2, 2)),
            layers.Conv2D(128, (3, 3), activation='relu'),
            layers.MaxPooling2D((2, 2)),
            
            # Dense layers
            layers.Flatten(),
            layers.Dropout(0.5),
            layers.Dense(128, activation='relu'),
            layers.Dropout(0.5),
            layers.Dense(2, activation='softmax')  # 2 classes: key1 and key2
        ])
        
        model.compile(
            optimizer='adam',
            loss='sparse_categorical_crossentropy',
            metrics=['accuracy']
        )
        
        return model
    
    def train(self):
        """Train the model"""
        # Load data
        X_train, X_test, y_train, y_test = self.load_and_preprocess_data()
        
        if X_train is None:
            return False
        
        print("\n" + "="*60)
        print("TRAINING MODEL")
        print("="*60)
        
        # Create model
        self.model = self.create_model()
        print("\nModel architecture:")
        self.model.summary()
        
        # Data augmentation
        data_augmentation = keras.Sequential([
            layers.RandomFlip("horizontal"),
            layers.RandomRotation(0.1),
            layers.RandomZoom(0.1),
        ])
        
        # Apply augmentation to training data
        X_train_aug = np.array([data_augmentation(img, training=True) for img in X_train])
        
        # Train model
        print("\nTraining started...")
        history = self.model.fit(
            X_train_aug, y_train,
            epochs=20,
            batch_size=32,
            validation_data=(X_test, y_test),
            verbose=1
        )
        
        # Evaluate model
        print("\n" + "="*60)
        print("EVALUATION")
        print("="*60)
        test_loss, test_accuracy = self.model.evaluate(X_test, y_test, verbose=0)
        print(f"Test accuracy: {test_accuracy:.2%}")
        print(f"Test loss: {test_loss:.4f}")
        
        # Save model and label encoder
        model_path = os.path.join(self.model_dir, "key_classifier.h5")
        encoder_path = os.path.join(self.model_dir, "label_encoder.pkl")
        
        self.model.save(model_path)
        joblib.dump(self.label_encoder, encoder_path)
        
        print("\n" + "="*60)
        print("MODEL SAVED")
        print("="*60)
        print(f"Model saved to: {model_path}")
        print(f"Label encoder saved to: {encoder_path}")
        print("\nTraining complete! Run 'python detect_keys.py' to start detection.")
        print("="*60)
        
        return True
    
    def create_simple_model(self):
        """Alternative: Create a simpler model using traditional CV features"""
        print("\nCreating alternative simple model using HOG features...")
        
        from sklearn.svm import SVC
        from sklearn.preprocessing import StandardScaler
        
        # Load data
        X_train, X_test, y_train, y_test = self.load_and_preprocess_data()
        
        if X_train is None:
            return False
        
        # Extract HOG features
        from skimage.feature import hog
        
        def extract_hog_features(images):
            features = []
            for img in images:
                gray = cv2.cvtColor((img * 255).astype(np.uint8), cv2.COLOR_BGR2GRAY)
                hog_features = hog(gray, orientations=9, pixels_per_cell=(8, 8),
                                  cells_per_block=(2, 2), visualize=False)
                features.append(hog_features)
            return np.array(features)
        
        print("Extracting HOG features...")
        X_train_hog = extract_hog_features(X_train)
        X_test_hog = extract_hog_features(X_test)
        
        # Scale features
        scaler = StandardScaler()
        X_train_scaled = scaler.fit_transform(X_train_hog)
        X_test_scaled = scaler.transform(X_test_hog)
        
        # Train SVM
        print("Training SVM classifier...")
        svm_model = SVC(kernel='rbf', probability=True)
        svm_model.fit(X_train_scaled, y_train)
        
        # Evaluate
        accuracy = svm_model.score(X_test_scaled, y_test)
        print(f"SVM Test accuracy: {accuracy:.2%}")
        
        # Save alternative model
        joblib.dump(svm_model, os.path.join(self.model_dir, "svm_classifier.pkl"))
        joblib.dump(scaler, os.path.join(self.model_dir, "scaler.pkl"))
        
        return True

if __name__ == "__main__":
    trainer = KeyModelTrainer()
    
    # Check if dataset exists
    if not os.path.exists("key_dataset"):
        print("\nERROR: Dataset directory not found!")
        print("Please run 'python collect_key_data.py' first to collect training data.")
    else:
        success = trainer.train()
        if not success:
            print("\nTrying alternative model...")
            trainer.create_simple_model()