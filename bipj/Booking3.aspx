<%@ Page Title="Book a Session – Step 3" 
    UnobtrusiveValidationMode="None"
    Language="C#" 
    MasterPageFile="~/Customer_Nav.Master" 
    AutoEventWireup="true" 
    CodeBehind="Booking3.aspx.cs" 
    Inherits="bipj.Booking3" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="head" runat="server">
  <!-- jQuery & jQuery UI -->
  <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css" />
  <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
  <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>

  <style>
    /* ── WIZARD HEADER ── */
    .wizard { text-align:center; margin:40px 0; }
    .wizard .step {
      display:inline-block; width:40px; height:40px; line-height:40px;
      border-radius:50%; background:#eee; color:#777; margin:0 10px;
      font-weight:bold; position:relative;
    }
    .wizard .step.completed,
    .wizard .step.active { background:#433e8e; color:#fff; }
    .wizard .step:after {
      content:''; position:absolute; top:50%; right:-30px;
      width:60px; height:4px; background:#eee;
      transform:translateY(-50%); z-index:-1;
    }
    .wizard .step.completed:after,
    .wizard .step.active:after { background:#433e8e; }
    .wizard .step:last-child:after { display:none; }

    /* ── STEP PANEL ── */
    .step-panel { display:none; background:#f5f3ff; border-radius:12px;
                  padding:32px; margin:0 auto 40px; max-width:800px; }
    .step-panel.active { display:block; }

    /* ── CALENDAR & TIMES ── */
    .calendar-times { display:flex; gap:32px; flex-wrap:wrap; justify-content:center; }
    #datepicker { background:#fff; padding:10px; border-radius:6px; }
    .timeslots {
      display:grid; grid-template-columns:repeat(auto-fill,minmax(80px,1fr));
      gap:12px; max-width:340px;
    }
    .timeslots button {
      padding:8px; border:none; border-radius:6px; background:#eee; cursor:pointer;
    }
    .timeslots button.selected { background:#433e8e; color:#fff; }
    .legend { display:flex; gap:16px; margin-top:12px; font-size:.85rem; }
    .legend span { display:flex; align-items:center; gap:4px; }
    .legend .avail { width:12px;height:12px;background:#eee;border-radius:2px }
    .legend .sel   { width:12px;height:12px;background:#433e8e;border-radius:2px }

    /* ── NAV BUTTONS ── */
    .btn-prev, .btn-next {
      background:#433e8e; color:#fff; border:none; border-radius:6px;
      padding:10px 24px; cursor:pointer; font-size:1rem; margin-top:20px;
    }
    .btn-prev { background:#aaa; margin-right:10px; }
  </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
  <h2 style="text-align:center; color:#3b3350; margin-top:24px;">
    Book a Session — Step 3: Date &amp; Time
  </h2>

  <!-- Wizard Header -->
  <div class="wizard">
    <div class="step completed" data-step="1">1</div>
    <div class="step completed" data-step="2">2</div>
    <div class="step active" data-step="3">3</div>
    <div class="step" data-step="4">4</div>
    <div class="step" data-step="5">5</div>
  </div>

  <!-- STEP 3 PANEL -->
  <div id="step3" class="step-panel active">
    <!-- hidden storage for date/time -->
    <asp:HiddenField ID="hfDate" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hfTime" runat="server" ClientIDMode="Static" />

    <div class="calendar-times">
      <!-- 1) Datepicker -->
      <div id="datepicker"></div>

      <!-- 2) Time slots -->
      <div>
        <div class="timeslots" id="timeslots"></div>
        <div class="legend">
          <span><i class="avail"></i> Available</span>
          <span><i class="sel"></i> Selected</span>
        </div>
      </div>
    </div>

    <asp:Button ID="btnBack" runat="server"
        Text="Back" CssClass="btn-prev"
        OnClick="btnBack_Click" />

    <asp:Button ID="btnNext" runat="server"
        Text="Next" CssClass="btn-next"
        OnClick="btnNext_Click"
        Enabled="false"
        ClientIDMode="Static" />
  </div>
</asp:Content>

<asp:Content ID="Scripts" ContentPlaceHolderID="scripts" runat="server">
  <script>
      // Booking state passed via session or window.name
      let booking = {
          advisorId: sessionStorage.getItem('advisorId'),
          date: null,
          time: null
      };

      // Init datepicker
      $(function () {
          $("#datepicker").datepicker({
              dateFormat: "yy-mm-dd",
              minDate: 0,
              onSelect: function (d) {
                  booking.date = d;
                  loadSlots();
                  $("#btnNext").prop("disabled", true);
              }
          });
      });

      // Fetch booked slots, render buttons
      function loadSlots() {
          fetch(`Booking3.aspx/GetBookedSlots`, {
              method: 'POST',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({
                  advisorId: +booking.advisorId,
                  date: booking.date
              })
          })
              .then(r => r.json()).then(data => {
                  const taken = data.d;
                  const container = $("#timeslots").empty();
                  ["09:00", "10:00", "11:00", "13:00", "14:00", "15:00"].forEach(t => {
                      const btn = $(`<button type="button">${t}</button>`)
                          .prop("disabled", taken.includes(t))
                          .on("click", () => {
                              $(".timeslots button").removeClass("selected");
                              btn.addClass("selected");
                              booking.time = t;
                              // persist in hidden fields
                              document.getElementById('hfDate').value = booking.date;
                              document.getElementById('hfTime').value = booking.time;
                              $("#btnNext").prop("disabled", false);
                          });
                      container.append(btn);
                  });
              });
      }
  </script>
</asp:Content>
