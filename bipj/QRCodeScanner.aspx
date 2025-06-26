<%@ Page Title="" Language="C#" MasterPageFile="~/Voucher.Master" AutoEventWireup="true" CodeBehind="QRCodeScanner.aspx.cs" Inherits="bipj.QRCodeScanner" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <style>
        html, body {
            height: 100%;
            margin: 0;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f4f7fa;
        }

        /* Flex container to center content vertically and horizontally */
        .flex-center {
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            padding: 20px;
        }

        /* The card container */
        .scanner-card {
            background-color: #fff;
            border-radius: 12px;
            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
            padding: 30px 40px;
            max-width: 360px;
            width: 100%;
            text-align: center;
        }

        h2 {
            margin: 0 0 25px 0;
            font-weight: 700;
            color: #2c3e50;
            letter-spacing: 1px;
            font-size: 24px;
        }

        #reader {
            margin: 0 auto 25px auto;
            width: 320px;
            max-width: 100%;
            border: 3px solid #27ae60;
            border-radius: 10px;
            box-sizing: border-box;
        }

        #scan-result {
            font-size: 18px;
            color: #27ae60;
            min-height: 28px;
            font-weight: 600;
            user-select: none;
        }

        /* Responsive tweaks */
        @media (max-width: 400px) {
            .scanner-card {
                padding: 20px;
                max-width: 90vw;
            }

            #reader {
                width: 100%;
                border-width: 2px;
            }

            h2 {
                font-size: 20px;
                margin-bottom: 20px;
            }

            #scan-result {
                font-size: 16px;
            }
        }
    </style>

    <div class="flex-center">
        <div class="scanner-card">
            <h2>Scan Your QR Code</h2>
            <div id="reader"></div>
            <div id="scan-result">Waiting for scan...</div>
        </div>
    </div>

    <script src="https://unpkg.com/html5-qrcode"></script>
    <script>
        function onScanSuccess(decodedText, decodedResult) {
            const scanResult = document.getElementById("scan-result");
            scanResult.innerText = "Scanned: " + decodedText;

            fetch("QRCodeScanner.aspx/ProcessScannedCode", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ scannedData: decodedText })
            })
                .then(res => res.json())
                .then(data => {
                    const response = data.d;
                    if (response.startsWith("http")) {
                        window.location.href = response;
                    } else {
                        alert("Server response: " + response);
                        scanResult.innerText = response;
                    }
                })
                .catch(err => {
                    alert("Error: " + err);
                });

            html5QrcodeScanner.clear(); // stop scanner after successful scan
        }

        let html5QrcodeScanner = new Html5QrcodeScanner(
            "reader", { fps: 10, qrbox: 250 }
        );

        html5QrcodeScanner.render(onScanSuccess);
    </script>

</asp:Content>
