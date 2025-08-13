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
      <asp:Label ID="UserLabel" runat="server" CssClass="h5 mb-3" Text=""></asp:Label>

      <!-- Start camera -->
      <input type="button" value="Access Camera" onclick="startCamera();" class="btn btn-primary mb-3" id="btnCamera" />
      
      <!-- Camera loading indicator -->
      <div id="cameraLoading" class="d-none text-center">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading camera...</span>
        </div>
        <p>Initializing camera...</p>
      </div>
      
      <!-- Live video -->
      <video id="videoElement" width="640" height="480" autoplay playsinline style="border: 1px solid #ccc;" class="d-none"></video>
      <br /><br />
      
      <!-- Hidden canvas (used only to grab a frame) -->
      <canvas id="canvas" width="640" height="480" style="display: none;"></canvas>
      
      <!-- Capture button (no preview; will auto-enroll) -->
      <input type="button" value="Capture Face" 
             onclick="captureAndEnroll();" 
             class="btn btn-warning" 
             id="btnCapture" 
             disabled />
      <br /><br />
      
      <!-- No snapshot preview image anymore -->

      <!-- Hidden field to store the descriptor -->
      <asp:HiddenField ID="hfDescriptor" runat="server" />
      
      <!-- Enroll button (clicked programmatically after capture) -->
      <asp:Button ID="btnEnroll" runat="server" Text="Enroll Facial Data" OnClick="btnEnroll_Click" CssClass="btn btn-success d-none" />
      <br /><br />
      
      <asp:Label ID="lblMessage" runat="server" CssClass="alert alert-info w-100 text-center"></asp:Label>
    </div>
  </div>

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

          const msg = document.getElementById('<%=lblMessage.ClientID%>');
        if (!modelsLoaded) {
            msg.textContent = "Failed to load facial recognition models. Try refreshing the page.";
            msg.className = "alert alert-danger";
            alert("Failed to load facial recognition models. Please check console for details.");
        } else {
            msg.textContent = "Facial recognition ready! Click 'Access Camera' to start.";
            msg.className = "alert alert-success";
            // Show who is enrolling
            document.getElementById('<%=UserLabel.ClientID%>').innerText = '<%=Session["UserEmail"] ?? "" %>';
          }
      }

      function startCamera() {
          if (!modelsLoaded) {
              alert("Models not loaded yet. Please wait...");
              return;
          }

          const msgEl = document.getElementById('<%=lblMessage.ClientID%>');
      msgEl.textContent = "Opening camera…";
      msgEl.className = "alert alert-warning";

      document.getElementById('cameraLoading').classList.remove('d-none');
      const video = document.getElementById('videoElement');

      // Clear any previous stream
      if (video.srcObject) {
        video.srcObject.getTracks().forEach(track => track.stop());
        video.srcObject = null;
      }

      if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: { facingMode: "user" } })
          .then(stream => {
            video.srcObject = stream;
            video.onloadedmetadata = function () {
              document.getElementById('cameraLoading').classList.add('d-none');
              video.classList.remove('d-none');
            };
            video.play().then(() => {
              document.getElementById('btnCapture').disabled = false;
              msgEl.textContent = "Align your face in the frame, then click 'Capture Face'.";
              msgEl.className = "alert alert-info";
            }).catch(e => {
              console.error("Video play error:", e);
              document.getElementById('cameraLoading').classList.add('d-none');
              msgEl.textContent = "Error starting video: " + e.message;
              msgEl.className = "alert alert-danger";
            });
          })
          .catch(error => {
            console.error("Error accessing camera:", error);
            document.getElementById('cameraLoading').classList.add('d-none');
            alert("Unable to access camera: " + error.message);
          });
      } else {
        alert("Your browser does not support camera access.");
      }
    }

    function stopCamera() {
      const video = document.getElementById('videoElement');
      if (video && video.srcObject) {
        video.srcObject.getTracks().forEach(t => t.stop());
        video.srcObject = null;
      }
      if (video) video.classList.add('d-none');
    }

    // Capture (no preview) and auto-enroll
    async function captureAndEnroll() {
      if (!modelsLoaded) {
        alert("Facial models are still loading. Please wait and try again.");
        return;
      }

      const video = document.getElementById('videoElement');
      const canvas = document.getElementById('canvas');
      const context = canvas.getContext('2d');

      // Capture current frame
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

        // Save descriptor and submit
        document.getElementById('<%=hfDescriptor.ClientID%>').value =
          JSON.stringify(Array.from(detection.descriptor));

        // Stop camera before submitting
        stopCamera();

        // Programmatically enroll
        document.getElementById('<%=btnEnroll.ClientID%>').click();
      } catch (e) {
        console.error("Face detection error:", e);
        alert("Error processing facial data: " + e.message);
      }
    }

    // Page load
    window.addEventListener('load', async () => {
      const msgElement = document.getElementById('<%=lblMessage.ClientID%>');
        msgElement.textContent = "Loading facial recognition models...";
        msgElement.className = "alert alert-warning";
        await loadModelsWithRetry();
    });
  </script>
</asp:Content>
