# Key Detection System with Computer Vision

A real-time key detection system that can identify and announce different keys using computer vision and machine learning.

## Features

- **Automatic Key Recognition**: Detects and identifies two different keys (Key 1 and Key 2)
- **Voice Announcements**: Speaks out which key is detected using text-to-speech
- **Real-time Detection**: Processes camera feed in real-time with confidence scoring
- **Training System**: Includes data collection and model training capabilities

## System Architecture

The system consists of three main components:

1. **Data Collection** (`collect_key_data.py`): Captures training images of your keys
2. **Model Training** (`train_key_model.py`): Trains a CNN model to classify keys
3. **Detection System** (`detect_keys.py`): Real-time detection and announcement

## Installation

### Prerequisites

- Python 3.8 or higher
- Webcam/Camera connected to your system
- Linux/Windows/Mac OS

### Setup

1. Clone or download this repository

2. Install required dependencies:
```bash
pip install -r requirements.txt
```

3. For text-to-speech on Linux, you may need:
```bash
sudo apt-get install espeak
```

## Usage

### Step 1: Collect Training Data

First, you need to collect images of your two keys:

```bash
python collect_key_data.py
```

**Instructions:**
- Press `1` to start collecting images for Key 1
- Press `2` to start collecting images for Key 2
- Press `SPACE` to capture the current frame
- Press `q` to quit

**Tips for better accuracy:**
- Collect at least 50 images per key
- Vary the angle, position, and lighting
- Hold the key at different distances
- Rotate the key to capture different orientations
- Use the center crosshair as a guide for positioning

### Step 2: Train the Model

Once you have collected enough images, train the classification model:

```bash
python train_key_model.py
```

The training process will:
- Load and preprocess your collected images
- Create a CNN (Convolutional Neural Network) model
- Train for 20 epochs with data augmentation
- Save the trained model to the `models/` directory
- Display training accuracy and validation metrics

Expected output:
- Training accuracy: >90%
- Model saved to: `models/key_classifier.h5`

### Step 3: Run Key Detection

Start the real-time key detection system:

```bash
python detect_keys.py
```

**Controls:**
- Show Key 1 or Key 2 to the camera
- Press `s` to toggle sound on/off
- Press `q` to quit

**Features:**
- Green box and text for Key 1 detection
- Red box and text for Key 2 detection
- Voice announcement: "Key one detected" or "Key two detected"
- Confidence percentage display
- FPS counter
- Stable detection using prediction history

## How It Works

### Detection Pipeline

1. **Image Capture**: Captures frames from webcam at ~30 FPS
2. **Preprocessing**: Resizes images to 128x128 and normalizes pixel values
3. **Prediction**: CNN model predicts which key is shown
4. **Stabilization**: Uses last 10 predictions to ensure stable detection
5. **Announcement**: Text-to-speech announces the detected key

### Model Architecture

The CNN model consists of:
- 3 Convolutional layers with MaxPooling
- Dropout layers for regularization
- Dense layers for classification
- Softmax output for 2 classes

### Key Features

- **Confidence Threshold**: Only detects keys with >70% confidence
- **Detection History**: Requires consistent detection across multiple frames
- **Announcement Cooldown**: 2-second delay between voice announcements
- **Real-time Performance**: Optimized for smooth real-time operation

## Troubleshooting

### Camera Issues
- Ensure your webcam is properly connected
- Check camera permissions in your OS settings
- Try changing the camera index in the code (0, 1, 2, etc.)

### Low Accuracy
- Collect more training images (100+ per key recommended)
- Ensure good lighting conditions
- Keep consistent background during training and detection
- Make sure keys are visually distinct

### Audio Issues
- Install system TTS dependencies:
  - Linux: `sudo apt-get install espeak`
  - Windows: Should work out of the box
  - Mac: `brew install espeak`

### Performance Issues
- Reduce camera resolution in the code
- Use a more powerful GPU if available
- Reduce model complexity if needed

## Advanced Configuration

### Adjusting Detection Sensitivity

Edit `detect_keys.py`:
```python
self.confidence_threshold = 0.7  # Increase for stricter detection
self.announcement_cooldown = 2.0  # Seconds between announcements
```

### Changing Model Parameters

Edit `train_key_model.py`:
```python
epochs=20  # Increase for more training
batch_size=32  # Adjust based on your system memory
```

### Adding More Keys

To detect more than 2 keys:
1. Create additional directories in `key_dataset/` (key3, key4, etc.)
2. Modify the data collection script to handle more keys
3. Update the model output layer to match the number of classes
4. Retrain the model

## Project Structure

```
.
├── collect_key_data.py      # Data collection script
├── train_key_model.py        # Model training script
├── detect_keys.py            # Real-time detection script
├── requirements.txt          # Python dependencies
├── README.md                 # This file
├── key_dataset/              # Training data (created automatically)
│   ├── key1/                 # Images of Key 1
│   └── key2/                 # Images of Key 2
└── models/                   # Trained models (created automatically)
    ├── key_classifier.h5     # Main CNN model
    └── label_encoder.pkl     # Label encoder
```

## Tips for Best Results

1. **Consistent Background**: Use a plain, consistent background
2. **Good Lighting**: Ensure adequate and consistent lighting
3. **Clear Keys**: Use keys that are visually distinct from each other
4. **Distance**: Keep keys at a consistent distance from camera
5. **Quality Data**: More training data generally leads to better accuracy

## Future Enhancements

Possible improvements:
- Support for multiple key detection simultaneously
- Mobile app version
- Cloud-based model training
- Key tracking with bounding boxes
- Support for more object types
- Integration with smart home systems

## License

This project is open source and available for educational purposes.

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Ensure all dependencies are correctly installed
3. Verify your Python version (3.8+)
4. Check that your camera is working with other applications