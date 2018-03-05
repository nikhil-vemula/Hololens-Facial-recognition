# Facerecognition in HoloLens

On Air tap gesture photo is captured and sent to a local server for facial recognition

### Hololens app is written using,
 * unity
 * visual studio
### Facial recognition server is written using

[dlib](http://dlib.net/) is a machine learning library used to detect and recognize the faces

[flask](http://flask.pocoo.org/) is a micro framework to create restful server using python

## Instructions

* Open cmd prompt, navigate to the FaceRecognitionServer directory

* Create conda environment with necessary packages using environment.yml file for running the facial recognition software
```
conda env create -f environment.yml
```
* Activate the environment
```
conda activate ./environment
```
* Enable Mobile hotspot in Windows 10 using Settings > Network & security > Mobile Hotspot. Turn On

![Hotspot](url)


* Edit the IP address in app.py replacing it with ip address found in cmd > ipconfig

![ipaddress](url)

* Run app.py using python
```
python app.py
```

* Connect the hololens to the same hotspot
* Replace the ip address in Hololens App Unity > Hud > Assets > Scripts > AirTapPhoto.cs
* Run the app in Hololens
* AirTap to see the result


## Packages Used
* dlib 
```
conda install -c conda-forge dlib
```
* Flask
```
conda install Flask
```
* Scikit-image
```
conda install scikit-image
```