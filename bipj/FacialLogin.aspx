<%@ Page Title="Facial Login" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav.master" 
    AutoEventWireup="true" 
    CodeBehind="FacialLogin.aspx.cs" 
    Inherits="badpjProject.FacialLogin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <div class="container mt-5">
    <div class="d-flex flex-column align-items-center">
      <h2>Facial Login</h2>
      <!-- Textbox for the user to enter their email -->
      <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control mb-3" 
                   placeholder="Enter Email"></asp:TextBox>
      
      <!-- Button to start the camera -->
      <input type="button" value="Access Camera" onclick="startCamera();" 
             class="btn btn-primary mb-3" id="btnCamera" />
      
      <!-- Camera loading indicator -->
      <div id="cameraLoading" class="d-none text-center">
          <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading camera...</span>
          </div>
          <p>Initializing camera...</p>
      </div>
      
      <!-- Video element to display the live camera feed -->
      <video id="videoElement" width="640" height="480" autoplay playsinline 
             style="border:1px solid #ccc;" class="d-none"></video>
      <br /><br />
      
      <!-- Hidden canvas for capturing the image -->
      <canvas id="canvas" width="640" height="480" style="display:none;"></canvas>
      
      <!-- Button to capture & display the facial data -->
      <input type="button" value="Capture & Display Facial Data" 
             onclick="captureAndDisplay();" class="btn btn-warning" 
             id="btnCapture" disabled />
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
      let modelsLoaded = false;
      let loadingAttempts = 0;
      const MAX_ATTEMPTS = 5;

      // Set up global AJAX settings
      $.ajaxSetup({
          xhrFields: { withCredentials: true }
      });

      async function loadModels() {
          console.log("Trying CDN models...");
          try {
              // Use official face-api.js CDN
              const modelUrl = 'https://raw.githubusercontent.com/justadudewhohacks/face-api.js/master/weights/';
              await Promise.all([
                  faceapi.nets.ssdMobilenetv1.loadFromUri(modelUrl),
                  faceapi.nets.faceLandmark68Net.loadFromUri(modelUrl),
                  faceapi.nets.faceRecognitionNet.loadFromUri(modelUrl)
              ]);
              console.log("CDN models loaded successfully!");
              modelsLoaded = true;
              return true;
          } catch (e) {
              console.error("CDN model loading failed:", e);
              return false;
          }
      }

      async function loadModelsWithRetry() {
          while (!modelsLoaded && loadingAttempts < MAX_ATTEMPTS) {
              loadingAttempts++;
              const success = await loadModels();
              if (!success) {
                  // Wait 5 seconds before retrying
                  await new Promise(resolve => setTimeout(resolve, 5000));
              }
          }

          if (!modelsLoaded) {
              console.error("Failed to load models after", MAX_ATTEMPTS, "attempts");
              alert("Failed to load facial recognition models. Please check console for details");
          } else {
              console.log("Models are ready!");
              // Update UI if needed
              document.getElementById('<%=lblResult.ClientID%>').textContent = "Facial recognition ready! Enter email and click 'Access Camera'.";
              document.getElementById('<%=lblResult.ClientID%>').className = "alert alert-success";
          }
      }

      // Start camera function
      function startCamera() {
          console.log("Attempting to start camera...");
          if (!modelsLoaded) {
              alert("Models not loaded yet. Please wait...");
              return;
          }
          
          // Show loading indicator
          document.getElementById('cameraLoading').classList.remove('d-none');
          
          var video = document.getElementById('videoElement');
          
          // Clear any previous stream
          if (video.srcObject) {
              video.srcObject.getTracks().forEach(track => track.stop());
              video.srcObject = null;
          }

          if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
              navigator.mediaDevices.getUserMedia({ 
                  video: { 
                      facingMode: "user" // Prefer front camera
                  } 
              })
              .then(function (stream) {
                  console.log("Camera access granted");
                  video.srcObject = stream;
                  
                  video.onloadedmetadata = function() {
                      document.getElementById('cameraLoading').classList.add('d-none');
                      video.classList.remove('d-none');
                  };
                  
                  video.play()
                      .then(() => {
                          console.log("Video playing");
                          // Enable capture button
                          document.getElementById('btnCapture').disabled = false;
                      })
                      .catch(e => {
                          console.error("Video play error:", e);
                          alert("Error starting video: " + e.message);
                      });
              })
              .catch(function (error) {
                  console.error("Error accessing camera:", error);
                  document.getElementById('cameraLoading').classList.add('d-none');
                  alert("Unable to access camera: " + error.message);
              });
          } else {
              alert("Your browser does not support camera access.");
          }
      }

      // Capture and display function
      async function captureAndDisplay() {
          if (!modelsLoaded) {
              alert("Facial models are still loading. Please wait and try again.");
              return;
          }
          
          console.log("Attempting capture...");
          var video = document.getElementById('videoElement');
          var canvas = document.getElementById('canvas');
          var context = canvas.getContext('2d');

          // Capture frame
          context.drawImage(video, 0, 0, canvas.width, canvas.height);
          var imageDataUrl = canvas.toDataURL("image/png");
          document.getElementById('capturedImage').src = imageDataUrl;
          console.log("Captured image displayed.");

          // Convert to Blob
          const blob = await (async () => {
              var byteString = atob(imageDataUrl.split(',')[1]);
              var mimeString = imageDataUrl.split(',')[0].split(':')[1].split(';')[0];
              var ab = new ArrayBuffer(byteString.length);
              var ia = new Uint8Array(ab);
              for (var i = 0; i < byteString.length; i++) {
                  ia[i] = byteString.charCodeAt(i);
              }
              return new Blob([ia], { type: mimeString });
          })();

          try {
              const img = await faceapi.bufferToImage(blob);
              const detection = await faceapi.detectSingleFace(img)
                  .withFaceLandmarks()
                  .withFaceDescriptor();
                  
              if (!detection) {
                  alert("No face detected. Please try again.");
                  return;
              }
              
              // Serialize the descriptor and store in hidden field
              document.getElementById('<%=hfDescriptor.ClientID%>').value = 
                  JSON.stringify(Array.from(detection.descriptor));
              console.log("Face descriptor captured");
          } catch (e) {
              console.error("Face detection error:", e);
              alert("Error processing facial data: " + e.message);
          }
      }

      // Page load handler
      window.addEventListener('load', async () => {
          console.log("Page loaded - starting model load");
          const msgElement = document.getElementById('<%=lblResult.ClientID%>');
          msgElement.textContent = "Loading facial recognition models...";
          msgElement.className = "alert alert-warning";

          await loadModelsWithRetry();
      });
  </script>
</asp:Content>