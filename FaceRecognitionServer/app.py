from flask import Flask, jsonify, request
import dlib
import scipy.misc
import numpy as np
import os
from skimage import io


app = Flask(__name__)

face_detector = dlib.get_frontal_face_detector()
shape_predictor = dlib.shape_predictor('shape_predictor_68_face_landmarks.dat')
face_recognition_model = dlib.face_recognition_model_v1('dlib_face_recognition_resnet_model_v1.dat')
TOLERANCE = 0.6

def get_face_encodings(image):
    detected_faces = face_detector(image, 1)
    shapes_faces = [shape_predictor(image, face) for face in detected_faces]
    return [np.array(face_recognition_model.compute_face_descriptor(image, face_pose, 1)) for face_pose in shapes_faces]

def compare_face_encodings(known_faces, face):
    return (np.linalg.norm(known_faces - face, axis=1) <= TOLERANCE)

def find_match(known_faces, names, face):
    matches = compare_face_encodings(known_faces, face)
    count = 0
    for match in matches:
        if match:
            return names[count]
        count += 1
    return 'Not Found'

@app.route('/')
def index():
    return '''<html>
    <body>
    
        <form action = "http://localhost:5000/facerec" method = "POST" 
            enctype = "multipart/form-data">
            <input type = "file" name = "file" />
            <input type = "submit"/>
        </form>
        
    </body>
    </html>'''

@app.route('/facerec', methods=["POST"])
def facerec():
    print (request.headers)
    face_data = filter(lambda x: x.endswith('.npy'), os.listdir('faces/'))
    face_data = sorted(face_data)
    names = [x[:-4] for x in face_data]
    paths_to_facedata = ['faces/' + x for x in face_data]
    face_encodings = []
    for path in paths_to_facedata:
        face_encodings.append(np.load(path))   
    image = scipy.misc.imread(request.files['file'])
    face_encodings_in_image = get_face_encodings(image)
    if len(face_encodings_in_image) != 1:
        print("Please change image: " + path_to_image + " - it has " + str(len(face_encodings_in_image)) + " faces; it can only have one")
        exit()
    match = find_match(face_encodings, names, face_encodings_in_image[0])
    return match

if __name__ == '__main__':
    ## Replace the ip address
    app.run(host="xxxx",port=5000)