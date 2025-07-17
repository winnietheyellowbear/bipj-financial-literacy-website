<%@ Page Title="Enable Facial Authentication" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav_loggedin.master" 
    AutoEventWireup="true" 
    CodeBehind="EnableFacialAuthentication.aspx.cs" 
    Inherits="badpjProject.EnableFacialAuthentication" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

  <div class="container mt-5">
    <div class="d-flex flex-column align-items-center">
      <h2>Enable Facial Authentication</h2>
      <!-- Display current username -->
      <asp:Label ID="UserLabel" runat="server" CssClass="h5 mb-3" Text=""></asp:Label>

      
      <!-- Button to start the camera -->
      <input type="button" value="Access Camera" onclick="startCamera();" class="btn btn-primary mb-3" />
      
      <!-- Video element for live camera feed -->
      <video id="videoElement" width="640" height="480" autoplay style="border: 1px solid #ccc;"></video>
      <br /><br />
      
      <!-- Hidden canvas for capturing image frame -->
      <canvas id="canvas" width="640" height="480" style="display: none;"></canvas>
      
      <!-- Button to capture image, process with face-api.js, and display it locally -->
      <input type="button" value="Capture & Process Facial Data" onclick="captureAndDisplay();" class="btn btn-warning" />
      <br /><br />
      
      <!-- Image element to display the captured image -->
      <img id="capturedImage" src="" alt="Captured Facial Data" style="border:1px solid #ccc; max-width:640px;" />
      <br /><br />
      
      <!-- Hidden field to store the computed face descriptor (as JSON) -->
      <asp:HiddenField ID="hfDescriptor" runat="server" />
      
      <!-- Button to enroll facial data (full postback) -->
      <asp:Button ID="btnEnroll" runat="server" Text="Enroll Facial Data" OnClick="btnEnroll_Click" CssClass="btn btn-success" />
      <br /><br />
      
      <!-- Label to display feedback messages -->
      <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-info"></asp:Label>
    </div>
  </div>

  <!-- Include jQuery and face-api.js -->
  <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/face-api.js@0.22.2/dist/face-api.min.js"></script>
  <script type="text/javascript">
      // Set up global AJAX settings (if needed)
      $.ajaxSetup({
          xhrFields: { withCredentials: true }
      });

      // Load face-api.js models from the /Facialmodels folder
      async function loadModels() {
          const modelUrl = '<%= ResolveUrl("~/Facialmodels") %>';

          await faceapi.nets.ssdMobilenetv1.loadFromUri(modelUrl);
          await faceapi.nets.faceLandmark68Net.loadFromUri(modelUrl);
          await faceapi.nets.faceRecognitionNet.loadFromUri(modelUrl);
          console.log("Models loaded");
      }

      // On page load, load the models and display the username from session
      window.addEventListener('load', async () => {
          await loadModels();
          document.getElementById('<%=UserLabel.ClientID%>').innerText = '<%=Session["UserEmail"] ?? "" %>';
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

    // Helper: Extract the Base64 part from a data URL (if needed)
    function getBase64Image(dataURL) {
      return dataURL.split(',')[1]; // Removes "data:image/png;base64," prefix
    }

    // Capture the image, perform face detection, compute its descriptor,
    // display the captured image, and store the descriptor in the hidden field.
    async function captureAndDisplay() {
      var canvas = document.getElementById('canvas');
      var video = document.getElementById('videoElement');
      var context = canvas.getContext('2d');

      // Capture the current frame onto the canvas
      context.drawImage(video, 0, 0, canvas.width, canvas.height);
      var imageDataUrl = canvas.toDataURL("image/png");
      document.getElementById('capturedImage').src = imageDataUrl;
      console.log("Captured image displayed.");

      // Convert the captured image to a Blob for processing
      const blob = await (async () => {
          // Similar to dataURItoBlob function
          var byteString;
          if (imageDataUrl.split(',')[0].indexOf('base64') >= 0)
              byteString = atob(imageDataUrl.split(',')[1]);
          else
              byteString = decodeURI(imageDataUrl.split(',')[1]);
          var mimeString = imageDataUrl.split(',')[0].split(':')[1].split(';')[0];
          var ab = new ArrayBuffer(byteString.length);
          var ia = new Uint8Array(ab);
          for (var i = 0; i < byteString.length; i++) {
              ia[i] = byteString.charCodeAt(i);
          }
          return new Blob([ia], { type: mimeString });
      })();

      // Create an image from the Blob
      const img = await faceapi.bufferToImage(blob);

      // Detect a face and compute its descriptor using the models
      const detection = await faceapi.detectSingleFace(img)
                                     .withFaceLandmarks()
                                     .withFaceDescriptor();
      if (!detection) {
        alert("No face detected. Please try again.");
        return;
      }
      console.log("Face descriptor:", detection.descriptor);

      // Serialize the descriptor as JSON and store it in the hidden field
      document.getElementById('<%=hfDescriptor.ClientID%>').value = JSON.stringify(Array.from(detection.descriptor));
      }
  </script>
</asp:Content>
