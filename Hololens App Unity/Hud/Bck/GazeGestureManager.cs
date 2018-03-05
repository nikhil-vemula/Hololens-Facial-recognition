using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !UNITY_EDITOR
using System.Net.Http;
using System.Net.Http.Headers;
#endif
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.UI;
public class GazeGestureManager : MonoBehaviour
{
    public Text Hello;
    private string filePath;
    public static GazeGestureManager Instance { get; private set; }

    GestureRecognizer recognizer;

    void Awake()
    {
        Instance = this;

        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.Tapped += (args) =>
        {
            Debug.Log("Air Tapped");
            //Capturing a photo to camera folder
            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
            //Performing facial recognition on that photo
            ProcessPhotoAsync();
        };
        recognizer.StartCapturingGestures();
    }

    PhotoCapture _photoCaptureObject = null;


    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        _photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            string filename = string.Format(@"terminator_analysis.jpg");
            filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            _photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDiskAsync);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }
    void OnCapturedPhotoToDiskAsync(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            _photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
            #if !UNITY_EDITOR
            var cameraRollFolder = Windows.Storage.KnownFolders.CameraRoll.Path;
            Debug.Log(cameraRollFolder);
            string p = cameraRollFolder + "/terminator_analysis.jpg";
            if (File.Exists(p))
            {
                File.Delete(p);
            }
            File.Move(filePath, Path.Combine(cameraRollFolder, "terminator_analysis.jpg"));
            Debug.Log(File.Exists(Path.Combine(cameraRollFolder, "terminator_analysis.jpg")));
            #endif
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }
    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        _photoCaptureObject.Dispose();
        _photoCaptureObject = null;
    }

    async void ProcessPhotoAsync(){
#if !UNITY_EDITOR
        Windows.Storage.StorageFolder storageFolder = Windows.Storage.KnownFolders.CameraRoll;
        Windows.Storage.StorageFile file = await storageFolder.GetFileAsync("terminator_analysis.jpg");
        if (file != null)
        {
            Debug.Log("File exists!!");
        }
        else
        {
            Debug.Log("File does not exists!!");
        }


        //convert filestream to byte array
        byte[] fileBytes;
        using (var fileStream = await file.OpenStreamForReadAsync())
        {
            var binaryReader = new BinaryReader(fileStream);
            fileBytes = binaryReader.ReadBytes((int)fileStream.Length);
        }

        HttpClient client = new HttpClient();
        //Ip Address of the server running face recognition service
        client.BaseAddress = new Uri("http://192.168.137.1:5000/");
        MultipartFormDataContent form = new MultipartFormDataContent();
        HttpContent content = new StringContent("file");
        form.Add(content, "file");
        var stream = await file.OpenStreamForReadAsync();
        content = new StreamContent(stream);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
        {
            Name = "file",
            FileName = file.Name
        };
        form.Add(content);
        String face = null;
        try
        {
            var response = await client.PostAsync("facerec", form);
            face = response.Content.ReadAsStringAsync().Result;
            Debug.Log(response.Content.ReadAsStringAsync().Result);
            Hello.text = response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception e) {
            Debug.Log(e);
        }
        //Hello.text = face;
#endif
    }

}