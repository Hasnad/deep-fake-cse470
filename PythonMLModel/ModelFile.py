import json

import cv2
import torch
import torch.nn.functional as F
from PIL import Image
from facenet_pytorch import MTCNN, InceptionResnetV1
from pytorch_grad_cam import GradCAM
from pytorch_grad_cam.utils.image import show_cam_on_image
from pytorch_grad_cam.utils.model_targets import ClassifierOutputTarget
from flask import Flask, request, jsonify
import base64
from io import BytesIO
from PIL import ImageFile

ImageFile.LOAD_TRUNCATED_IMAGES = True

mtcnn: MTCNN = None
DEVICE = ""
model: InceptionResnetV1 = None

is_initialized = False


def initialize_model():
    global mtcnn, DEVICE, model, is_initialized
    DEVICE = 'cuda:0' if torch.cuda.is_available() else 'cpu'

    mtcnn = MTCNN(
        select_largest=False,
        post_process=False,
        device=DEVICE
    ).to(DEVICE).eval()

    model = InceptionResnetV1(
        pretrained="vggface2",
        classify=True,
        num_classes=1,
        device=DEVICE
    )
    
    checkpoint = torch.load("resnetinceptionv1_epoch_32.pth", map_location=torch.device('cpu'))
    model.load_state_dict(checkpoint['model_state_dict'])
    model.to(DEVICE)
    model.eval()
    is_initialized = True


def predict(input_image: Image.Image):
    if not is_initialized:
        initialize_model()
    """Predict the label of the input_image"""
    face = mtcnn(input_image)
    if face is None:
        raise Exception('No face detected')
    face = face.unsqueeze(0)  # add the batch dimension
    face = F.interpolate(face, size=(256, 256), mode='bilinear', align_corners=False)

    # convert the face into a numpy array to be able to plot it
    prev_face = face.squeeze(0).permute(1, 2, 0).cpu().detach().int().numpy()
    prev_face = prev_face.astype('uint8')

    face = face.to(DEVICE)
    face = face.to(torch.float32)
    face = face / 255.0
    face_image_to_plot = face.squeeze(0).permute(1, 2, 0).cpu().detach().int().numpy()

    target_layers = [model.block8.branch1[-1]]
    use_cuda = True if torch.cuda.is_available() else False
    cam = GradCAM(model=model, target_layers=target_layers, use_cuda=use_cuda)
    targets = [ClassifierOutputTarget(0)]

    grayscale_cam = cam(input_tensor=face, targets=targets, eigen_smooth=True)
    grayscale_cam = grayscale_cam[0, :]
    visualization = show_cam_on_image(face_image_to_plot, grayscale_cam, use_rgb=True)
    face_with_mask = cv2.addWeighted(prev_face, 1, visualization, 0.5, 0)

    with torch.no_grad():
        output = torch.sigmoid(model(face).squeeze(0))
        prediction = "real" if output.item() < 0.5 else "fake"

        real_prediction = 1 - output.item()
        fake_prediction = output.item()

        confidences = {
            'real': real_prediction,
            'fake': fake_prediction
        }
    return confidences, face_with_mask


app = Flask(__name__)


@app.route('/process_image', methods=['POST'])
def process_image():
    try:
        data = request.get_json()
        data = json.loads(data)
        if 'image' in data:
            image_data = data['image']

            # Convert base64 string to bytes
            image_bytes = base64.b64decode(image_data)

            # Create a PIL Image from the bytes
            pil_image = Image.open(BytesIO(image_bytes))

            # Process the image as needed
            # For example, you can save the image to a file
            # pil_image.save('processed_image.jpg')
            confidences, face_with_mask = predict(pil_image)
            face_mask = Image.fromarray(face_with_mask)
            buffered = BytesIO()
            face_mask.save(buffered, format="JPEG")
            face_mask = base64.b64encode(buffered.getvalue())
            face_with_mask_decode = face_mask.decode('utf-8')

            return jsonify(
                {'success': True, 'message': 'Image processed successfully', 'confidence': confidences,
                 'face_with_mask': face_with_mask_decode})
        else:
            return jsonify({'success': False, 'message': 'Missing image data'})
    except Exception as e:
        return jsonify({'success': False, 'message': f'Error processing image: {str(e)}'})


if __name__ == '__main__':
    app.run(debug=True)
    print("Server started successfully!")
