<%@ Page Title="Facial Login" 
    Language="C#" 
    MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" 
    CodeBehind="FacialLogin.aspx.cs" 
    Inherits="badpjProject.FacialLogin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
  <div class="container mt-5">
    <div class="d-flex flex-column align-items-center">
      <h2>Facial Login</h2>
      <!-- Textbox for the user to enter their username -->
      <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control mb-3" 
                   placeholder="Enter Username"></asp:TextBox>
      <!-- Button to start the camera -->
      <input type="button" value="Access Camera" onclick="startCamera();" 
             class="btn btn-primary mb-3" />
      <!-- Video element to display the live camera feed -->
      <video id="videoElement" width="640" height="480" autoplay 
             style="border:1px solid #ccc;"></video>
      <br /><br />
      <!-- Hidden canvas for capturing the image -->
      <canvas id="canvas" width="640" height="480" style="display:none;"></canvas>
      <!-- Button to capture & display the facial data -->
      <input type="button" value="Capture & Display Facial Data" 
             onclick="captureAndDisplay();" class="btn btn-warning" />
      <br /><br />
      <!-- Image element to show the captured image -->
      <img id="capturedImage" src="" alt="Captured Facial Data" 
           style="border:1px solid #ccc; max-width:640px;" />
      <br /><br />
      <!-- Hidden field to store the captured face descriptor (as JSON) -->
      <asp:HiddenField ID="hfDescriptor" runat="server" />
      <!-- Button to submit the form for facial login -->
      <asp:Button ID="btnLogin" runat="server" Text="Login via Face" 
                  OnClick="btnLogin_Click" CssClass="btn btn-success" />
      <br /><br />
      <!-- Label to display the result -->
      <asp:Label ID="lblResult" runat="server" CssClass="alert alert-info"></asp:Label>
    </div>
  </div>

  <!-- Include jQuery and face-api.js -->
  <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/face-api.js@0.22.2/dist/face-api.min.js"></script>
  <script type="text/javascript">
      // Load face-api.js models from the /Facialmodels folder
      async function loadModels() {
          const modelUrl = '/Facialmodels';
          await faceapi.nets.ssdMobilenetv1.loadFromUri(modelUrl);
          await faceapi.nets.faceLandmark68Net.loadFromUri(modelUrl);
          await faceapi.nets.faceRecognitionNet.loadFromUri(modelUrl);
          console.log("Models loaded");
      }

      window.addEventListener('load', async () => {
          await loadModels();
      });

      // Start camera streaming into the video element
      function startCamera() {
          if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
              navigator.mediaDevices.getUserMedia({ video: true })
                  .then(function (stream) {
                      var video = document.getElementById('videoElement');
                      video.srcObject = stream;
                      video.play();
                  })
                  .catch(function (error) {
                      console.error("Error accessing camera:", error);
                      alert("Unable to access the camera. Please check your device permissions.");
                  });
          } else {
              alert("Your browser does not support camera access.");
          }
      }

      // Helper: Convert DataURL to Blob
      function dataURItoBlob(dataURI) {
          var byteString;
          if (dataURI.split(',')[0].indexOf('base64') >= 0)
              byteString = atob(dataURI.split(',')[1]);
          else
              byteString = decodeURI(dataURI.split(',')[1]);
          var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];
          var ab = new ArrayBuffer(byteString.length);
          var ia = new Uint8Array(ab);
          for (var i = 0; i < byteString.length; i++) {
              ia[i] = byteString.charCodeAt(i);
          }
          return new Blob([ia], { type: mimeString });
      }

      // Helper: Serialize the face descriptor (Float32Array) as JSON
      function getDescriptorJSON(detection) {
          return JSON.stringify(Array.from(detection.descriptor));
      }

      // Capture the image, perform face detection, and display it.
      // Also, store the computed descriptor (as JSON) in the hidden field.
      async function captureAndDisplay() {
          var canvas = document.getElementById('canvas');
          var video = document.getElementById('videoElement');
          var context = canvas.getContext('2d');

          // Capture current frame
          context.drawImage(video, 0, 0, canvas.width, canvas.height);
          var imageDataUrl = canvas.toDataURL("image/png");
          document.getElementById('capturedImage').src = imageDataUrl;
          console.log("Captured image displayed.");

          // Convert the image to a Blob and then process it with face-api.js
          const blob = dataURItoBlob(imageDataUrl);
          const img = await faceapi.bufferToImage(blob);

          // Detect a single face and compute its descriptor
          const detection = await faceapi.detectSingleFace(img)
              .withFaceLandmarks()
              .withFaceDescriptor();
          if (!detection) {
              alert("No face detected. Please try again.");
              return;
          }
          console.log("Face descriptor:", detection.descriptor);

          // Store the descriptor as JSON in the hidden field
          document.getElementById('<%=hfDescriptor.ClientID%>').value = getDescriptorJSON(detection);
      }
  </script>
</asp:Content>
