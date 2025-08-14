<%@ Page Title="Facial Login" 
    Language="C#" 
    MasterPageFile="~/Customer_Nav_loggedin.master" 
    AutoEventWireup="true" 
    CodeBehind="FacialLogin.aspx.cs" 
    Inherits="badpjProject.FacialLogin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <div class="container mt-5">
    <div class="d-flex flex-column align-items-center">
      <h2>Facial Login</h2>

      <!-- Email -->
      <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control mb-3" 
                   placeholder="Enter Email" Width="320px"></asp:TextBox>
      
      <!-- Start camera -->
      <input type="button" value="Access Camera" onclick="startCamera();" 
             class="btn btn-primary mb-3" id="btnCamera" />
      
      <!-- Camera loading indicator -->
      <div id="cameraLoading" class="d-none text-center">
          <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading camera...</span>
          </div>
          <p>Initializing camera...</p>
      </div>
      
      <!-- Live video -->
      <video id="videoElement" width="640" height="480" autoplay playsinline 
             style="border:1px solid #ccc;" class="d-none"></video>
      <br /><br />
      
      <!-- Hidden canvas (no preview shown) -->
      <canvas id="canvas" width="640" height="480" style="display:none;"></canvas>
      
      <!-- Capture button (no preview; auto-submit after capture) -->
      <input type="button" value="Capture Face" 
             onclick="captureAndDisplay();" class="btn btn-warning" 
             id="btnCapture" disabled />
      <br /><br />
      
      <!-- Hidden field for descriptor -->
      <asp:HiddenField ID="hfDescriptor" runat="server" />
      
      <!-- Submit button (clicked programmatically after capture) -->
      <asp:Button ID="btnLogin" runat="server" Text="Login via Face" 
                  OnClick="btnLogin_Click" CssClass="btn btn-success d-none" />
      <br /><br />
      
      <!-- Status -->
      <asp:Label ID="lblResult" runat="server" CssClass="alert alert-info w-100 text-center"></asp:Label>
    </div>
  </div>

  <!-- jQuery and face-api.js -->
  <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/face-api.js@0.22.2/dist/face-api.min.js"></script>
  <script type="text/javascript">
      let modelsLoaded = false;
      let loadingAttempts = 0;
      const MAX_ATTEMPTS = 5;

      $.ajaxSetup({ xhrFields: { withCredentials: true } });

      async function loadModels() {
          try {
              const modelUrl = 'https://raw.githubusercontent.com/justadudewhohacks/face-api.js/master/weights/';
              await Promise.all([
                  faceapi.nets.ssdMobilenetv1.loadFromUri(modelUrl),
                  faceapi.nets.faceLandmark68Net.loadFromUri(modelUrl),
                  faceapi.nets.faceRecognitionNet.loadFromUri(modelUrl)
              ]);
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
              if (!success) await new Promise(resolve => setTimeout(resolve, 5000));
          }

          const msg = document.getElementById('<%=lblResult.ClientID%>');
          if (!modelsLoaded) {
              msg.textContent = "Failed to load facial recognition models. Try refreshing the page.";
              msg.className = "alert alert-danger";
              alert("Failed to load facial recognition models. Please check console for details.");
          } else {
              msg.textContent = "Facial recognition ready! Enter email and click 'Access Camera'.";
              msg.className = "alert alert-success";
          }
      }

      function startCamera() {
          // Require email first
          const emailBox = document.getElementById('<%=txtEmail.ClientID%>');
          const emailVal = (emailBox.value || '').trim();
          if (!emailVal) {
              alert("Please enter your email first.");
              emailBox.focus();
              return;
          }

          if (!modelsLoaded) {
              alert("Models not loaded yet. Please wait...");
              return;
          }

          document.getElementById('cameraLoading').classList.remove('d-none');
          const video = document.getElementById('videoElement');

          // Clear any previous stream
          if (video.srcObject) {
              video.srcObject.getTracks().forEach(track => track.stop());
              video.srcObject = null;
          }

          if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
              navigator.mediaDevices.getUserMedia({ video: { facingMode: "user" } })
                  .then(function (stream) {
                      video.srcObject = stream;
                      video.onloadedmetadata = function () {
                          document.getElementById('cameraLoading').classList.add('d-none');
                          video.classList.remove('d-none');
                      };
                      video.play()
                          .then(() => {
                              // Enable capture button
                              document.getElementById('btnCapture').disabled = false;
                              const msg = document.getElementById('<%=lblResult.ClientID%>');
                          msg.textContent = "Align face in the frame, then click 'Capture Face'.";
                          msg.className = "alert alert-info";
                      })
                      .catch(e => {
                          console.error("Video play error:", e);
                          document.getElementById('cameraLoading').classList.add('d-none');
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

      // Capture (no preview) and submit
      async function captureAndDisplay() {
          if (!modelsLoaded) {
              alert("Facial models are still loading. Please wait and try again.");
              return;
          }
          
          const video = document.getElementById('videoElement');
          const canvas = document.getElementById('canvas');
          const context = canvas.getContext('2d');

          // Capture frame to canvas
          context.drawImage(video, 0, 0, canvas.width, canvas.height);

          // Convert to data URL -> Blob
          const imageDataUrl = canvas.toDataURL("image/png");
          const blob = await (async () => {
              const byteString = atob(imageDataUrl.split(',')[1]);
              const mimeString = imageDataUrl.split(',')[0].split(':')[1].split(';')[0];
              const ab = new ArrayBuffer(byteString.length);
              const ia = new Uint8Array(ab);
              for (let i = 0; i < byteString.length; i++) ia[i] = byteString.charCodeAt(i);
              return new Blob([ia], { type: mimeString });
          })();

          try {
              const img = await faceapi.bufferToImage(blob);
              const detection = await faceapi
                    .detectSingleFace(img)
                    .withFaceLandmarks()
                    .withFaceDescriptor();
                  
              if (!detection) {
                  alert("No face detected. Please try again.");
                  return;
              }
              
              // Put descriptor in hidden field
              document.getElementById('<%=hfDescriptor.ClientID%>').value = 
                  JSON.stringify(Array.from(detection.descriptor));

              // Stop camera before submitting
              if (video.srcObject) {
                  video.srcObject.getTracks().forEach(t => t.stop());
                  video.srcObject = null;
              }
              video.classList.add('d-none');

              // Auto-submit for verification (2-step flow)
              document.getElementById('<%=btnLogin.ClientID%>').click();
          } catch (e) {
              console.error("Face detection error:", e);
              alert("Error processing facial data: " + e.message);
          }
      }

      window.addEventListener('load', async () => {
          const msgElement = document.getElementById('<%=lblResult.ClientID%>');
          msgElement.textContent = "Loading facial recognition models...";
          msgElement.className = "alert alert-warning";
          await loadModelsWithRetry();
      });
  </script>
</asp:Content>
